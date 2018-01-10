using UnityEngine;
using System.Collections.Generic;
using System;

namespace Leap.Unity {

  public class DevCommandsRunner : MonoBehaviour {

    public const string DEV_COMMANDS_RUNNER_NAME = "__Dev Commands Runner__";

    private static Dictionary<string, Action<Vector3>> s_gesturePositionCommands
             = new Dictionary<string, Action<Vector3>>();

    private static Dictionary<string, Action> s_noArgCommands
             = new Dictionary<string, Action>();

    private static DevCommandsRunner s_runnerInstance = null;

    [RuntimeInitializeOnLoadMethod]
    private static void RuntimeInitializeOnLoad() {
      var runnerObj = new GameObject(DEV_COMMANDS_RUNNER_NAME);
      s_runnerInstance = runnerObj.AddComponent<DevCommandsRunner>();
    }

    public static void Register(string commandName,
                                Action<Vector3> actionWithGesturePosition) {
      s_gesturePositionCommands[commandName] = actionWithGesturePosition;
    }

    public static void Register(string commandName,
                                Action commandAction) {
      s_noArgCommands[commandName] = commandAction;
    }

  }

}

