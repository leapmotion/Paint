using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity.Attachments {
  
  public class FollowHandPoint : MonoBehaviour {

    #region Inspector

    public Chirality whichHand;

    [QuickButton("Move Here", "moveToAttachmentPointNow")]
    public AttachmentPointFlags attachmentPoint;

    public enum FollowMode { Update, FixedUpdate }
    private FollowMode _followMode = FollowMode.Update;
    public FollowMode followMode {
      get {
        return _followMode;
      }
      set {
        if (value != _followMode) {
          unsubscribeFrameCallback();

          _followMode = value;

          subscribeFrameCallback();
        }
      }
    }

    [SerializeField]
    [Disable]
    private bool _isHandTracked = false;
    public bool isHandTracked { get { return _isHandTracked; } }

    #endregion

    #region Unity Events

    void OnEnable() {
      unsubscribeFrameCallback();
      subscribeFrameCallback();
    }

    void OnDisable() {
      unsubscribeFrameCallback();
    }

    #endregion

    #region On Frame Event
    
    private void onUpdateFrame(Frame frame) {
      if (frame == null) Debug.Log("Frame null");

      var hand = Hands.Get(whichHand);

      if (hand == null) {
        
      }

      if (hand != null) {
        _isHandTracked = true;

        if (enabled && gameObject.activeInHierarchy) {
          Vector3 pointPosition; Quaternion pointRotation;
          AttachmentPointBehaviour.GetLeapHandPointData(hand, attachmentPoint,
                                                        out pointPosition,
                                                        out pointRotation);

          this.transform.position = pointPosition;
          this.transform.rotation = pointRotation;
        }
      }
      else {
        _isHandTracked = false;
      }
    }

    #endregion

    #region Frame Subscription

    private void unsubscribeFrameCallback() {
      if (Hands.Provider != null) {
        switch (_followMode) {
          case FollowMode.Update:
            Hands.Provider.OnUpdateFrame -= onUpdateFrame;
            break;
          case FollowMode.FixedUpdate:
            Hands.Provider.OnFixedFrame -= onUpdateFrame;
            break;
        }
      }
    }

    private void subscribeFrameCallback() {
      if (Hands.Provider != null) {
        switch (_followMode) {
          case FollowMode.Update:
            Hands.Provider.OnUpdateFrame += onUpdateFrame;
            break;
          case FollowMode.FixedUpdate:
            Hands.Provider.OnFixedFrame += onUpdateFrame;
            break;
        }
      }
    }

    #endregion

    #region Editor Methods

    private void moveToAttachmentPointNow() {
      Debug.Log("Trying to move via frame now");

      //onUpdateFrame(TestHandFactory.Hands.Provider.editTimePose);
    }

    #endregion

  }

}
