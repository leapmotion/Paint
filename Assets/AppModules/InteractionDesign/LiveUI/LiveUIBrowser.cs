using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LiveUI {

  public class Browser {

    #region Dev Command Registration

    public const string LAUNCH_COMMAND_NAME = "Launch LiveUI Browser";

    private static void RuntimeInitializeLiveUI() {

      // DevCommand registration
      DevCommand.Register(LAUNCH_COMMAND_NAME,
                          typeof(LiveUIBrowserGesture),
                          Launch);

    }

    public static Promise<Browser> LaunchNew(Vector3 position) {

    }

    #endregion

  }

}