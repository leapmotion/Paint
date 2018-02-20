using UnityEngine;

namespace Leap.Unity.OSControl {

  public class SetCursorPoseReceiver : MonoBehaviour,
                                       IStreamReceiver<Pose> {

    public void Close() {

    }

    public void Open() {

    }

    public void Receive(Pose data) {
      if (this.enabled && gameObject.activeInHierarchy) {
        CursorControl.SetCursorPos((int)data.position.x, (int)data.position.y);
      }
    }

  }

}
