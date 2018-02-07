using System;
using Leap.Unity.Attributes;
using Leap.Unity.Query;
using UnityEngine;

namespace Leap.Unity.Attachments {
  
  [ExecuteInEditMode]
  public class FollowHandPoint : MonoBehaviour, IStream<Pose> {

    #region Inspector

    [SerializeField]
    private LeapProvider _provider;
    public LeapProvider provider {
      get {
        if (_provider == null) { _provider = Hands.Provider; }
        return _provider;
      }
    }

    public Chirality whichHand;
    
    public AttachmentPointFlags attachmentPoint = AttachmentPointFlags.Palm;

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

    [Header("Pose Stream Offset")]

    public bool usePoseStreamOffset = false;

    [DisableIf("usePoseStreamOffset", isEqualTo: false)]
    public Transform poseStreamOffsetSource = null;

    [DisableIf("usePoseStreamOffset", isEqualTo: false)]
    public Pose poseStreamOffset = Pose.identity;

    private bool _isStreamOpen = false;

    #endregion

    #region Unity Events

    private void OnValidate() {
      if (!usePoseStreamOffset) {
        poseStreamOffset = Pose.identity;
      }
      else if (poseStreamOffsetSource != null) {
        poseStreamOffset = poseStreamOffsetSource.ToWorldPose()
                             .From(transform.ToWorldPose());
      }
    }

    void OnEnable() {
      unsubscribeFrameCallback();
      subscribeFrameCallback();
    }

    void OnDisable() {
      unsubscribeFrameCallback();
    }

    private void Update() {
      if (!Application.isPlaying) {
        moveToAttachmentPointNow();
      }
    }

    #endregion

    #region On Frame Event

    private void onUpdateFrame(Frame frame) {
      if (frame == null) Debug.Log("Frame null");

      var hand = frame.Hands.Query()
                            .FirstOrDefault(h => h.IsLeft == (whichHand == Chirality.Left));
      
      if (hand != null) {
        if (!_isHandTracked && Application.isPlaying) {
          // Hand just began tracking, open Pose stream.
          OnOpen();
          _isStreamOpen = true;
        }

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

          var streamPose = new Pose(pointPosition, pointRotation);
          var streamOffset = Pose.identity;
          if (usePoseStreamOffset && poseStreamOffsetSource != null) {
            streamOffset = poseStreamOffsetSource.transform.ToWorldPose()
                             .From(streamPose);
          }

          if (Application.isPlaying) {
            if (!_isStreamOpen) {
              OnOpen();
              _isStreamOpen = true;
            }
            OnSend(streamPose.Then(streamOffset));
          }
        }
      }
      else {
        if (_isHandTracked && Application.isPlaying && _isStreamOpen) {
          // Hand just stopped tracking; close Pose stream.
          OnClose();

          _isStreamOpen = false;
        }

        _isHandTracked = false;
      }
    }

    #endregion

    #region Frame Subscription

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

    #region IStream<Pose>
    
    public event Action OnOpen = () => { };
    public event Action<Pose> OnSend = (pose) => { };
    public event Action OnClose = () => { };

    #endregion

    #region Editor Methods

    private void moveToAttachmentPointNow() {
      onUpdateFrame(provider.CurrentFrame);
    }

    #endregion

  }

}
