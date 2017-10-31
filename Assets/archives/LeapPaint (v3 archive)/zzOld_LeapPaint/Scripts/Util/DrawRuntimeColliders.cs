using UnityEngine;
using System.Collections;
using Leap.Unity.RuntimeGizmos;
namespace Leap.Unity.LeapPaint_v3 {

  public class DrawRuntimeColliders : MonoBehaviour, IRuntimeGizmoComponent {

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      drawer.DrawColliders(this.gameObject);
    }

  }


}
