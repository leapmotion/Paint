using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class PoseStreamProcessor_InflectionPointsTest : MonoBehaviour,
                                                          IStreamReceiver<Pose> {

    public float maxSkipDistance = 0.10f;
    public float maxSkipAngle = 5f;

    private Pose? _lastOutputPose = null;
    private List<Pose> _skippedPoses = new List<Pose>();

    public void Open() {
      _lastOutputPose = null;
      _skippedPoses.Clear();
    }

    public void Receive(Pose pose) {
      
      if (!_lastOutputPose.HasValue) {

      }

    }

    public void Close() {
      _lastOutputPose = null;
      _skippedPoses.Clear();
    }

  }

}
