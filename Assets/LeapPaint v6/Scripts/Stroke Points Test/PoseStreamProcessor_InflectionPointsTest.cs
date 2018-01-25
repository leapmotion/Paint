using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public interface IStream<T> {

    event Action<T> OnStream;

  }

  public interface IStreamReceiver<T> {

    void Open();

    void Receive(T data);

    void Close();

  }

  public class PoseStreamProcessor_InflectionPointsTest : MonoBehaviour,
                                                          IStreamReceiver<Pose> {

    public float maxSkipDistance = 0.10f;
    public float maxSkipAngle = 5f;

    private Pose? _lastOutputPose = null;
    private List<Pose> _skippedPoses = new List<Pose>();

    public void Open() {

    }

    public void Close() {

    }

    public void Receive(Pose pose) {

    }

  }

}
