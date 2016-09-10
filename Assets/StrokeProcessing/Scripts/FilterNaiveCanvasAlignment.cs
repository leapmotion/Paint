using System;
using UnityEngine;

public class FilterNaiveCanvasAlignment : IMemoryFilter<StrokePoint> {

  public int GetMemorySize() {
    return 1;
  }

  public void Process(RingBuffer<StrokePoint> data, RingBuffer<int> indices) {
    StrokePoint current = data.GetFromEnd(0);
    StrokePoint memory = data.GetFromEnd(1);

    current.rotation = Quaternion.identity;
    current.normal = current.handOrientation * Vector3.back;

    data.SetFromEnd(0, current);
  }

  public void Reset() {
    return;
  }

}