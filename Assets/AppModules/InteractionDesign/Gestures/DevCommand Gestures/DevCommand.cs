using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class DevCommands : MonoBehaviour {

    private static Dictionary<string, Action<Vector3>> s_commands
             = new Dictionary<string, Action<Vector3>>();

    public static void RegisterCommand(string commandName,
                                       Action<Vector3> commandAction) {
      ensureRunnerExists();

      s_commands[commandName] = commandAction;
    }

    public const string DEV_COMMANDS_RUNNER_NAME = "__Dev Commands Runner__";
    
    private static DevCommands s_devCommandsRunner;

    private static void ensureRunnerExists() {
      var devCommandsRunnerObj = new GameObject();
      s_devCommandsRunner
    }

  }

  public enum DevCommandType {
    Recenter,
    LaunchLiveUI
  }

  public static class DevCommand {

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
