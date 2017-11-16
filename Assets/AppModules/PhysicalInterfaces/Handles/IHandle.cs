using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public delegate Pose FilterPoseEvent(Pose curPose, Pose newPose);
  public delegate Pose FilterHandlePoseEvent(IHandle handle, Pose curPose, Pose newPose);

  public interface IHandle {

    Pose targetPose { get; }
    event FilterPoseEvent OnSetTarget;
    event FilterHandlePoseEvent OnHandleSetTarget;

  }
  
  public delegate Pose FilterObjectPoseEvent(IHandledObject handledObject,
                                             IHandle handle,
                                             )

  public interface IHandledObject {



  }

}
