using UnityEngine;
using System.Collections;
using Leap.Unity;
using System;
using Leap.Unity.Animation;

public class WearableAnchor : HandedPalmAnchor, IWearable {

  public Action OnAnchorBeginAppearing = () => { };
  public Action OnAnchorBeginDisappearing = () => { };
  public Action OnAnchorFinishDisappearing = () => { };

  [Header("Rendering")]
  public MeshRenderer _anchorRingRenderer;
  [Tooltip("The material to use when this object is fully opaque. Prevents tearing when multiple transparent materials overlap.")]
  public Material _opaqueMaterial;
  [Tooltip("The material to use when this object is fading in or out.")]
  public Material _fadeMaterial;

  public SoundEffect showEffect;
  private Material _opaqueInstance;
  private Material _fadeInstance;

  public void ManualInitialize() {
    _opaqueInstance = new Material(_opaqueMaterial);
    _fadeInstance = new Material(_fadeMaterial);

    InitAppearVanish();
  }

  public void ManualFixedUpdate() {
    FixedAppearVanishUpdate();
  }

  private void RefreshVisibility() {
    bool leftHandCanDisplay = _isLeftHandTracked && _isLeftPalmFacingCamera && !_isLeftHandPinching;
    bool rightHandCanDisplay = _isRightHandTracked && _isRightPalmFacingCamera && !_isRightHandPinching;

    if (leftHandCanDisplay && !IsDisplaying && !_appearScheduled) {
      SetChirality(Chirality.Left);
      ScheduleAppear();
    }
    else if (rightHandCanDisplay && !IsDisplaying && !_appearScheduled) {
      SetChirality(Chirality.Right);
      ScheduleAppear();
    }
    else if (!leftHandCanDisplay && _chirality == Chirality.Left && IsDisplaying && !_vanishScheduled) {
      ScheduleVanish();
    }
    else if (!rightHandCanDisplay && _chirality == Chirality.Right && IsDisplaying && !_vanishScheduled) {
      ScheduleVanish();
    }
  }

  #region Wearable Implementation

  private bool _isLeftHandTracked = false;
  private bool _isLeftPalmFacingCamera = false;
  private bool _isLeftHandPinching = false;
  private bool _isRightHandTracked = false;
  private bool _isRightPalmFacingCamera = false;
  private bool _isRightHandPinching = false;

  public void NotifyHandTracked(bool isHandTracked, Chirality whichHand) {
    if (whichHand == Chirality.Left) {
      _isLeftHandTracked = isHandTracked;
    }
    else {
      _isRightHandTracked = isHandTracked;
    }
    RefreshVisibility();
  }

  public void NotifyPalmFacingCamera(bool isPalmFacingCamera, Chirality whichHand) {
    if (whichHand == Chirality.Left) {
      _isLeftPalmFacingCamera = isPalmFacingCamera;
    }
    else {
      _isRightPalmFacingCamera = isPalmFacingCamera;
    }
    RefreshVisibility();
  }

  public void NotifyPinchChanged(bool isPinching, Chirality whichHand) {
    if (whichHand == Chirality.Left) {
      _isLeftHandPinching = isPinching;
    }
    else {
      _isRightHandPinching = isPinching;
    }
    RefreshVisibility();
  }

  public Vector3 GetPosition() {
    return this.transform.position;
  }

  public bool CanBeGrabbed() {
    return false;
  }

  public bool BeGrabbedBy(Transform grabber) {
    return false;
  }

  public void ReleaseFromGrab(Transform grabber) { }

  #endregion

  #region Appear / Vanish

  private Tween _appearTween;

  private bool _appearScheduled = false;
  private bool _vanishScheduled = false;

  private bool _isDisplaying = false;
  public bool IsDisplaying {
    get {
      return _isDisplaying || _appearScheduled;
    }
  }

  private void InitAppearVanish() {
    _appearTween = ConstructAppearTween();
    _appearTween.progress = 0.001F;
    Vanish();
    _isDisplaying = false;
  }

  private Tween ConstructAppearTween() {
    return Tween.Persistent().Value(new Color(1F, 1F, 1F, 0F), Color.white, SetColor)
      .OverTime(0.2F)
      .Smooth(SmoothType.Smooth)
      .OnReachStart(DoOnFinishedVanishing)
      .OnLeaveStart(DoOnBeganAppearing);
  }

  private void DoOnBeganAppearing() {
    _isDisplaying = true;
  }

  private void DoOnFinishedVanishing() {
    _isDisplaying = false;
    OnAnchorFinishDisappearing();
    RefreshVisibility();
  }

  private void ScheduleAppear() {
    _appearScheduled = true;
  }
  private void ScheduleVanish() {
    _vanishScheduled = true;
  }

  private void FixedAppearVanishUpdate() {
    if (_appearScheduled) {
      Appear();
      _appearScheduled = false;
      if (_vanishScheduled) _vanishScheduled = false;
    }
    else if (_vanishScheduled) {
      Vanish();
      _vanishScheduled = false;
    }
  }

  public void Appear() {
    _appearTween.Play(Direction.Forward);
    OnAnchorBeginAppearing();
    showEffect.PlayOnTransform(transform);
  }

  public void Vanish() {
    _appearTween.Play(Direction.Backward);
    OnAnchorBeginDisappearing();
  }

  #endregion

  #region Rendering

  public void SetColor(Color color) {
    if (color.a < 0.99F) {
      if (color.a < 0.001F) {
        _anchorRingRenderer.enabled = false;
      }
      else {
        _anchorRingRenderer.enabled = true;
        _fadeInstance.color = color;
        _anchorRingRenderer.material = _fadeInstance;
      }
    }
    else {
      _anchorRingRenderer.enabled = true;
      _opaqueInstance.color = color;
      _anchorRingRenderer.material = _opaqueInstance;
    }
  }

  #endregion

}
