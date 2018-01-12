using UnityEngine;
using Leap;
using Leap.Unity;
using Leap.Unity.Networking;
using UnityEngine.Networking;

namespace Leap.Unity.Networking {
  public class LeapUNETProvider : NetworkBehaviour {
    public const string F_N = "LeapUNETProvider";

    public FrameEncodingEnum FrameEncodingType;
    public Transform HandController;
    LeapServiceProvider LeapDataProvider;
    LeapStreamingProvider NetworkDataProvider;
    float lastUpdate = 0f;
    float interval = 0.035f;
    byte[] handData;
    [HideInInspector]
    public FrameEncoding playerState;

    // Use this for initialization
    void Start() {
      //Application.targetFrameRate = 60;
      if (isLocalPlayer) {
        switch (FrameEncodingType) {
          case FrameEncodingEnum.VectorHand:
            playerState = new VectorFrameEncoding();
            break;
          case FrameEncodingEnum.CurlHand:
            playerState = new CurlFrameEncoding();
            break;
          default:
            playerState = new VectorFrameEncoding();
            break;
        }
        LeapDataProvider = HandController.gameObject.AddComponent<LeapServiceProvider>();
        //ENABLE THESE AGAIN ONCE THE SERVICE HAS THESE EXPOSED SOMEHOW
        //LeapDataProvider._temporalWarping = HandController.parent.GetComponent<LeapVRTemporalWarping>();
        //LeapDataProvider._temporalWarping.provider = LeapDataProvider;
        //LeapDataProvider._isHeadMounted = true;
        LeapDataProvider.UpdateHandInPrecull = true;
      } else {
        NetworkDataProvider = HandController.gameObject.AddComponent<LeapStreamingProvider>();
        Destroy(HandController.parent.GetComponent<LeapVRTemporalWarping>());
        switch (FrameEncodingType) {
          case FrameEncodingEnum.VectorHand:
            playerState = new VectorFrameEncoding();
            NetworkDataProvider.lerpState = new VectorFrameEncoding();
            NetworkDataProvider.prevState = new VectorFrameEncoding();
            NetworkDataProvider.currentState = new VectorFrameEncoding();
            break;
          case FrameEncodingEnum.CurlHand:
            playerState = new CurlFrameEncoding();
            NetworkDataProvider.lerpState = new CurlFrameEncoding();
            NetworkDataProvider.prevState = new CurlFrameEncoding();
            NetworkDataProvider.currentState = new CurlFrameEncoding();
            break;
          default:
            playerState = new VectorFrameEncoding();
            NetworkDataProvider.lerpState = new VectorFrameEncoding();
            NetworkDataProvider.prevState = new VectorFrameEncoding();
            NetworkDataProvider.currentState = new VectorFrameEncoding();
            break;
        }
      }
      HandController.gameObject.AddComponent<LeapHandController>();
      playerState.fillEncoding(null);
    }

    [ClientRpc(channel = 1)]
    void RpcsetState(byte[] data) {
      if (!isLocalPlayer) {
        handData = data;
        if (playerState != null && handData != null) {
          playerState.fillEncoding(handData);
          if (NetworkDataProvider) {
            NetworkDataProvider.AddFrameState(playerState); //Enqueue new tracking data on an interval for everyone else
          }
        }
      }
      return;
    }

    [Command(channel = 1)]
    void CmdsetState(byte[] data) {
      handData = data;
      playerState.fillEncoding(handData);
      RpcsetState(playerState.data);
    }

    void Update() {
      using (new ProfilerSample("LeapUNET Update", this)) {
        if (isLocalPlayer && (!isServer || (isServer && NetworkManager.singleton.numPlayers > 1))) {
          if (Time.realtimeSinceStartup > lastUpdate + interval) {
            lastUpdate = Time.realtimeSinceStartup;
            playerState.fillEncoding(LeapDataProvider.CurrentFrame, HandController);
            CmdsetState(playerState.data);
          }
        }
      }
    }
  }
}