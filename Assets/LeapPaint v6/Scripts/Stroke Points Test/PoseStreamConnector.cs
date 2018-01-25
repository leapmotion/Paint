using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class PoseStreamConnector : MonoBehaviour {

    private MonoBehaviour _poseStream;
    public IStream<Pose> poseStream;

    private MonoBehaviour _poseStreamProcessor;

  }

}
