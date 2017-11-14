using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class UIPanelObject : MonoBehaviour {

    public HandledObject handledObj;

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

      // TODO: Yeah, this doesn't work.

      // Best solve is with both handles.

      handledObj.targetPose = target.WithRotation(
        Utils.FaceTargetWithoutTwist(target.position,
          Camera.main.transform.position));
    }

  }

}
