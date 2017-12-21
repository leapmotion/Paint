using Leap.Unity.Animation;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Leap.Unity.PhysicalInterfaces {

  [ExecuteInEditMode]
  public class StepperRail : MonoBehaviour, IRuntimeGizmoComponent {

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

  }

  [Serializable]
  public struct PoseSplineSequence : IIndexable<HermitePoseSpline> {
    public HermitePoseSpline[] splines;
    public bool allowExtrapolation;

    public PoseSplineSequence(HermitePoseSpline[] splines,
                              bool allowExtrapolation = false) {
      this.splines = splines;
      this.allowExtrapolation = allowExtrapolation;
    }

    public HermitePoseSpline this[int idx] {
      get { return splines[idx]; }
    }

    public int Count { get { return splines.Length; } }

    public Pose PoseAt(float t) {
      var minT = splines[0].t0;
      var maxT = splines[splines.Length - 1].t1;

      var dt = 0f;
      Pose poseOrigin; Movement extrapMovement;
      if (t < minT) {
        if (allowExtrapolation) {
          splines[0].PoseAndMovementAt(minT, out poseOrigin, out extrapMovement);
          dt = t - minT;
          return poseOrigin.Integrated(extrapMovement, dt);
        }
        else {
          t = minT;
        }
      }
      else if (t > maxT) {
        if (allowExtrapolation) {
          splines[splines.Length - 1].PoseAndMovementAt(maxT, out poseOrigin, out extrapMovement);
          dt = t - maxT;
          return poseOrigin.Integrated(extrapMovement, dt);
        }
        else {
          t = maxT;
        }
      }
      
      foreach (var spline in splines) {
        if (t >= spline.t0 && t <= spline.t1) {
          return spline.PoseAt(t);
        }
      }

      Debug.LogError("PoseSplineSequence couldn't evaluate T: " + t);
      return Pose.identity;
    }
  }

  public static class PoseSplineSequenceExtensions {
    public static void DrawPoseSplineSequence(this RuntimeGizmoDrawer drawer,
                                              PoseSplineSequence poseSplines) {
      for (int i = 0; i < poseSplines.Count; i++) {
        drawer.DrawPoseSpline(poseSplines[i]);
      }
    }
  }

  public static class StepperRailExtensions {
    public static Transform GetFirstChild(this Transform t) {
      if (t.childCount == 0) { return null; }
      return t.GetChild(0);
    }
  }

}
