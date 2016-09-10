using System;
using UnityEngine;

public class FilterRollToCanvasAlignment : IMemoryFilter<StrokePoint> {

  public int GetMemorySize() {
    return 2;
  }

  public void Process(RingBuffer<StrokePoint> data, RingBuffer<int> indices) {
    if (data.Size < 2) return;
    else {
      for (int offset = data.Size - 2; offset >= 0; offset--) {
        StrokePoint p0 = data.GetFromEnd(1 + offset);
        StrokePoint p1 = data.GetFromEnd(0 + offset);

        Vector3 segmentDirection = (p1.position - p0.position).normalized;
        Vector3 handForward = p0.handOrientation * Vector3.forward;
        Vector3 handBackward = p0.handOrientation * Vector3.back;
        Vector3 handDirection = (Vector3.Dot(segmentDirection, handForward) > 0 ? handForward : handBackward);

        // Coefficient 1 when segment velocity parallel to canvas plane, 0 when perpendicular.
        float canvasAlignmentCoefficient = Vector3.Cross(segmentDirection, handDirection).magnitude;

        Vector3 normalCrossHand = Vector3.Cross(p0.normal, handDirection);
        Quaternion alignmentRotation = Quaternion.AngleAxis(
          Mathf.Asin(normalCrossHand.magnitude) * 360F / (2 * Mathf.PI) * canvasAlignmentCoefficient,
          normalCrossHand.normalized);

        p1.normal = alignmentRotation * p0.normal;
        data.SetFromEnd(0 + offset, p1);
      }
    }
  }

  public void Reset() {
    return;
  }

}