using InteractionEngineUtility;
using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LeapPaint_v3 {

  public class NoPaintZoneFeedbackDrawer : MonoBehaviour, IRuntimeGizmoComponent {

    private AnimationCurve _curve = DefaultCurve.SigmoidUp;

    private static NoPaintZoneFeedbackDrawer _instance = null;
    public static NoPaintZoneFeedbackDrawer instance {
      get { return _instance; }
    }

    private void Awake() {
      if (_instance == null) _instance = this;
    }

    public static void DrawFailedPaintGizmo(Vector3 pinchPos, Collider dueToCollider) {
      instance.Draw(pinchPos, dueToCollider);
    }

    public void Draw(Vector3 pinchPos, Collider dueToCollider) {
      var collider = dueToCollider;

      //var surfacePos = collider.transform.TransformPoint(collider.ClosestPointOnSurface(
      //                            collider.transform.InverseTransformPoint(pinchPos)));
      //var normal = surfacePos - dueToCollider.transform.position;
      var normal = collider.transform.position - pinchPos;

      var newGizmo = new FailedPaintGizmo() {
        position = pinchPos,
        normal = normal,
        colliderPos = dueToCollider.transform.position,
        initRingSize = 0.01f,
        finalRingSize = 0.06f,
        duration = 0.6f,
        t = 0f
      };

      _failedPaintGizmos.Add(newGizmo);
    }

    private List<FailedPaintGizmo> _failedPaintGizmos = new List<FailedPaintGizmo>();

    public struct FailedPaintGizmo {
      public Vector3 position;
      public Vector3 normal;
      public Vector3 colliderPos;
      public float initRingSize;
      public float finalRingSize;
      public float duration;
      public float t;

      public bool isFinished;
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      drawer.color = LeapColor.cyan;

      var rDelta = 0.008f;
      var dt = Time.deltaTime;
      FailedPaintGizmo gizmo;
      for (int i = 0; i < _failedPaintGizmos.Count; i++) {
        gizmo = _failedPaintGizmos[i];

        gizmo.t += dt;
        if (gizmo.t > gizmo.duration) {
          gizmo.t = gizmo.duration;
          gizmo.isFinished = true;
        }

        var progress = _curve.Evaluate(gizmo.t / gizmo.duration);
        drawer.color = drawer.color.WithAlpha(Mathf.Sin(progress * Mathf.PI));

        for (int j = 0; j < 3; j++) {
          var r = Mathf.Lerp(gizmo.initRingSize, gizmo.finalRingSize, progress)
                    - rDelta * j;

          if (r > 0f) {
            drawer.DrawWireArc(gizmo.position, gizmo.normal, gizmo.normal.Perpendicular(),
                               r, 1f, 32);
          }
        }
        drawer.DrawDashedLine(gizmo.position, gizmo.colliderPos, segmentsPerMeter: 128);

        _failedPaintGizmos[i] = gizmo;
      }

      _failedPaintGizmos.RemoveAll((g) => g.isFinished);
    }

  }

}
