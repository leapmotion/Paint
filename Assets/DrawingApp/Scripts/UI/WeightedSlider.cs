using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class WeightedSlider : Slider {

  private bool _beingHandled = false;
  private float _handleVelocity = 0F;
  private const float _handleAcceleration = 50F;
  private bool _readyForCallback = true;

  public UnityEvent OnToggleOn;
  public UnityEvent OnToggleOff;

  protected virtual void Update() {
    if (!_beingHandled) {
      int direction = (base.value < 0.5 ? -1 : 1);
      _handleVelocity += _handleAcceleration * direction * Time.deltaTime;
      base.value = Mathf.Clamp(base.value + _handleVelocity * Time.deltaTime, 0F, 1F);

      if (base.value == 0F && _readyForCallback) {
        OnToggleOff.Invoke();
      }
      else if (base.value == 1F && _readyForCallback) {
        OnToggleOn.Invoke();
      }
    }
    else {
      _handleVelocity = 0F;
    }

    if (base.value != 0F && base.value != 1F) {
      _readyForCallback = true;
    }
  }

  public override void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData) {
    base.OnPointerDown(eventData);

    _beingHandled = true;
  }

  public override void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData) {
    base.OnPointerUp(eventData);

    _beingHandled = false;
  }

}
