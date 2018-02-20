using Leap.Unity.Infix;
using Leap.Unity.RuntimeGizmos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.OSControl {

  public class WorldToScreenPoseFilter : MonoBehaviour,
                                         IStreamReceiver<Pose>,
                                         IStream<Pose>,
                                         IRuntimeGizmoComponent {

    public uDesktopDuplication.Texture uddScreenTexture;

    public event Action OnOpen = () => { };
    public event Action<Pose> OnSend = (pose) => { };
    public event Action OnClose = () => { };

    public void Close() {
      OnClose();
    }

    public void Open() {
      OnOpen();
    }

    public Vector3 screen00;
    public Vector3 screen10;
    public Vector3 screen01;

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      drawer.color = LeapColor.amber;

      Rect monitorRect = new Rect(-5f, -5f, 10f, 10f);

      drawer.DrawRect(uddScreenTexture.transform, monitorRect);

      if (Application.isPlaying) {
        var screenTex = uddScreenTexture;
        var u0v0 = screenTex.GetWorldPositionFromCoord(new Vector2(0, 0));
        var u1v0 = screenTex.GetWorldPositionFromCoord(new Vector2(screenTex.monitor.width, 0));
        var u0v1 = screenTex.GetWorldPositionFromCoord(new Vector2(0, screenTex.monitor.height));

        drawer.color = LeapColor.white;
        drawer.DrawWireSphere(u0v0, 0.01f);
        drawer.color = LeapColor.red;
        drawer.DrawWireSphere(u1v0, 0.01f);
        drawer.color = LeapColor.green;
        drawer.DrawWireSphere(u0v1, 0.01f);
      }
    }

    public void Receive(Pose data) {
      Vector3 worldPos = data.position;

      var monitor = uddScreenTexture;
      var u0v0 = monitor.GetWorldPositionFromCoord(new Vector2(0, 0));
      var u1v0 = monitor.GetWorldPositionFromCoord(new Vector2(1, 0)); // one pixel right
      var u0v1 = monitor.GetWorldPositionFromCoord(new Vector2(0, 1)); // one pixel up
      var monitorRight = u1v0 - u0v0;
      var monitorUp = u0v1 - u0v0;

      var posFromMonitor = worldPos - u0v0;

      var screenX = posFromMonitor.Dot(monitorRight) / monitorRight.sqrMagnitude;
      var screenY = posFromMonitor.Dot(monitorUp) / monitorUp.sqrMagnitude;

      this.transform.position = u0v0 + monitorRight * screenX + monitorUp * screenY;

      OnSend(new Pose(new Vector3(screenX, screenY, 0f), data.rotation));
    }
  }

}
