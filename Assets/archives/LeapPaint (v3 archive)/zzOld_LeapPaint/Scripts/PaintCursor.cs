using UnityEngine;
using System.Collections;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity;
using Leap.Unity.Attributes;
using Leap.Unity.Gestures;
using Leap.Unity.Infix;

namespace Leap.Unity.LeapPaint_v3 {

  public class PaintCursor : MonoBehaviour {

    [Header("Pinch Detection Mode")]

    public PinchDetectionMode pinchMode = PinchDetectionMode.PinchGesture;

    public enum PinchDetectionMode {
      PinchGesture,
      PinchDetector
    }

    [Header("Pinch Gesture")]

    public PinchGesture pinchGesture;


    [Header("Pinch Detector")]

    public PinchDetector pinchDetector;


    [Header("Cursor Following")]
    
    public CursorFollowType cursorFollowType = CursorFollowType.Dynamic;

    public enum CursorFollowType {
      Rigid,
      Dynamic
    }
    
    public void SetCursorFollowRigid() {
      cursorFollowType = CursorFollowType.Rigid;
    }
    public void SetCursorFollowDynamic() {
      cursorFollowType = CursorFollowType.Dynamic;
    }


    [Header("Misc")]

    public RectToroid _rectToroidPinchTarget;
    public MeshRenderer _rectToroidPinchTargetRenderer;
    public RectToroid _rectToroidPinchState;
    public MeshRenderer _rectToroidPinchStateRenderer;
    public IndexTipColor _indexTipColor;
    public CapsuleHand capsuleHand;
    public Material _ghostableHandMat;
    public Material _nonGhostableHandMat;
    public Renderer _indexTipColorRenderer;

    [HideInInspector]
    public HandModelBase _handModel;

    private float _thicknessMult = 1.5F;
    private float _radius = 0F;
    private float _minRadius = 0.02F;
    private float _maxRadius = 0.03F;
    private Color _cursorColor = Color.white;
    private Color _drawBeginMarkerCircleColor = Color.white;
    private bool _isPaintingPossible = true;
    private bool _canBeginPainting = true;
    private bool _isPainting = false;
    private float _smoothedHandAlpha = 1F;

    public Vector3 Position {
      get { return this.transform.position; }
    }
    public Quaternion Rotation {
      get { return this.transform.rotation; }
    }
    public bool IsPinching {
      get {
        if (pinchMode == PinchDetectionMode.PinchDetector) {
          return this.pinchDetector.IsActive;
        }
        else {
          return this.pinchGesture.isActive;
        }
      }
    }
    public bool IsTracked {
      get {
        return this.pinchDetector.HandModel.IsTracked;
      }
    }
    public bool DidStartPinch {
      get {
        if (pinchMode == PinchDetectionMode.PinchDetector) {
          return this.pinchDetector.DidStartPinch;
        }
        else {
          return this.pinchGesture.wasActivated;
        }
      }
    }
    public Chirality Handedness {
      get {
        if (pinchMode == PinchDetectionMode.PinchDetector) {
          return this.pinchDetector.HandModel.Handedness;
        }
        else {
          return this.pinchGesture.whichHand;
        }
      }
    }
    public Color Color {
      get { return _indexTipColor.GetColor(); }
    }

    protected virtual void Start() {
      _handModel = pinchDetector.GetComponentInParent<HandModelBase>();
      _minRadius = pinchDetector.ActivateDistance / 2F;
    }

    protected virtual void Update() {

      var hand = pinchDetector.HandModel.GetLeapHand();
      var indexPos = hand.GetIndex().TipPosition.ToVector3();
      var thumbPos = hand.GetThumb().TipPosition.ToVector3();
      var indexThumbDist = Vector3.Distance(indexPos, thumbPos);

      // Cursor follow type
      {
        var rigidLocalPosition = this.transform.parent.InverseTransformPoint(
                                             hand.GetPredictedPinchPosition());

        switch (cursorFollowType) {
          case CursorFollowType.Rigid:
            this.transform.localPosition = rigidLocalPosition;
            break;
          case CursorFollowType.Dynamic:
            var pinchPos = (indexPos + thumbPos) / 2f;

            var idlePos = this.transform.parent.TransformPoint(rigidLocalPosition);
            var effPinchStrength = 0f;
            if (IsPinching) {
              effPinchStrength = 1f;
            }
            else {
              effPinchStrength = indexThumbDist.Map(0.10f, 0.02f, 0f, 1f);
            }
            var finalPos = Vector3.Lerp(idlePos, pinchPos, effPinchStrength);

            this.transform.position = finalPos;
            break;
        }
      }

      // Calc radius
      float pinchRadiusTarget = indexThumbDist / 2f;
      if (pinchMode == PinchDetectionMode.PinchGesture) {
        if (!pinchGesture.isEligible) {
          pinchRadiusTarget = _maxRadius;
        }
      }
      pinchRadiusTarget = pinchRadiusTarget.Clamped(_minRadius, _maxRadius);
      _radius = Mathf.Lerp(_radius, pinchRadiusTarget, 20f * Time.deltaTime);

      // Calc fade
      float cursorAlpha = _radius.Map(_minRadius, _maxRadius, 1F, 0F);
      if (!_isPaintingPossible) {
        cursorAlpha = 0F;
      }
      else if (!_canBeginPainting && !_isPainting) {
        cursorAlpha = 0F;
      }

      // Set cursor radius
      if (_rectToroidPinchTarget.Radius != _minRadius) {
        _rectToroidPinchTarget.Radius = _minRadius * _thicknessMult;
      }
      _rectToroidPinchState.Radius = _radius * _thicknessMult;

      // Fade cursor
      _drawBeginMarkerCircleColor = Color.Lerp(_drawBeginMarkerCircleColor, new Color(_drawBeginMarkerCircleColor.r, _drawBeginMarkerCircleColor.g, _drawBeginMarkerCircleColor.b, cursorAlpha), 0.3F);
      _rectToroidPinchTargetRenderer.material.color = _drawBeginMarkerCircleColor;
      _cursorColor = Color.Lerp(_cursorColor, new Color(_cursorColor.r, _cursorColor.g, _cursorColor.b, cursorAlpha), 0.3F);
      _rectToroidPinchStateRenderer.material.color = _cursorColor;

      // Fade hands when drawing
      float handAlphaTarget = (1F - cursorAlpha).Map(0F, 1F, 0.4F, 1F);
      _smoothedHandAlpha = Mathf.Lerp(_smoothedHandAlpha, handAlphaTarget, 0.2F);
      if (_smoothedHandAlpha < 0.01F) {
        capsuleHand.doRender = false;
        _indexTipColorRenderer.enabled = false;
      }
      else {
        capsuleHand.doRender = true;
        _indexTipColorRenderer.enabled = true;

        if (_smoothedHandAlpha > 0.99F) {
          capsuleHand.useGhostable = false;
        }
        else {
          capsuleHand.useGhostable = true;
          Color ghostHandColor = _ghostableHandMat.color;
          _ghostableHandMat.color = _ghostableHandMat.color.WithAlpha(_smoothedHandAlpha);
        }
      }
    }

    public void NotifyPossibleToActualize(bool isPossible) {
      _isPaintingPossible = isPossible;
    }

    public void NotifyPossibleToBeginActualizing(bool canBeginPainting) {
      _canBeginPainting = canBeginPainting;
    }

    public void NotifyIsPainting(bool isPainting) {
      _isPainting = isPainting;
    }

    public float GetHandAlpha() {
      return _smoothedHandAlpha;
    }

  }

  public static class RuntimeGizmoDrawerExtensions {
    public static void DrawCircle(this RuntimeGizmoDrawer drawer, Vector3 position, float radius, Vector3 planeDirection) {
      Vector3 perpDirection = Vector3.Cross(planeDirection, Vector3.up).normalized;
      if (perpDirection.magnitude < 0.99F) {
        perpDirection = Vector3.Cross(planeDirection, Vector3.right).normalized;
      }
      int numSegments = 64;
      for (int i = 0; i < numSegments; i++) {
        drawer.DrawLine(position + Quaternion.AngleAxis(360F * (i / (float)numSegments), planeDirection) * perpDirection * radius,
          position + Quaternion.AngleAxis(360F * ((i + 1) / (float)numSegments), planeDirection) * perpDirection * radius);
      }
    }
  }

}
