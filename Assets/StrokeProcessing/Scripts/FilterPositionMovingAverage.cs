using UnityEngine;
using System.Collections;
using Leap.Unity.Attributes;

public class FilterPositionMovingAverage : IMemoryFilter<StrokePoint> {

  private const int NEIGHBORHOOD = 16;

  public int GetMemorySize() {
    return NEIGHBORHOOD * 2;
  }

  public void Process(RingBuffer<StrokePoint> data, RingBuffer<int> indices) {
    for (int i = Mathf.Min(data.Size - 1, NEIGHBORHOOD); i >= 0; i--) {
      StrokePoint point = data.GetFromEnd(i);
      point.position = Vector3.Lerp(point.position, CalcNeighborAverage(i, NEIGHBORHOOD, data), 1F / (data.Size - NEIGHBORHOOD));
      data.SetFromEnd(i, point);
    }
  }

  private Vector3 CalcNeighborAverage(int index, int R, RingBuffer<StrokePoint> data) {
    Vector3 neighborSum = data.GetFromEnd(index).position;
    int numPointsInRadius = 1;
    for (int r = 1; r <= R; r++) {
      if (index - r < 0) continue;
      if (index + r >= data.Size) continue;
      neighborSum += data.GetFromEnd(index - r).position;
      neighborSum += data.GetFromEnd(index + r).position;
      numPointsInRadius += 2;
    }
    return neighborSum / numPointsInRadius;
  }

  //private Vector3 CalcNeighborAverage(int index, int R, RingBuffer<StrokePoint> data) {
  //  Vector3 neighborSum = Vector3.zero;
  //  int numPointsInRadius = 0;
  //  for (int r = -R; r <= R; r++) {
  //    if (index + r < 0) continue;
  //    else if (index + r > data.Size - 1) continue;
  //    else {
  //      neighborSum += data.GetFromEnd(index + r).position;
  //      numPointsInRadius += 1;
  //    }
  //  }
  //  if (numPointsInRadius == 0) {
  //    return data.GetFromEnd(index).position;
  //  }
  //  else {
  //    return neighborSum / numPointsInRadius;
  //  }
  //}

  public void Reset() {
    return;
  }

}
