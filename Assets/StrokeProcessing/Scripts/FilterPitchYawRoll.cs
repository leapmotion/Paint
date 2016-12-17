using UnityEngine;
using System.Collections;
using StrokeProcessing;

namespace Leap.Paint {

  public class FilterPitchYawRoll : IBufferFilter<StrokePoint> {

    private const float RAD_2_DEG = 360F / (2 * Mathf.PI);

    public int GetMinimumBufferSize() {
      return 2;
    }

    public void Process(RingBuffer<StrokePoint> data, RingBuffer<int> indices) {
      if (data.Size < 1) return;
      if (data.Size == 1) {
        StrokePoint point = data.Get(0);
        point.rotation = point.handOrientation;
        point.normal = point.rotation * Vector3.up;
        data.Set(0, point);
      }
      else if (data.Size >= 2) {
        for (int offset = 0; offset < data.Size - 1; offset++) {
          StrokePoint point = data.Get(0 + offset);
          Vector3 T = point.rotation * Vector3.forward;
          Vector3 N = point.rotation * Vector3.up;

          Vector3 segmentDirection = (data.Get(1 + offset).position - point.position).normalized;

          // Pitch correction
          Vector3 sD_TN = (Vector3.Dot(T, segmentDirection) * T + Vector3.Dot(N, segmentDirection) * N).normalized;
          Vector3 T_x_sD_TN = Vector3.Cross(T, sD_TN);
          float T_x_sD_TN_magnitude = Mathf.Clamp(T_x_sD_TN.magnitude, 0F, 1F); // Fun fact! Sometimes the magnitude of this vector is 0.000002 larger than 1F, which causes NaNs from Mathf.Asin().
          Quaternion pitchCorrection;
          if (Vector3.Dot(T, sD_TN) >= 0F) {
            pitchCorrection = Quaternion.AngleAxis(Mathf.Asin(T_x_sD_TN_magnitude) * RAD_2_DEG, T_x_sD_TN.normalized);
          }
          else {
            pitchCorrection = Quaternion.AngleAxis(180F - (Mathf.Asin(T_x_sD_TN_magnitude) * RAD_2_DEG), T_x_sD_TN.normalized);
          }

          // Yaw correction
          Vector3 T_pC = pitchCorrection * T;
          Vector3 T_pC_x_sD = Vector3.Cross(T_pC, segmentDirection);
          Quaternion yawCorrection = Quaternion.AngleAxis(Mathf.Asin(T_pC_x_sD.magnitude) * RAD_2_DEG, T_pC_x_sD.normalized);

          // Roll correction (align to canvas)
          T = pitchCorrection * yawCorrection * T;
          N = pitchCorrection * yawCorrection * N;
          Vector3 handUp = point.handOrientation * Vector3.up;
          Vector3 handDown = point.handOrientation * Vector3.down;
          Vector3 canvasDirection;
          if (Vector3.Dot(N, handUp) >= 0F) {
            canvasDirection = handUp;
          }
          else {
            canvasDirection = handDown;
          }
          Vector3 B = Vector3.Cross(T, N).normalized; // binormal
          Vector3 canvasCastNB = (Vector3.Dot(N, canvasDirection) * N + Vector3.Dot(B, canvasDirection) * B);
          Vector3 N_x_canvasNB = Vector3.Cross(N, canvasCastNB.normalized);
          float N_x_canvasNB_magnitude = Mathf.Clamp(N_x_canvasNB.magnitude, 0F, 1F); // Fun fact! Sometimes the magnitude of this vector is 0.000002 larger than 1F, which causes NaNs from Mathf.Asin().
          Quaternion rollCorrection = Quaternion.AngleAxis(
            DeadzoneDampenFilter(canvasCastNB.magnitude) * Mathf.Asin(N_x_canvasNB_magnitude) * RAD_2_DEG,
            N_x_canvasNB.normalized
            );

          point.rotation = pitchCorrection * yawCorrection * rollCorrection * point.rotation;
          point.normal = point.rotation * Vector3.up;

          data.Set(0 + offset, point);

          StrokePoint nextPoint = data.Get(1 + offset);
          nextPoint.rotation = point.rotation;
          nextPoint.normal = point.normal;
          data.Set(1 + offset, nextPoint);
        }
      }
    }

    public void Reset() { }

    // Assumes input from 0 to 1.
    private float DeadzoneDampenFilter(float input) {
      float deadzone = 0.5F;
      float dampen = 0.2F;
      return Mathf.Max(0F, (input - deadzone) * dampen);
    }

  }


}