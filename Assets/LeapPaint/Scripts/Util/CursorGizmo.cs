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
    DrawPinchDetectorCursorGizmo(_leftPinchDetector, drawer);

    this.transform.position = _rightPinchDetector.transform.position;
    this.transform.rotation = _rightPinchDetector.transform.rotation * Quaternion.Euler(_rightHandEulerRotation);
    DrawPinchDetectorCursorGizmo(_rightPinchDetector, drawer);
  }

  private void DrawPinchDetectorCursorGizmo(PinchDetector pinchDetector, RuntimeGizmoDrawer drawer) {
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
