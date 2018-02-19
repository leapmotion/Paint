using Leap.Unity.Animation;
using Leap.Unity.UserContext;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LeapPaint {

  public class Tool_ModeActivator : MonoBehaviour {

    [Tooltip("If true, when this behaviour receives Start(), it will set the mode string "
           + "at its channel to the state specified by the activeMode field.")]
    public bool setModeOnStart = false;

    public string activeMode = "paint";

    [Header("Ucon Mode Channel In")]
    public StringChannel modeChannel = new StringChannel("tool");

    private void Start() {
      Updater.singleton.OnUpdate += onUpdaterUpdate;

      if (setModeOnStart) {
        modeChannel.Set(activeMode);
      }
    }

    /// <summary>
    /// The use of an Updater callback here allows this MonoBehaviour to control the
    /// "active" or "inactive" state of its GameObject dynamically -- it will receive
    /// onUpdaterUpdate even if its GameObject is disabled.
    /// </summary>
    private void onUpdaterUpdate() {
      var shouldBeActive = activeMode.Equals(modeChannel.Get());

      if ((shouldBeActive && !gameObject.activeSelf)
          || (!shouldBeActive && gameObject.activeSelf)) {
        gameObject.SetActive(shouldBeActive);
      }
    }

  }

}
