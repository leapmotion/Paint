using Leap.Unity.Animation;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Leap.Unity.PhysicalInterfaces {

  using IPositionSpline = ISpline<Vector3, Vector3>;

  [ExecuteInEditMode]
  public class StepperRail : MonoBehaviour, IRuntimeGizmoComponent,
                                            IPositionSpline {

    [Header("Center Point Velocity Source")]
    public Transform centerPointVelocity;

    [Header("Rail Edge Point (Mirrored on X) - child for Velocity")]
    public Transform railEdgePoint;
    public Transform railEdgePointVelocity;

    [Header("Stack Edge Point (Mirrored on X) - child for Velocity")]
    public Transform stackEdgePoint;
    public Transform stackEdgePointVelocity;

    [Header("Panel Objects Test")]
    public Transform panelObjectsParent;
    public float tSpacing = 0.75f;
    public float speedMod = 1f;
    public float testTCenter = 0f;

    private List<Transform> _panelObjectsBuffer = new List<Transform>();

    public Pose centerPose {
      get { return this.transform.ToPose(); }
    }

    public PoseSplineSequence? maybePoseSplines = null;

    private HermitePoseSpline[] _backingPoseSplinesArr = null;
    private HermitePoseSpline[] _poseSplinesArr {
      get {
        if (_backingPoseSplinesArr == null) {
          _backingPoseSplinesArr = new HermitePoseSpline[4];
        }
        return _backingPoseSplinesArr;
      }
    }

    private void Update() {
      if (stackEdgePoint != null && railEdgePoint != null) {
        var poseN2 = stackEdgePoint.parent.transform.ToPose().Then(stackEdgePoint.ToLocalPose().MirroredX());
        var poseN1 = railEdgePoint.parent.transform.ToPose().Then(railEdgePoint.ToLocalPose().MirroredX());
        var pose0 = this.transform.ToPose();
        var pose1 = railEdgePoint.ToPose();
        var pose2 = stackEdgePoint.ToPose();

        var pose1Child = railEdgePoint.transform.GetFirstChild();
        var pose2Child = stackEdgePoint.transform.GetFirstChild();

        var pose0Movement = new Movement(transform.ToPose(), centerPointVelocity.ToPose(), 0.1f);
        Movement pose1Movement = Movement.identity;
        Movement poseN1Movement = Movement.identity;
        if (pose1Child != null) {
          pose1Movement = new Movement(pose1, pose1Child.ToPose(), 0.1f);
          poseN1Movement = new Movement(poseN1, poseN1.Then(pose1Child.ToLocalPose().Negated().MirroredX()), 0.1f);
        }
        Movement pose2Movement = Movement.identity;
        Movement poseN2Movement = Movement.identity;
        if (pose2Child != null) {
          pose2Movement = new Movement(pose2, pose2Child.ToPose(), 0.1f);
          poseN2Movement = new Movement(poseN2, poseN2.Then(pose2Child.ToLocalPose().Negated().MirroredX()), 0.1f);
        }

        _poseSplinesArr[0] = new HermitePoseSpline(-2f, -1f, poseN2, poseN1, poseN2Movement, poseN1Movement);
        _poseSplinesArr[1] = new HermitePoseSpline(-1f, 0f, poseN1, pose0, poseN1Movement, pose0Movement);
        _poseSplinesArr[2] = new HermitePoseSpline(0f, 1f, pose0, pose1, pose0Movement, pose1Movement);
        _poseSplinesArr[3] = new HermitePoseSpline(1f, 2f, pose1, pose2, pose1Movement, pose2Movement);

        maybePoseSplines = new PoseSplineSequence(_poseSplinesArr,
                                                  allowExtrapolation: true);


        // Panel Objects Test

        if (panelObjectsParent != null && panelObjectsParent.childCount > 0) {
          _panelObjectsBuffer.Clear();
          foreach (var panelObject in panelObjectsParent.GetChildren()) {
            _panelObjectsBuffer.Add(panelObject);
          }

          var splines = maybePoseSplines.Value;
          var baseT = testTCenter;
          if (!Application.isPlaying) baseT = testTCenter;
          for (int i = 0; i < _panelObjectsBuffer.Count; i++) {
            var t = baseT + ((-2 + i) * tSpacing);
            var objPose = splines.PoseAt(t);
            _panelObjectsBuffer[i].transform.SetPose(objPose);
          }
        }

      }
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (!gameObject.activeInHierarchy || !this.enabled) return;

      drawer.color = LeapColor.brown;

      if (maybePoseSplines != null) {
        drawer.DrawPoseSplineSequence(maybePoseSplines.Value);
      }
    }

    #region IPositionSpline - ISpline<Vector3, Vector3>

    public float minT {
      get { return maybePoseSplines.HasValue ? maybePoseSplines.Value.minT : 0f; }
    }

    public float maxT {
      get { return maybePoseSplines.HasValue ? maybePoseSplines.Value.maxT : 0f; }
    }

    public Vector3 ValueAt(float t) {
      return maybePoseSplines.HasValue ? maybePoseSplines.Value.ValueAt(t).position
                                       : Vector3.zero;
    }

    public Vector3 DerivativeAt(float t) {
      return maybePoseSplines.HasValue ? maybePoseSplines.Value.DerivativeAt(t).velocity
                                       : Vector3.zero;
    }

    public void ValueAndDerivativeAt(float t, out Vector3 value, out Vector3 deltaValuePerT) {
      value = Vector3.zero;
      deltaValuePerT = Vector3.zero;

      if (maybePoseSplines.HasValue) {
        Pose pose;
        Movement movement;
        maybePoseSplines.Value.ValueAndDerivativeAt(t, out pose, out movement);

        value = pose.position;
        deltaValuePerT = movement.velocity;
      }
    }

    #endregion

  }

  public static class StepperRailExtensions {
    public static Transform GetFirstChild(this Transform t) {
      if (t.childCount == 0) { return null; }
      return t.GetChild(0);
    }
  }

}
