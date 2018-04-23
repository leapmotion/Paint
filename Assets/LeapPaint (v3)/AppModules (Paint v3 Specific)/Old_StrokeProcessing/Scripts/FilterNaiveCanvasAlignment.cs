
using System;
using UnityEngine;

namespace Leap.Unity.LeapPaint_v3 {

  public class FilterNaiveCanvasAlignment : IBufferFilter<StrokePoint> {

    public int GetMinimumBufferSize() {
      return 1;
    }

    public void Process(RingBuffer<StrokePoint> data, RingBuffer<int> indices) {
      StrokePoint current = data.GetLatest();

      current.rotation = Quaternion.identity;
      current.normal = current.handOrientation * Vector3.back;

      data.SetLatest(current);
    }

    public void Reset() {
      return;
    }

  }

}