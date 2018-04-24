using UnityEngine;
using System.Collections;
using Leap.Unity.RuntimeGizmos;
using UnityEngine.Events;

namespace Leap.Unity.LeapPaint_v3 {

  [System.Serializable]
  public class FloatEvent : UnityEvent<float> { }

  public class PressableSlider : PressableUI {

    [Header("Pressable Slider")]
    [Tooltip("This layer and any layers above it will slide with the UIActivator activating the Slider.")]
    public int _slideableLayersBeginIndex = 0;
    public SlideAxis _slideAxis = SlideAxis.X;
    public float _minSlideDistance = 0F;
    public float _maxSlideDistance = 1F;

    public FloatEvent OnSliderValueChanged;

    public SoundEffect _tickEffect;
    public float _distancePerTick = 0.02f;
    public int tickCooldown = 2;

    private Vector3 _slideXZPosition = Vector3.zero;
    private float _lastSliderValue = 0F;

    private float _distTillTick = 0;
    private int _framesUntilTickAllowed = 0;

    protected override void Start() {
      base.Start();

      _slideXZPosition = GetAxisSlideXZPosition();
      OnSliderValueChanged.Invoke(0F);
    }

    protected override void LayerUpdate() {
      base.LayerUpdate();

      _framesUntilTickAllowed--;

      if (IsActivated) {
        _slideXZPosition = GetAxisSlideXZPosition();

        float sliderValue = GetSliderValue();
        if (sliderValue != _lastSliderValue) {

          _distTillTick -= Mathf.Abs(_lastSliderValue - sliderValue);
          if (_distTillTick < _distancePerTick && _framesUntilTickAllowed <= 0) {
            while (_distTillTick < _distancePerTick) _distTillTick += _distancePerTick;
            _tickEffect.PlayAtPosition(transform);
            _framesUntilTickAllowed = tickCooldown + 1;
          }

          _lastSliderValue = sliderValue;
          OnSliderValueChanged.Invoke(sliderValue);
        }
      }

      for (int i = _slideableLayersBeginIndex; i < _layers.Length; i++) {
        _layers[i].layerTransform.position = this.transform.TransformPoint(new Vector3(_slideXZPosition.x, _layers[i].height, _slideXZPosition.z));
      }
    }

    #region Utility

    private float GetSliderValue() {
      Vector3 P = GetAxisSlideXZPosition();
      Vector3 S = GetSliderStartPosition();
      Vector3 E = GetSliderEndPosition();
      if (_slideAxis == SlideAxis.X || _slideAxis == SlideAxis.NegativeX) {
        return (float)System.Math.Round((P.x - S.x) / (E.x - S.x), 3);
      }
      else {
        return (float)System.Math.Round((P.z - S.z) / (E.z - S.z), 3);
      }
    }

    private Vector3 GetAxisSlideXZPosition() {
      if (!IsActivated) {
        return GetSliderStartPosition();
      }
      else {
        Vector3 P = this.transform.InverseTransformPoint(GetActivatorWorldPosition());
        Vector3 S = GetSliderStartPosition();
        Vector3 E = GetSliderEndPosition();
        return S + (Mathf.Max(0F, Mathf.Min(Vector3.Dot((E - S), (P - S)) / (E - S).magnitude, (E - S).magnitude)) * _slideAxis.Direction());
      }
    }

    private Vector3 GetSliderStartPosition() {
      float sliderLength = (_slideAxis == SlideAxis.X || _slideAxis == SlideAxis.NegativeX) ? _xWidth : _zWidth;
      return (-_slideAxis.Direction() * sliderLength / 2F) + _slideAxis.Direction() * _minSlideDistance;
    }

    private Vector3 GetSliderEndPosition() {
      float sliderLength = (_slideAxis == SlideAxis.X || _slideAxis == SlideAxis.NegativeX) ? _xWidth : _zWidth;
      return (-_slideAxis.Direction() * sliderLength / 2F) + _slideAxis.Direction() * _maxSlideDistance;
    }

    #endregion

    #region Gizmos

    public override void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      base.OnDrawRuntimeGizmos(drawer);

      if (_enableGizmos) {
        drawer.PushMatrix();
        drawer.matrix = this.transform.localToWorldMatrix;

        drawer.color = Color.magenta;
        drawer.DrawWireCube(GetSliderStartPosition(), Vector3.one * 0.5F);

        drawer.color = Color.black;
        drawer.DrawWireCube(GetSliderEndPosition(), Vector3.one * 0.5F);


        if (IsActivated) {
          drawer.color = Color.cyan;
          drawer.DrawWireCube(this.transform.InverseTransformPoint(GetActivatorWorldPosition()), Vector3.one);

          drawer.color = Color.blue;
          drawer.DrawWireCube(GetAxisSlideXZPosition(), Vector3.one * 0.5F);
        }

        drawer.PopMatrix();
      }
    }

    #endregion

  }

  public enum SlideAxis {
    X,
    NegativeX,
    Z,
    NegativeZ
  }

  public static class SlideAxisFunctions {
    public static Vector3 Direction(this SlideAxis slideAxis) {
      if (slideAxis == SlideAxis.X) {
        return Vector3.right;
      }
      else if (slideAxis == SlideAxis.NegativeX) {
        return -Vector3.right;
      }
      else if (slideAxis == SlideAxis.Z) {
        return Vector3.forward;
      }
      else {
        return -Vector3.forward;
      }
    }
  }

}