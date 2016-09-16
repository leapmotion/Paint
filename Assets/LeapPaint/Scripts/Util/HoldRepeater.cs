using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class HoldRepeater : MonoBehaviour {

  public float _waitBeforeActivation = 1F;
  public float _timeBetweenEvents = 0.6F;

  public UnityEvent OnRepeatedEventFired;

  private float _timer = 0F;

  private IEnumerator _timerCoroutine;

  protected void Start() {
  }

  public void StartRepeatTimer() {
    _timer = 0F;
    _timerCoroutine = DoRepeatTiming();
    StartCoroutine(_timerCoroutine);
  }

  public void StopRepeatTimer() {
    StopCoroutine(_timerCoroutine);
  }

  private IEnumerator DoRepeatTiming() {
    float period = 0.1F;
    while (_timer < _waitBeforeActivation) {
      yield return new WaitForSecondsRealtime(period);
      _timer += period;
    }
    _timer = 0F;
    while (true) {
      yield return new WaitForSecondsRealtime(period);
      _timer += period;
      if (_timer > _timeBetweenEvents) {
        _timer = 0F;
        OnRepeatedEventFired.Invoke();
      }
    }
  }

}
