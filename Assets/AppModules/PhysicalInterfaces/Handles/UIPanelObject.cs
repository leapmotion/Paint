using Leap.Unity.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class UIPanelObject : MonoBehaviour {

    public HandledObject handledObj;

    public bool flip180 = false;

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
      if (handledObj.isHeld) {
        var target = handledObj.targetPose;
        var pivot = handledObj.heldHandle.targetPose;

        handledObj.targetPose = PivotLook.Solve(target,
                                                pivot.From(target),
                                                Camera.main.transform.position,
                                                Camera.main.transform.parent.up,
                                                flip180: flip180);
      }
    }

  }

}
