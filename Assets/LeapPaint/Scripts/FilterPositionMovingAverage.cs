using UnityEngine;
using System.Collections;

public class FilterPositionMovingAverage : IMemoryFilter<StrokePoint> {

  private int _windowRadius = 0;

  public FilterPositionMovingAverage(int windowRadius) {
    _windowRadius = windowRadius;
  }

  public int GetMemorySize() {
    return _windowRadius * 2;
  }

  public void Process(RingBuffer<StrokePoint> data) {
    for (int i = data.Size / 2; i >= 0; i--) {
      StrokePoint point = data.GetFromEnd(i);
      point.position = CalcNeighborAverage(i, _windowRadius, data);
      data.SetFromEnd(i, point);
    }
  }

  private Vector3 CalcNeighborAverage(int index, int R, RingBuffer<StrokePoint> data) {
    Vector3 neighborSum = data.GetFromEnd(index).position;
    int numPointsInRadius = 1;
    for (int r = 1; r <= R; r++) {
      if (index - r < 0) break;
      if (index + r >= data.Size) break;
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
