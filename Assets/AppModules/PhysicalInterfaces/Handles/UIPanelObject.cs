using Leap.Unity.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class UIPanelObject : MonoBehaviour {

    public HandledObject handledObj;

    [SerializeField]
    [ImplementsInterface(typeof(IHandle))]
    private MonoBehaviour _handle;
    public IHandle handle {
      get { return _handle as IHandle; }
    }

    void OnEnable() {
      handledObj.OnUpdateTarget -= _onHandleUpdateTargetAction;
      handledObj.OnUpdateTarget += _onHandleUpdateTargetAction;
    }

    void OnDisable() {
      handledObj.OnUpdateTarget -= _onHandleUpdateTargetAction;
    }

    private Action _backingOnHandleUpdateTargetAction = null;
    private Action _onHandleUpdateTargetAction {
      get {
        if (_backingOnHandleUpdateTargetAction == null) {
          _backingOnHandleUpdateTargetAction = onHandleUpdateTarget;
        }
        return _backingOnHandleUpdateTargetAction;
      }
    }
    private void onHandleUpdateTarget() {
      var target = handledObj.targetPose;

      var pivot = handle.pose;

      handledObj.targetPose = PivotLook.Solve(handledObj.pose,
                                              pivot.From(handledObj.pose),
                                              handle.targetPose.position,
                                              Camera.main.transform.position,
                                              Camera.main.transform.parent.up);
    }

    //public static Quaternion PivotLookRotation(Pose lookerPose,
    //                                           Vector3 pivotAroundPoint,
    //                                           Vector3 lookAtTarget,
    //                                           bool flip180 = false,
    //                                           Maybe<Vector3> horizonNormal
    //                                             = default(Maybe<Vector3>)) {

    //}

  }

}
