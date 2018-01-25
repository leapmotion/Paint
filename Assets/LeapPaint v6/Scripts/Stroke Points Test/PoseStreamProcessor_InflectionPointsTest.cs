using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class PoseStreamProcessor_InflectionPointsTest : MonoBehaviour,
                                                          IStreamReceiver<Pose>,
                                                          IStream<Pose> {

    public float maxSkipDistance = 0.10f;
    public float maxSkipAngle = 5f;

    private Pose? _lastOutputPose = null;
    private List<Pose> _skippedPoses = new List<Pose>();

    // Stream<Pose> events
    public event Action OnOpen = () => { };
    public event Action<Pose> OnSend = (pose) => { };
    public event Action OnClose = () => { };

    public void Open() {
      _lastOutputPose = null;
      _skippedPoses.Clear();

      OnOpen();
    }

    public void Receive(Pose pose) {
      Pose? outputPose = null;

      if (!_lastOutputPose.HasValue) {

      }
      
      if (outputPose.HasValue) {
        OnSend(outputPose.Value);
      }
    }

    public void Close() {
      _lastOutputPose = null;
      _skippedPoses.Clear();

      OnClose();
    }

  }

}
