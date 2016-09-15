using UnityEngine;
using System.Collections;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity;

public class CursorGizmo : MonoBehaviour, IRuntimeGizmoComponent {

  public PinchDetector _leftPinchDetector;
  public PinchDetector _rightPinchDetector;
  private Vector3 _leftHandEulerRotation = new Vector3(0F, 180F, 0F);
  private Vector3 _rightHandEulerRotation = new Vector3(0F, 180F, 0F);

  public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    this.transform.position = _leftPinchDetector.transform.position;
    this.transform.rotation = _leftPinchDetector.transform.rotation * Quaternion.Euler(_leftHandEulerRotation);
    DrawPinchDetectorAlignmentGizmo(_leftPinchDetector, drawer);
    DrawCursorGizmo(_leftPinchDetector, drawer);

    this.transform.position = _rightPinchDetector.transform.position;
    this.transform.rotation = _rightPinchDetector.transform.rotation * Quaternion.Euler(_rightHandEulerRotation);
    DrawPinchDetectorAlignmentGizmo(_rightPinchDetector, drawer);
    DrawCursorGizmo(_rightPinchDetector, drawer);
  }

  private bool _alignmentGizmoEnabled = false;
  private bool _cursorRingEnabled = true;

  private void DrawPinchDetectorAlignmentGizmo(PinchDetector pinchDetector, RuntimeGizmoDrawer drawer) {
    if (_alignmentGizmoEnabled) {
      drawer.PushMatrix();

      drawer.matrix = pinchDetector.transform.localToWorldMatrix;

      drawer.color = Color.red;
      drawer.DrawLine(Vector3.zero, Vector3.right * 0.01F);
      drawer.color = Color.green;
      drawer.DrawLine(Vector3.zero, Vector3.up * 0.01F);
      drawer.color = Color.blue;
      drawer.DrawLine(Vector3.zero, Vector3.forward * 0.01F);

      drawer.PopMatrix();
    }
  }

  private void DrawCursorGizmo(PinchDetector pinchDetector, RuntimeGizmoDrawer drawer) {
    if (_cursorRingEnabled) {
      drawer.PushMatrix();

      drawer.matrix = pinchDetector.transform.localToWorldMatrix;

      drawer.color = Color.white;
      drawer.DrawCircle(Vector3.zero, 0.02F, Vector3.up);

      drawer.PopMatrix();
    }
  }

}

public static class RuntimeGizmoDrawerExtensions {
  public static void DrawCircle(this RuntimeGizmoDrawer drawer, Vector3 position, float radius, Vector3 planeDirection) {
    Vector3 perpDirection = Vector3.Cross(planeDirection, Vector3.up).normalized;
    if (perpDirection.magnitude < 0.99F) {
      perpDirection = Vector3.Cross(planeDirection, Vector3.right).normalized;
    }
    int numSegments = 64;
    for (int i = 0; i < numSegments; i++) {
      drawer.DrawLine(position + Quaternion.AngleAxis(360F * (i / (float)numSegments), planeDirection) * perpDirection * radius,
        position + Quaternion.AngleAxis(360F * ((i + 1) / (float)numSegments), planeDirection) * perpDirection * radius);
    }
  }
}