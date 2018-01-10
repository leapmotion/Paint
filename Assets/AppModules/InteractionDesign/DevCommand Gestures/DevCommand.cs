using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class DevCommand {

    public static void Register(string commandName,
                                Action<Vector3> actionWithGesturePosition) {
      DevCommandsRunner.Register(commandName, actionWithGesturePosition);
    }

    public static void Register(string commandName,
                                Action commandAction) {
      DevCommandsRunner.Register(commandName, commandAction);
    }

  }

  public enum DevCommandType {
    Recenter,
    LaunchLiveUI
  }

  public static class DevCommandStatic {

    public static void Recenter() {
      UnityEngine.XR.InputTracking.Recenter();
    }

    public static void LaunchLiveUI() {

    }

    public static void Invoke(DevCommandType type) {
      switch (type) {
        case DevCommandType.Recenter:
          Recenter();
          break;
        case DevCommandType.LaunchLiveUI:
          LaunchLiveUI();
          break;
      }
    }

  }

}
