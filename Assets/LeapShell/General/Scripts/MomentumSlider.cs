using UnityEngine;
using System;
using System.Collections.Generic;
using Leap;
using Leap.Unity;
using Leap.Unity.Attributes;

[Serializable]
public class MomentumSlider {
  public event Action<Hand, float> OnConnect;
  public event Action OnDisconnect;
  public event Action OnStationary;
  public event Action<float> OnPosition;

  [SerializeField]
  private float _position = 0;

  [SerializeField]
  private float _activationDistance;

  [SerializeField]
  private float _deactivationDistance;

  [MinValue(0)]
  [Units("Seconds")]
  [SerializeField]
  private float _velocitySmoothing = 0.02f;

  [Tooltip("Multiplier to affect how much velocity affects whether or not to change to the next position. " +
           "At zero, velocity has no effect.")]
  [MinValue(0)]
  [SerializeField]
  private float _velocityBiasMultiplier = 1;

  [MinValue(0)]
  [Units("Seconds")]
  [SerializeField]
  private float _restDuration = 0.3f;

  public bool IsConnected {
    get {
      return _connectedHand != -1;
    }
  }

  public float Velocity {
    get {
      if (_smoothedVelocity != null) {
        return _smoothedVelocity.value;
      } else {
        return 0;
      }
    }
  }

  public float ActivationDistance {
    get {
      return _activationDistance;
    }
  }

  public float DeactivationDistance {
    get {
      return _deactivationDistance;
    }
  }

  public bool IsStationary {
    get {
      return !IsConnected && _disconnectTime == 0;
    }
  }

  public float Position {
    get {
      return _position;
    }
  }

  public Func<Hand, bool> CanHandInteract = hand => true;
  public Func<Hand, bool> CanConnect = hand => true;
  public Func<float, float> GetRestPosition;
  public Func<Hand, float> HandToPosition = null;
  public Func<Hand, float> HandToDistance = null;
  public Func<float, float> ConstrainOffset = o => o;

  private SmoothedFloat _smoothedVelocity;

  private int _connectedHand = -1;
  private float _connectOffset = 0;
  private float _connectPosition = 0;

  private float _disconnectTime = 0;
  private float _disconnectPosition;
  private float _disconnectVelocity;
  private float _restPosition;

  public void ResetSlider(float position) {
    _position = position;
    if (OnPosition != null) {
      OnPosition(position);
    }

    _connectedHand = -1;
    _disconnectTime = 0;
  }

  public void Update(List<Hand> hands) {
    if (_smoothedVelocity == null) {
      _smoothedVelocity = new SmoothedFloat();
      _smoothedVelocity.delay = _velocitySmoothing;
    }

    if (GetRestPosition == null) {
      GetRestPosition = defaultGetRestPosition;
    }

    if (_connectedHand == -1) {
      float delta = (Time.time - _disconnectTime) / _restDuration;
      if (delta <= 1) {
        _position = hermiteSpline(_disconnectVelocity, 0, _disconnectPosition, _restPosition, delta);
        if (OnPosition != null) OnPosition(_position);
      } else if (_disconnectTime != 0) {
        _position = _restPosition;
        if (OnPosition != null) OnPosition(_position);
        _disconnectTime = 0;

        if (OnStationary != null) {
          OnStationary();
        }
      }

      for (int i = 0; i < hands.Count; i++) {
        Hand hand = hands[i];
        if (!CanHandInteract(hand)) {
          continue;
        }

        if (!CanConnect(hand)) {
          continue;
        }

        float distance = HandToDistance(hand);
        if (distance < _activationDistance) {
          Connect(hand);
        }
      }
    } else {
      Hand hand = null;
      for (int i = 0; i < hands.Count; i++) {
        if (hands[i].Id == _connectedHand) {
          hand = hands[i];
          continue;
        }
      }

      if (hand == null) {
        Disconnect();
        return;
      }

      if (!CanHandInteract(hand)) {
        Disconnect();
        return;
      }

      float distance = HandToDistance(hand);
      if (distance > _deactivationDistance) {
        Disconnect();
        return;
      }

      float handPosition = HandToPosition(hand);
      float offset = handPosition + _connectOffset - _connectPosition;
      offset = ConstrainOffset(offset);
      float newPosition = _connectPosition + offset;

      float instantVel = (newPosition - _position) / Time.deltaTime;
      _smoothedVelocity.Update(instantVel, Time.deltaTime);

      _position = newPosition;

      if (OnPosition != null) OnPosition(_position);
    }
  }

  protected virtual void Connect(Hand hand) {
    if (OnConnect != null) OnConnect(hand, HandToPosition(hand));

    _connectOffset = _position - HandToPosition(hand);
    _connectPosition = _position;
    _connectedHand = hand.Id;
    _smoothedVelocity.reset = true;
  }

  protected virtual void Disconnect() {
    if (OnDisconnect != null) OnDisconnect();

    _disconnectTime = Time.time;
    _disconnectPosition = _position;
    _connectedHand = -1;

    _restPosition = GetRestPosition(getIntegerRestPosition());

    _disconnectVelocity = _smoothedVelocity.value * _restDuration;
  }

  private float hermiteSpline(float startVel, float endVel, float startPos, float endPos, float t) {
    float t2 = t * t;
    float t3 = t2 * t;
    return (2 * t3 - 3 * t2 + 1) * startPos + (t3 - 2 * t2 + t) * startVel + (-2 * t3 + 3 * t2) * endPos + (t3 - t2) * endVel;
  }

  private float getIntegerRestPosition() {
    int sign = _smoothedVelocity.value >= 0 ? 1 : -1;

    float mockVel = _smoothedVelocity.value * sign * _restDuration;
    float mockPos = _position * sign;

    int backIndex = Mathf.FloorToInt(mockPos);
    int nextIndex = backIndex + 1;
    float max;
    bool didReverse;

    simulateHermite(backIndex, mockPos, mockVel * _velocityBiasMultiplier, out max, out didReverse);

    if (max < (nextIndex + backIndex) * 0.5f) {
      return backIndex * sign;
    }

    int bail = 100;
    for (int i = nextIndex; ; i++) {
      if (bail-- < 0) throw new Exception("Bailing");
      simulateHermite(i, mockPos, 0 * mockVel, out max, out didReverse);
      if ((max - i) > 0.5f || didReverse) {
        continue;
      } else {
        return i * sign;
      }
    }
  }

  private void simulateHermite(float mockRest, float mockPos, float mockVel, out float max, out bool didReverse) {
    max = mockPos;

    bool didReachMin = false;
    didReverse = false;
    for (float f = 0; f <= 1; f += 0.1f) {
      float p = hermiteSpline(mockVel, 0, mockPos, mockRest, f);

      if (p > max) {
        didReachMin = true;
        max = p;
      } else if (didReachMin) {
        didReverse = true;
      }
    }
  }

  private float defaultGetRestPosition(float currPos) {
    return Mathf.Clamp01(Mathf.Round(currPos));
  }

}
