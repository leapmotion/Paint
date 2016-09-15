using Leap.Unity.Attributes;
using System;
using System.Collections;
using UnityEngine;

public class Pulsator : MonoBehaviour {

  [MinValue(0.01F)]
  public float _speed = 1F;
  public float _restValue = 0F;
  public float _pulseValue = 1F;
  public float _holdValue = 0.5F;

  [SerializeField]
  private float _value;
  [SerializeField]
  private TweenHandle _valueTween;
  [SerializeField]
  private bool _pulsing = false;
  [SerializeField]
  private bool _awaitingRelease = false;
  [SerializeField]
  private bool _releasing = false;
  public bool IsReleasing { get { return _releasing; } }
  public bool AtRest { get { return _value == _restValue; } }

  public float Value { get { return _value; } }
  public Action<float> OnValueChanged = (x) => { };

  private void OnTweenValue(float value) {
    _value = value;
    OnValueChanged(_value);
  }

  private void TweenTo(float to, Action OnReachEnd, float speedMultiplier) {
    if (_valueTween.IsValid) {
      _valueTween.Release();
    }
    _valueTween = MakeTween(to, OnReachEnd, speedMultiplier);
    _valueTween.Progress = 0F;
    _valueTween.Play();
  }

  private TweenHandle MakeTween(float to, Action OnReachEnd, float speedMultiplier) {
    return Tween.Value(_value, to, OnTweenValue)
      .Smooth(TweenType.SMOOTH)
      .AtRate(_speed * speedMultiplier)
      .OnReachEnd(OnReachEnd)
      .Keep();
  }

  public void Activate() {
    if (!_pulsing) {
      StartPulse();
    }
  }

  public void Release() {
    if (_pulsing) {
      _awaitingRelease = true;
    }
    else {
      TweenTo(_restValue, () => { _releasing = false; Debug.Log("Aaand releasing is now false."); }, 1F);
      _awaitingRelease = false;
      _releasing = true;
      Debug.Log("OK! Releasing." + _releasing);
    }
  }

  public void WarmUp() {
    if (!_pulsing && !_awaitingRelease && !_releasing) {
      TweenTo(_holdValue, () => { }, 0.75F);
    }
  }

  private void StartPulse() {
    TweenTo(_pulseValue, DoOnReachedPulsePeak, 3F);
    _pulsing = true;
  }

  private void DoOnReachedPulsePeak() {
    _pulsing = false;
    StartCoroutine(OnNextFrameTweenToHoldValue());
  }

  private IEnumerator OnNextFrameTweenToHoldValue() {
    yield return new WaitForEndOfFrame();
    if (!_awaitingRelease) {
      TweenTo(_holdValue, () => { }, 1F);
    }
    else {
      TweenTo(_restValue, () => { _releasing = false; }, 1F);
      _releasing = true;
      _awaitingRelease = false;
    }
  }

}
