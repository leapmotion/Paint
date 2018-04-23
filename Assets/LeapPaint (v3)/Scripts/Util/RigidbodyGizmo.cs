using UnityEngine;
using System.Collections;
using Leap.Unity.RuntimeGizmos;


namespace Leap.Unity.LeapPaint_v3 {

  public class RigidbodyGizmo : MonoBehaviour, IRuntimeGizmoComponent {

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      Rigidbody body = GetComponent<Rigidbody>();
      if (body == null) return;

      drawer.PushMatrix();
      drawer.matrix = this.transform.localToWorldMatrix;
      drawer.color = Color.red;
      drawer.DrawWireCube(Vector3.zero, Vector3.one * 0.01F);
      drawer.PopMatrix();
    }

  }


}