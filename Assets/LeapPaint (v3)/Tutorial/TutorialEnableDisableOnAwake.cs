using System;
using UnityEngine;

namespace Leap.Unity.LeapPaint_v3 {

  public class TutorialEnableDisableOnAwake : MonoBehaviour {

    public TutorialControl tutorialControl;

    [Header("Enabled State on Awake (overridden by flags)")]
    public bool tutorialEnabledOnAwake = true;
    public string enableTutorialFlag = "--enable-tutorial";
    public string disableTutorialFlag = "--disable-tutorial";

    private bool _shouldTutorialBeEnabled = true;

    private void Awake() {
      _shouldTutorialBeEnabled = tutorialEnabledOnAwake;

      string[] arguments = Environment.GetCommandLineArgs();
      for (int i = 0; i < arguments.Length; i++) {
        if (arguments.Equals(enableTutorialFlag)) {
          _shouldTutorialBeEnabled = true;
        }
        if (arguments.Equals(disableTutorialFlag)) {
          _shouldTutorialBeEnabled = false;
        }
      }

      if (tutorialControl != null) {
        if (_shouldTutorialBeEnabled) {
          tutorialControl.EnableTutorial();
        }
        else {
          tutorialControl.DisableTutorial();
        }
      }
    }

  }

}
