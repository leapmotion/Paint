using System;
using UnityEngine;

public class FilterNaiveCanvasAlignment : IBufferFilter<StrokePoint> {

  public int GetMinimumBufferSize() {
    return 1;
  }

  public void Process(RingBuffer<StrokePoint> data, RingBuffer<int> indices) {
    StrokePoint current = data.GetFromEnd(0);

    current.rotation = Quaternion.identity;
    current.normal = current.handOrientation * Vector3.back;

    data.SetFromEnd(0, current);
  }

  public void Reset() {
    return;
  }

}