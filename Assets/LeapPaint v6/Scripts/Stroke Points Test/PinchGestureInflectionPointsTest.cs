using Leap.Unity.Attributes;
using Leap.Unity.Gestures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class PoseGestureInflectionPointsTest : MonoBehaviour {

    [SerializeField]
    [ImplementsInterface(typeof(IPoseGesture))]
    private MonoBehaviour _poseGesture;
    public IPoseGesture poseGesture {
      get { return _poseGesture as IPoseGesture; }
    }

    public float maxSkipDistance = 0.10f;
    public float maxSkipAngle = 5f;

    private Pose? _lastOutputPose = null;
    private List<Pose> _skippedPoses = new List<Pose>();

    void Update() {
      if (poseGesture.isActive) {
        var pose = poseGesture.pose;

        bool shouldOutput = false;
        if (!_lastOutputPose.HasValue) {
          shouldOutput = true;
        }
        else {

        }
      }
      else if (poseGesture.wasDeactivated) {

      }
    }

    private void outputPose(Pose pose) {
      _lastOutputPose = pose;
    }

  }

}
