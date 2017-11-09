

namespace Leap.Unity.DevCommands {

  public enum DevCommandType {
    Recenter
  }

  public static class DevCommand {

    public static void Recenter() {
      UnityEngine.XR.InputTracking.Recenter();
    }
    
    public static void Invoke(DevCommandType type) {
      switch (type) {
        case DevCommandType.Recenter:
          Recenter();
          break;
      }
    }

  }

}
