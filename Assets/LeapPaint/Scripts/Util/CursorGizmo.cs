using UnityEngine;
using System.Collections;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity;

public class CursorGizmo : MonoBehaviour, IRuntimeGizmoComponent {

  public PinchDetector pinchDetector;

  protected virtual void Update() {
    this.transform.position = pinchDetector.transform.position;
    this.transform.rotation = pinchDetector.transform.rotation * Quaternion.Euler(new Vector3(90F, 0F, 180F));
  }

  public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    drawer.PushMatrix();

    drawer.matrix = this.transform.localToWorldMatrix;

    drawer.color = Color.red;
    drawer.DrawLine(Vector3.zero, Vector3.right * 0.01F);
    drawer.color = Color.green;
    drawer.DrawLine(Vector3.zero, Vector3.up * 0.01F);
    drawer.color = Color.blue;
    drawer.DrawLine(Vector3.zero, Vector3.forward * 0.01F);

    drawer.PopMatrix();
  }

}
