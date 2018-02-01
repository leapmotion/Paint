using Leap.Unity.Gestures;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity.Streams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Leap.Unity.Animation;
using Leap.Unity.Attributes;

namespace Leap.Unity.Drawing {

  public class Paintbrush : PoseStreamTransformFollower,
                            IRuntimeGizmoComponent,
                            IStream<StrokePoint> {

    #region Inspector

    [Header("Activation Gesture")]
    [SerializeField]
    private MonoBehaviour _activationGesture;
    public IGesture activationGesture {
      get { return _activationGesture as IGesture; }
    }

    [Header("Eligibility Switch")]
    [SerializeField]
    [ImplementsInterface(typeof(IPropertySwitch))]
    private MonoBehaviour _eligibilitySwitch;
    public IPropertySwitch eligibilitySwitch {
      get { return _eligibilitySwitch as IPropertySwitch; }
    }

    [Header("Brush Settings")]
    public float radius = 0.05f;
    public Color color = Color.white;

    [Header("Brush Tip (Optional)")]
    public Transform tipTransform = null;

    [Header("Debug")]
    public bool drawDebug = false;
    public bool drawDebugIdlePaths = false;

    #endregion

    #region Paintbrush

    public Pose GetLeftEdgePose() {
      return GetLeftEdgePose(transform.ToPose());
    }
    public Pose GetLeftEdgePose(Pose brushPose) {
      var tipPose = GetTipPose(brushPose);
      var edgePosition = tipPose.position + tipPose.rotation * -Vector3.right * radius;
      return new Pose(edgePosition, tipPose.rotation);
    }

    public Pose GetRightEdgePose() {
      return GetRightEdgePose(transform.ToPose());
    }
    public Pose GetRightEdgePose(Pose brushPose) {
      var tipPose = GetTipPose(brushPose);
      var edgePosition = tipPose.position + tipPose.rotation * -Vector3.right * radius;
      return new Pose(edgePosition, tipPose.rotation);
    }

    public Pose GetTipPose() {
      return GetTipPose(transform.ToPose());
    }
    public Pose GetTipPose(Pose brushPose) {
      var tipOffset = Pose.identity;
      if (tipTransform != null) {
        tipOffset = tipTransform.ToPose().From(transform.ToPose());
      }
      return brushPose.Then(tipOffset);
    }

    #endregion

    #region Unity Events

    protected virtual void OnEnable() {
      if (eligibilitySwitch != null && activationGesture != null) {
        if (!activationGesture.isEligible) {
          eligibilitySwitch.OffNow();
        }
      }
    }

    protected virtual void Update() {

      if (eligibilitySwitch != null && activationGesture != null) {
        var shouldBeOn = activationGesture.isEligible;
        if (eligibilitySwitch.GetIsOnOrTurningOn() && !shouldBeOn) {
          eligibilitySwitch.Off();
        }
        else if (eligibilitySwitch.GetIsOffOrTurningOff() && shouldBeOn) {
          eligibilitySwitch.On();
        }
      }

    }

    #endregion

    #region IStream<StrokePoint>
    
    public event Action OnOpen  = () => { };
    public event Action<StrokePoint> OnSend = (strokePoint) => { };
    public event Action OnClose = () => { };

    private bool _isStreamOpen = false;

    #endregion

    #region PoseStreamTransformFollower

    public override void Open() {
      base.Open();

      _debugPoseBuffer.Clear();
    }

    public override void Receive(Pose data) {
      base.Receive(data);

      bool shouldBePainting = false;
      if (this.enabled && this.gameObject.activeInHierarchy) {
        shouldBePainting = activationGesture != null && activationGesture.isActive;

        /* Debug */ { 
          var tipPose = GetTipPose(data);
          _debugPoseBuffer.Add(tipPose);
          _debugActivatedBuffer.Add(shouldBePainting);
        }
      }

      if (shouldBePainting) {
        if (!_isStreamOpen) {
          OnOpen();
          _isStreamOpen = true;
        }

        var tipPose = GetTipPose(data);
        OnSend(new StrokePoint() {
          pose = tipPose,
          radius = radius,
          color = color
        });
      }
      else if (_isStreamOpen) {
        OnClose();
        _isStreamOpen = false;
      }
    }

    #endregion

    #region Runtime Gizmos

    private const int DEBUG_BUFFER_SIZE = 16;
    private RingBuffer<Pose> _debugPoseBuffer = new RingBuffer<Pose>(DEBUG_BUFFER_SIZE);
    private RingBuffer<bool> _debugActivatedBuffer = new RingBuffer<bool>(DEBUG_BUFFER_SIZE);

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (!this.enabled || !this.gameObject.activeInHierarchy || !drawDebug) return;

      drawer.color = LeapColor.purple;

      var poseRadius = 0.004f;

      if (!Application.isPlaying) {
        _debugPoseBuffer.Clear();
        _debugPoseBuffer.Add(GetTipPose());
        _debugActivatedBuffer.Clear();
        _debugActivatedBuffer.Add(false);
      }
      for (int i = 0; i < _debugPoseBuffer.Length; i++) {
        var pose = _debugPoseBuffer.Get(i);
        var isActive = _debugActivatedBuffer.Get(i);

        var a = pose.position + pose.rotation * Vector3.right * radius;
        var b = pose.position - pose.rotation * Vector3.right * radius;

        var multiplier = 1f;
        if (isActive) multiplier = 2.5f;

        if (isActive || drawDebugIdlePaths || !Application.isPlaying) {
          drawer.DrawPose(new Pose(a, pose.rotation), poseRadius * multiplier);
          drawer.DrawPose(new Pose(b, pose.rotation), poseRadius * multiplier);
        }
      }
    }

    #endregion

  }

}