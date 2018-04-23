using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LeapPaint {

  public class SetLocalPositionWhenEnabled : MonoBehaviour {

    public Transform target;

    [QuickButton("Use Current", "setCurrentLocalPositionWhenEnabled")]
    public Vector3 localPositionWhenEnabled = Vector3.zero;

    [QuickButton("Use Current", "setCurrentLocalPositionWhenDisabled")]
    public Vector3 localPositionWhenDisabled = Vector3.zero;

    private void OnEnable() {
      if (target != null) {
        target.localPosition = localPositionWhenEnabled;
      }
    }

    private void OnDisable() {
      if (target != null) {
        target.localPosition = localPositionWhenDisabled;
      }
    }

    private void setCurrentLocalPositionWhenEnabled() {
      if (target != null) {
        localPositionWhenEnabled = target.localPosition;
      }
    }

    private void setCurrentLocalPositionWhenDisabled() {
      if (target != null) {
        localPositionWhenDisabled = target.localPosition;
      }
    }

  }

}
