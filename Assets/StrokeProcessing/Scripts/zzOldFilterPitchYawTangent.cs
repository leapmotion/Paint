using System;
using UnityEngine;

public class zzOldFilterPitchYawTangent : IMemoryFilter<StrokePoint> {

  public int GetMemorySize() {
    return 2;
  }

  public virtual void Process(RingBuffer<StrokePoint> data, RingBuffer<int> indices) {
    Vector3 T, B, N;
    StrokePoint memory, current;

    if (data.Size < 1) return;
    else if (data.Size == 1) {
      StrokePoint point = data.Get(0);
      point.rotation = point.handOrientation;
      point.normal = point.rotation * Vector3.up;
      data.Set(0, point);
      return;
    }

    for (int offset = data.Size - 2; offset >= 0; offset--) {
      memory = data.GetFromEnd(1 + offset);
      current = data.GetFromEnd(0 + offset);
      
      if (offset == data.Size - 2) {
        N = memory.rotation * Vector3.up;
        B = memory.rotation * Vector3.right;
        T = Vector3.Cross(N, B);
      }
      else {
        StrokePoint preMemory = data.GetFromEnd(2 + offset);
        Vector3 prevSegmentDirection = (memory.position - preMemory.position).normalized;
        T = prevSegmentDirection;
        N = memory.rotation * Vector3.up;
        B = Vector3.Cross(T, N);
        if (B.magnitude < 0.999F) {
          //Debug.LogWarning("T not orthogonal to N, got B magnitude: " + B.magnitude);
          N = Vector3.Cross(B, T).normalized;
          memory.normal = N;
          data.SetFromEnd(1, memory);
        }
      }

      Vector3 segmentDirection = (current.position - memory.position).normalized;

      // Correct pitch and then yaw.
      Vector3 segmentDirectionCastTN = (Vector3.Dot(T, segmentDirection) * T + Vector3.Dot(N, segmentDirection) * N).normalized;
      Vector3 T_x_Vcast = Vector3.Cross(T, segmentDirectionCastTN);
      Quaternion pitchCorrection = Quaternion.AngleAxis(
        Mathf.Asin(T_x_Vcast.magnitude) * 360F / (2 * Mathf.PI), T_x_Vcast.normalized);
      Vector3 T_pC = pitchCorrection * T;
      Vector3 T_pC_x_V = Vector3.Cross(T_pC, segmentDirection);
      Quaternion yawCorrection = Quaternion.AngleAxis(
        Mathf.Asin(T_pC_x_V.magnitude) * 360F / (2 * Mathf.PI), T_pC_x_V.normalized);

      current.rotation = pitchCorrection * yawCorrection * memory.rotation;
      current.normal = current.rotation * Vector3.up;

      data.SetFromEnd(0 + offset, current);
    }
  }

  public virtual void Reset() {
    return;
  }

}