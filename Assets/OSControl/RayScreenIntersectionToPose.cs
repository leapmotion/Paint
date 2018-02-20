using Leap.Unity.RuntimeGizmos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.OSControl {

  public class RayScreenIntersectionToPose : MonoBehaviour,
                                             IStreamReceiver<Ray>,
                                             IStream<Pose>,
                                             IRuntimeGizmoComponent {

    public bool drawDebug = false;

    public uDesktopDuplication.Texture uddTexture;

    public event Action OnOpen = () => { };
    public event Action<Pose> OnSend = (ray) => { };
    public event Action OnClose = () => { };

    public void Close() {
      OnClose();
    }

    public void Open() {
      OnOpen();
    }

    private Pose? _lastHit = null;

    public void Receive(Ray data) {
      Pose? screenHit = null;
      if (uddTexture != null) {
        var result = uddTexture.RayCast(data.origin, data.direction);
        if (result.hit) {
          var pos = result.position;
          var normal = result.normal;
          var rot = Quaternion.LookRotation(normal.Perpendicular(), normal);
          screenHit = new Pose(pos, rot);
          _lastHit = screenHit;
        }
      }

      if (screenHit.HasValue) {
        OnSend(screenHit.Value);
      }
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (drawDebug && _lastHit.HasValue) {
        drawer.color = LeapColor.coral;

        var lastHit = _lastHit.Value;
        drawer.PushMatrix();
        drawer.matrix = Matrix4x4.TRS(lastHit.position, lastHit.rotation, Vector3.one);
        drawer.DrawWireCube(Vector3.zero, new Vector3(0.01f, 0.05f, 0.01f));
        drawer.PopMatrix();
      }
    }

  }

}
