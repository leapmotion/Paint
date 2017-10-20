﻿using System.Collections;
using System.Collections.Generic;
using Leap.Unity.PhysicalInterfaces;
using UnityEngine;

namespace Leap.Unity.Layout {

  public class SimpleCameraFacingPoseProvider : MonoBehaviour,
                                                IPoseProvider {

    public bool flipPose = false;

    public Vector3 GetTargetPosition() {
      return this.transform.position;
    }

    public Quaternion GetTargetRotation() {
      return NewUtils.FaceTargetWithoutTwist(this.transform.position,
                                             Camera.main.transform.position,
                                             flipPose);
    }
  }

}
