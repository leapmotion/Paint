using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity.Query;
using Leap.Unity.Attributes;
using Leap.Unity.Splines;
using Leap.Unity.PhysicalInterfaces;
using Leap.Unity.Layout;
using Leap.Unity.Streams;

namespace Leap.Unity {

  public class TestStrokeGizmos : MonoBehaviour, IRuntimeGizmoComponent {

    [ImplementsInterface(typeof(IIndexable<Pose>))]
    [SerializeField]
    private MonoBehaviour _strokePoses;
    public IIndexable<Pose> strokePoses {
      get { return _strokePoses as IIndexable<Pose>; }
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      drawer.color = LeapColor.jade;

      Debug.Log("There are " + strokePoses.Count + " poses, so we should have that many "
                + "minus one WithPrevious.");

      // THIS works
      Pose? prevPose = null;
      int maybePoseCount = 0;
      foreach (var pose in strokePoses.Query()) {
        if (prevPose != null) {
          drawer.DrawLine(prevPose.Value.position, pose.position);
          drawer.DrawWireCapsule(prevPose.Value.position, pose.position, 0.005f);
          maybePoseCount++;
        }

        prevPose = pose;
      }
      Debug.Log("Non-WithPrevious strategy count: " + maybePoseCount);

      // THIS doesn't work
      int strokePosesQueryCount = 0;
      foreach (var prevPair in strokePoses.Query().WithPrevious()) {
        strokePosesQueryCount++;
      }
      Debug.Log("IIndexable<Pose> WithPrevious count: " + strokePosesQueryCount);

      // THIS works! WithPrevious does work on a List<>
      // this is a List<Transform> with the same pose sources as the strokePoses
      var testTransforms = (_strokePoses as TestChildPoseSequenceProvider).testTransforms;
      int listTransformCount = 0;
      foreach (var prevPair in testTransforms.Query().WithPrevious()) {
        listTransformCount++;
      }
      Debug.Log("Count (List<Transform>): " + listTransformCount);

    }

  }

}