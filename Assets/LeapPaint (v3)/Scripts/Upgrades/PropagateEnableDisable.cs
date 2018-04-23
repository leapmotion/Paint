using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LeapPaint {

  /// <summary>
  /// When enabled, enables targets. When disabled, disables targets.
  /// </summary>
  public class PropagateEnableDisable : MonoBehaviour {

    public List<GameObject> targets;

    private void OnEnable() {
      foreach (var target in targets) {
        target.SetActive(true);
      }
    }

    private void OnDisable() {
      foreach (var target in targets) {
        target.SetActive(false);
      }
    }

  }

}
