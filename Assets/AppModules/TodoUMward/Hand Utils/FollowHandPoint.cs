using Leap.Unity.Attributes;
using Leap.Unity.Query;
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

      var hand = frame.Hands.Query()
                            .FirstOrDefault(h => h.IsLeft == (whichHand == Chirality.Left));
      
      if (hand != null) {
        _isHandTracked = true;

        if (enabled && gameObject.activeInHierarchy) {
          Vector3 pointPosition; Quaternion pointRotation;
          AttachmentPointBehaviour.GetLeapHandPointData(hand, attachmentPoint,
                                                        out pointPosition,
                                                        out pointRotation);

          // Replace wrist rotation data with that from the palm for now.
          if (attachmentPoint == AttachmentPointFlags.Wrist) {
            Vector3 unusedPos;
            AttachmentPointBehaviour.GetLeapHandPointData(hand, AttachmentPointFlags.Palm,
                                                          out unusedPos,
                                                          out pointRotation);
          }

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

    private LeapProvider _provider;

    private void unsubscribeFrameCallback() {
      if (_provider != null) {
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
      if (_provider == null) _provider = Hands.Provider;

      if (_provider != null) {
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
      onUpdateFrame(Hands.Provider.CurrentFrame);
    }

    #endregion

  }

}
