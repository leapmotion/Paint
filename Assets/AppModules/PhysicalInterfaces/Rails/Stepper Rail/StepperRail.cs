using Leap.Unity.RuntimeGizmos;
using Leap.Unity.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class StepperRail : MonoBehaviour, IRuntimeGizmoComponent {

    [Header("Rail Edge Control Point (Mirrored)")]
    public SplineTransformControlPoint railEdgePoint;

    [Header("Stack Edge Control Point (Mirrored)")]
    public SplineTransformControlPoint stackEdgePoint;

    public struct PoseSpline {
      public Pose[] points;
    }

    public Pose centerPose {
      get { return this.transform.ToPose(); }
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (!gameObject.activeInHierarchy || !this.enabled) return;

      drawer.color = LeapColor.brown;

      drawer.DrawPose(centerPose);
    }

  }

}
