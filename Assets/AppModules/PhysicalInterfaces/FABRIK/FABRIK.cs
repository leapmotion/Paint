using Leap.Unity.Query;
using UnityEngine;

namespace Leap.Unity.IK {

  public static class FABRIK {

    public struct FABRIKChain {
      public Vector3 start;
      public Vector3[] points;
      public float[] lengths;
      public Vector3 target;
    }

    /// <summary>
    /// Performs a FABRIK inverse kinematic solve, modifying the FABRIKChain points array.
    /// </summary>
    public static void Solve(FABRIKChain chain,
                             int maxIterations = 8,
                             float solveDistance = 0.01f) {
      var start = chain.start;
      var target = chain.target;

      var sumLengths = chain.lengths.Query().Fold((sum, l) => sum + l);
      var sqrDistToTarget = (target - start).sqrMagnitude;

      if (sqrDistToTarget > sumLengths * sumLengths) {
        // Target out of reach, straighten towards it.
        var toTarget = (target - start).normalized;
        for (int i = 0; i + 1 < chain.points.Length; i++) {
          chain.points[i + 1] = chain.points[i] + toTarget * chain.lengths[i];
        }
      }
      else {
        // Target within reach.
        var iterations = 0;
        var sqrDistToGoal = (chain.points[chain.points.Length - 1] - target).sqrMagnitude;
        var solveDistanceSqr = solveDistance * solveDistance;
        while (sqrDistToGoal > solveDistanceSqr && iterations++ < maxIterations) {
          BackwardSolve(chain);
          ForwardSolve(chain);
          sqrDistToGoal = (chain.points[chain.points.Length - 1] - target).sqrMagnitude;
        }
      }
    }

    /// <summary>
    /// Performs a single backwards FABRIK solver iteration on the argument chain.
    /// </summary>
    public static void BackwardSolve(FABRIKChain chain) {
      // Move the last point to the target and project backwards along chain.
      chain.points[chain.points.Length] = chain.target;
      for (int i = chain.points.Length - 1; i - 1 >= 0; i--) {
        var linkDir = (chain.points[i - 1] - chain.points[i]).normalized;
        chain.points[i - 1] = chain.points[i] + linkDir * chain.lengths[i - 1];
      }
    }

    /// <summary>
    /// Performs a single forward FABRIK solver iteration on the argument chain.
    /// </summary>
    public static void ForwardSolve(FABRIKChain chain) {
      // Move the first point to the start and project forward along chain.
      chain.points[0] = chain.start;
      for (int i = 0; i + 1 < chain.points.Length; i++) {
        var linkDir = (chain.points[i + 1] - chain.points[i]).normalized;
        chain.points[i + 1] = chain.points[i] + linkDir * chain.lengths[i];
      }
    }

  }

}