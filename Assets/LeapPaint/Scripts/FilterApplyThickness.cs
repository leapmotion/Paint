using UnityEngine;
using System.Collections;

public class FilterApplyThickness : MonoBehaviour, IMemoryFilter<StrokePoint> {

  public float _thickness = 0.003F;

  private float _minThickness = 0.003F;
  private float _maxThickness = 0.03F;

  public void SetThickness(float normalizedValue) {
    float value = Mathf.Clamp(normalizedValue, 0F, 1F);
    _thickness = Mathf.Lerp(_minThickness, _maxThickness, value);
  }

  public int GetMemorySize() {
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
