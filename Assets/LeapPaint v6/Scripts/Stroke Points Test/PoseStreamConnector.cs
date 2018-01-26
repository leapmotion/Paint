using Leap.Unity.Attributes;
using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  [ExecuteInEditMode]
  public class PoseStreamConnector : MonoBehaviour, IRuntimeGizmoComponent {

    [SerializeField]
    [ImplementsInterface(typeof(IStream<Pose>))]
    private MonoBehaviour _stream;
    public IStream<Pose> stream {
      get { return _stream as IStream<Pose>; }
    }

    [SerializeField]
    [ImplementsInterface(typeof(IStreamReceiver<Pose>))]
    private MonoBehaviour _receiver;
    public IStreamReceiver<Pose> receiver {
      get { return _receiver as IStreamReceiver<Pose>; }
    }

    [OnEditorChange("resetConnection")]
    public bool debugWire = false;

    private void resetConnection() {
      OnDisable();
      OnEnable();
    }

    private void OnEnable() {
      if (stream != null && receiver != null) {
        stream.OnOpen -= receiver.Open;
        stream.OnOpen += receiver.Open;

        stream.OnClose -= receiver.Close;
        stream.OnClose += receiver.Close;

        stream.OnSend -= receiver.Receive;
        stream.OnSend += receiver.Receive;

        #if UNITY_EDITOR
        if (debugWire) {
          stream.OnOpen -= debugOnOpen;
          stream.OnOpen += debugOnOpen;

          stream.OnClose -= debugOnClose;
          stream.OnClose += debugOnClose;

          stream.OnSend -= debugOnSend;
          stream.OnSend += debugOnSend;
        }
        #endif
      }
    }

    private void debugOnOpen() {
      Debug.Log("Wire " + this.name + " received OnOpen.");
    }
    private void debugOnSend(Pose pose) {
      Debug.Log("Wire " + this.name + " received OnSend: " + pose);
    }
    private void debugOnClose() {
      Debug.Log("Wire " + this.name + " received OnClose.");
    }

    private void OnDisable() {
      if (stream != null && receiver != null) {
        stream.OnOpen -= receiver.Open;

        stream.OnClose -= receiver.Close;

        stream.OnSend -= receiver.Receive;
      }

      #if UNITY_EDITOR
      if (stream != null) {
        stream.OnOpen -= debugOnOpen;
        stream.OnClose -= debugOnClose;
        stream.OnSend -= debugOnSend;
      }
      #endif
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (!this.enabled || !this.gameObject.activeInHierarchy) return;

      if (_stream != null && _receiver != null) {
        drawer.color = LeapColor.white.WithAlpha(0.3f);

        var a = _stream.transform.position;
        var b = _receiver.transform.position;
        drawer.DrawLine(a, b);

        if (debugWire) {
          drawer.color = LeapColor.magenta.Lerp(LeapColor.white, 0.4f);
          drawer.DrawDashedLine(a, b, segmentsPerMeter: 128);
        }
      }
    }

  }

}
