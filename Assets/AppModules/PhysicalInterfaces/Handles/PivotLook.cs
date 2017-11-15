using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public static class PivotLook {

    public struct PivotLookConstraint {
      public Pose panel;
      public Pose panelToPivot;

      public Vector3 pivotTarget;
      public Vector3 lookTarget;
      public Vector3 horizonNormal;
    }

    //public static void AnalyticSolve(Pose panel,
    //                                 Pose panelToPivot,
    //                                 Vector3 pivotTarget,
    //                                 Vector3 lookTarget,
    //                                 Vector3 horizonNormal) {
    //  var R = panelToPivot.position.magnitude;

    //  var camToPivot = panel.Then(panelToPivot).position - lookTarget;

    //  var pivotRotateAxis = Vector3.Cross(camToPivot, horizonNormal);

    //  var pivotUpward = Vector3.Cross(pivotRotateAxis, camToPivot);
    //}

    /// <summary>
    /// Returns the Panel pose necessary to solve the look constraint that places
    /// the pivot defined by panelToPivot at the pivotTarget, and the panel looking at
    /// the lookTarget.
    /// </summary>
    public static Pose Solve(Pose panel,
                             Pose panelToPivot,
                             Vector3 pivotTarget,
                             Vector3 lookTarget,
                             Vector3 horizonNormal = default(Vector3),
                             int maxIterations = 8) {
      if (horizonNormal == default(Vector3)) {
        horizonNormal = Vector3.up;
      };

      return Solve(new PivotLookConstraint() {
        panel = panel,
        panelToPivot = panelToPivot,
        pivotTarget = pivotTarget,
        lookTarget = lookTarget
      }).panel;
    }

    public static PivotLookConstraint Solve(PivotLookConstraint pivotLook,
                                            int maxIterations = 8,
                                            float solveAngle = 0.1f) {
      var lookTarget = pivotLook.lookTarget;
      var pivotTarget = pivotLook.pivotTarget;
      var horizonNormal = pivotLook.horizonNormal;
      var panelToPivot = pivotLook.panelToPivot;

      var panelPivotSqrDist = pivotLook.panelToPivot.position.sqrMagnitude;
      var lookPivotSqrDist = (lookTarget - pivotTarget).sqrMagnitude;
      if (lookPivotSqrDist <= panelPivotSqrDist) {
        Debug.LogError("Pivot to close to look target; no solution.");
        return pivotLook;
      }
      
      var iterations = 0;
      var angleToCam = Vector3.Angle((pivotLook.panel.rotation * Vector3.forward),
                                     (lookTarget - pivotLook.panel.position));
      while (angleToCam > solveAngle && iterations++ < maxIterations) {
        // Panel look at camera.
        pivotLook.panel.rotation = Utils.FaceTargetWithoutTwist(pivotLook.panel.position,
                                                                lookTarget,
                                                                horizonNormal);

        // Restore pivot position relative to panel.
        var newPivotPosition = pivotLook.panel.Then(panelToPivot).position;

        // Shift panel by translation away from pivotTarget.
        var newPivotToPivotTarget = pivotTarget - newPivotPosition;

        pivotLook.panel = pivotLook.panel.WithPosition(pivotLook.panel.position
                                                       + newPivotToPivotTarget);

        angleToCam = Vector3.Angle((pivotLook.panel.rotation * Vector3.forward),
                                     (lookTarget - pivotLook.panel.position));
      }

      return pivotLook;
    }

  }

}
