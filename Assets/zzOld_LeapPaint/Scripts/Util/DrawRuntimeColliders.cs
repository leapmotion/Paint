using UnityEngine;
using System.Collections;
using Leap.Unity.RuntimeGizmos;

public class DrawRuntimeColliders : MonoBehaviour, IRuntimeGizmoComponent {

  public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    drawer.DrawColliders(this.gameObject);
  }

}
