using UnityEngine;
using System.Collections;

namespace Leap.zzOldPaint {

  public class PressableButtonFeedback : MonoBehaviour {

    public Renderer _buttonRenderer;
    public Color _normalColor = new Color(223 / 255F, 223 / 255F, 223 / 255F);
    public Color _pressedColor = new Color(152 / 255F, 152 / 255F, 152 / 255F);

    private PressableUI _pressable;
    private Pulsator _pulsator;

    void Start() {
      _pressable = GetComponent<PressableUI>();
      if (_pressable != null) {
        _pressable.OnPress.AddListener(DoOnPress);
        _pressable.OnRelease.AddListener(DoOnRelease);

        _pulsator = gameObject.AddComponent<Pulsator>();
        _pulsator.OnValueChanged += DoOnPulsatorValue;
        _pulsator._speed = 5F;
      }
      else {
        Debug.LogWarning("[PressableButtonFeedback] GameObject lacks a PressableUI.");
      }
    }

    private void DoOnPress() {
      _pulsator.Activate();
    }

    private void DoOnRelease() {
      _pulsator.Release();
    }

    private void DoOnPulsatorValue(float value) {
      if (Application.isPlaying) {
        if (_buttonRenderer != null) {
          _buttonRenderer.material.color = Color.Lerp(_normalColor, _pressedColor, value);
        }
      }
    }

  }


}