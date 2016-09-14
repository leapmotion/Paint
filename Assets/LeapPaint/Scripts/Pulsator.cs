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

  private float _value;
  private TweenHandle _valueTween;
  private bool _pulsing = false;
  private bool _awaitingRelease = false;

  public float Value { get { return _value; } }
  public Action<float> OnValueChanged = (x) => { };

  private void OnTweenValue(float value) {
    _value = value;
    OnValueChanged(_value);
  }

  private void TweenTo(float to, Action OnReachEnd, float speedMultiplier) {
    _pulsing = false;
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

  private void StartPulse() {
    TweenTo(_pulseValue, DoOnReachedPulsePeak, 3F);
    _pulsing = true;
  }

  private void DoOnReachedPulsePeak() {
    StartCoroutine(OnNextFrameTweenToHoldValue());
  }

  private IEnumerator OnNextFrameTweenToHoldValue() {
    yield return new WaitForEndOfFrame();
    if (!_awaitingRelease) {
      TweenTo(_holdValue, () => { }, 1F);
    }
    else {
      TweenTo(_restValue, () => { }, 1F);
      _awaitingRelease = false;
    }
  }

  public void Release() {
    if (_pulsing) {
      _awaitingRelease = true;
    }
    else {
      TweenTo(_restValue, () => { }, 1F);
      _awaitingRelease = false;
    }
  }

}
