using Leap.Unity;
using Leap.Unity.Promises;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LiveUI {

  public class Browser {

    #region Dev Command Gesture

    public const string LAUNCH_COMMAND_NAME = "Launch LiveUI Browser";

    [RuntimeInitializeOnLoadMethod]
    private static void RuntimeInitializeOnLoad() {

      // Register the LiveUIBrowserGesture with the associated name and action.
      DevCommandGesture.Register(LAUNCH_COMMAND_NAME,
                                 typeof(LiveUIBrowserGesture),
                                 LaunchNew);

    }

    #endregion

    public static Promise<Browser> LaunchNew(Vector3 atPosition) {
      return Promise.ToReturn<Browser>(constructBrowser)
                    .WithArgs(atPosition)
                    .OnThread(ThreadType.UnityThread)
                    .Otherwise(notifyBrowserLaunchException);
    }

    private static Browser constructBrowser() {
      return new Browser();
    }

    private static void notifyBrowserLaunchException(Exception e) {
      throw e;
    }

    private Browser() {
      
    }

  }

}