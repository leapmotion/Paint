using Leap.Unity.Gestures;
using Leap.Unity.Timers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Leap.Unity.Paint {

  [System.Serializable]
  public class Vector3Event : UnityEvent<Vector3> { }

  public class PinchFingertipGesture : TwoHandedGesture {

    /// <summary>
    /// Whether the gesture should deactivate on the next frame after activation
    /// or deactivate when the activation pinch ends.
    /// </summary>
    public enum EndGestureMode {
      /// <summary>
      /// Indicates the gesture should deactivate when the activating pinch ends.
      /// </summary>
      OnPinchEnd,

      /// <summary>
      /// Indicates the gesture should end on the next frame after activation.
      /// </summary>
      Immediately,
    }
    public EndGestureMode endGestureMode = EndGestureMode.OnPinchEnd;

    public Chirality pinchingHandHandedness = Chirality.Right;
    public Finger.FingerType otherHandFinger = Finger.FingerType.TYPE_INDEX;
    
    [Tooltip("The maximum distance the pinch must occur from the fingertip to activate "
           + "the gesture.")]
    public float maxFingertipDistance = 0.02F;

    public float activationCooldownTime = 0.2F;
    private Timer _backingCooldown;
    private Timer _cooldown {
      get {
        if (_backingCooldown == null) {
          _backingCooldown = Pool<Timer>.Spawn().Type(TimerType.CountDown)
                                                .ResetTime(activationCooldownTime);
        }
        return _backingCooldown;
      }
    }

    public Hand pinchHand {
      get { return pinchingHandHandedness == Chirality.Left ? leftHand : rightHand; }
    }

    public Hand otherHand {
      get { return pinchingHandHandedness == Chirality.Left ? rightHand : leftHand; }
    }

    private bool _handPinchingLastFrame = false;
    public bool pinchingHandJustPinched {
      get {
        return pinchHand.IsPinching() && !_handPinchingLastFrame;
      }
    }

    private Vector3 _pinchPosition {
      get { return pinchHand.GetPredictedPinchPosition();  }
    }

    private Vector3 _otherFingertipPosition {
      get { return otherHand.Fingers[(int)otherHandFinger].TipPosition.ToVector3(); }
    }

    public Vector3Event WhilePinchActive = new Vector3Event();

    protected override void Update() {
      base.Update();

      _handPinchingLastFrame = pinchHand.IsPinching();
    }

    protected virtual void OnDestroy() {
      if (_backingCooldown != null && Application.isPlaying) {
        _backingCooldown.Dispose();
        Pool<Timer>.Recycle(_backingCooldown);
        _backingCooldown = null;
      }
    }

    protected override bool ShouldGestureActivate(Hand leftHand, Hand rightHand) {
      return pinchingHandJustPinched
          && Vector3.SqrMagnitude(_pinchPosition - _otherFingertipPosition)
               < maxFingertipDistance * maxFingertipDistance;
    }

    protected override bool ShouldGestureDeactivate(Hand leftHand, Hand rightHand,
                                                    out Gesture.DeactivationReason? deactivationReason) {
      deactivationReason = Gesture.DeactivationReason.FinishedGesture;
      return true;
    }

    #region Gizmos

#if UNITY_EDITOR
    public Color gizmoColor = Color.Lerp(Color.green, Color.yellow, 0.4F);

    void OnDrawGizmosSelected() {
      // Fingertip target.
      Gizmos.color = gizmoColor.WithAlpha(0.3F);
      Gizmos.DrawSphere(_otherFingertipPosition, maxFingertipDistance);

      // Pinch position.
      Gizmos.color = gizmoColor.WithAlpha(0.8F);
      Gizmos.DrawSphere(_pinchPosition, pinchHand.Fingers[1].Width);
    }
#endif

    #endregion

  }

}