using UnityEngine;
using System.Collections;
using StrokeProcessing;

namespace Leap.Paint {

  public class FilterApplyThickness : MonoBehaviour, IBufferFilter<StrokePoint> {

    private float _thickness = 0.002F;
    public float _lastNormalizedValue = 0F;

    private float _minThickness = 0.002F;
    private float _maxThickness = 0.03F;

    public void SetThickness(float normalizedValue) {
      float value = Mathf.Clamp(normalizedValue, 0F, 1F);
      _thickness = Mathf.Lerp(_minThickness, _maxThickness, value);
      _lastNormalizedValue = normalizedValue;
    }

    public int GetMinimumBufferSize() {
      return 0;
    }

    public void Process(RingBuffer<StrokePoint> data, RingBuffer<int> indices) {
      StrokePoint s = data.GetFromEnd(0);
      s.thickness = _thickness;
      data.SetFromEnd(0, s);
    }

    public void Reset() {
      return;
    }
  }


}