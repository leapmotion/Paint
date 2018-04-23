using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LeapPaint_v3 {

  public class NoPaintZone : MonoBehaviour, IRuntimeGizmoComponent {

    private static HashSet<Collider> s_noPaintColliders = new HashSet<Collider>();
    public static ReadonlyHashSet<Collider> noPaintColliders {
      get { return s_noPaintColliders; }
    }

    public
    #if UNITY_EDITOR
    new
    #endif
    Collider collider;

    public bool drawDebug = false;

    void Reset() {
      if (collider == null) {
        collider = GetComponent<Collider>();
      }
    }

    void OnEnable() {
      s_noPaintColliders.Add(collider);
    }

    void OnDisable() {
      s_noPaintColliders.Remove(collider);
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (!drawDebug || !this.enabled || !gameObject.activeInHierarchy) return;

      drawer.color = LeapColor.cerulean.WithAlpha(0.5f);
      drawer.DrawColliders(this.gameObject, useWireframe: false, drawTriggers: true);
    }
  }

}
