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

    public Color rawLinesColor = LeapColor.jade.WithAlpha(0.4f);

    [ImplementsInterface(typeof(IIndexable<Pose>))]
    [SerializeField]
    private MonoBehaviour _strokePoses;
    public IIndexable<Pose> strokePoses {
      get { return _strokePoses as IIndexable<Pose>; }
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      // Raw Lines
      drawer.color = rawLinesColor;
      foreach (var prevPair in strokePoses.Query().WithPrevious()) {
        var pose = prevPair.value;
        var prevPose = prevPair.prev;

        drawer.DrawLine(prevPose.position, pose.position);
      }


    }

  }

}