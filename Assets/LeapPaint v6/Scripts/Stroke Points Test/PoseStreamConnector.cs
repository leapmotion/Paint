using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class PoseStreamConnector : MonoBehaviour {

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

    private void OnEnable() {
      if (stream != null && receiver != null) {
        stream.OnOpen -= receiver.Open;
        stream.OnOpen += receiver.Open;

        stream.OnClose -= receiver.Close;
        stream.OnClose += receiver.Close;

        stream.OnSend -= receiver.Receive;
        stream.OnSend += receiver.Receive;
      }
    }

    private void OnDisable() {
      if (stream != null && receiver != null) {
        stream.OnOpen -= receiver.Open;

        stream.OnClose -= receiver.Close;

        stream.OnSend -= receiver.Receive;
      }
    }

  }

}
