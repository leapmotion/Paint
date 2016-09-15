using UnityEngine;
using System.Collections;
using Leap.Unity;
using System;

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

  public bool IsDisplaying {
    get {
      if (_appearTween.IsValid) {
        return _appearTween.Progress != 0F;
      }
      else {
        InitAppearVanish();
        return false;
      }
    }
  }

  protected void Start() {
    InitAppearVanish();
  }

  protected void FixedUpdate() {
    FixedAppearVanishUpdate();
  }

  private void RefreshVisibility() {
    bool leftHandCanDisplay = _isLeftHandTracked && _isLeftPalmFacingCamera && !_isLeftHandPinching;
    bool rightHandCanDisplay = _isRightHandTracked && _isRightPalmFacingCamera && !_isRightHandPinching;

    if (leftHandCanDisplay && !IsDisplaying) {
      SetChirality(Chirality.Left);
      ScheduleAppear();
    }
    else if (rightHandCanDisplay && !IsDisplaying) {
      SetChirality(Chirality.Right);
      ScheduleAppear();
    }
    else if (!leftHandCanDisplay && _chirality == Chirality.Left && IsDisplaying) {
      ScheduleVanish();
    }
    else if (!rightHandCanDisplay && _chirality == Chirality.Right && IsDisplaying) {
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

  private TweenHandle _appearTween;

  private bool _appearScheduled = false;
  private bool _vanishScheduled = false;

  private void InitAppearVanish() {
    _appearTween = ConstructAppearTween();
    _appearTween.Progress = 0.001F;
    Vanish();
  }

  private TweenHandle ConstructAppearTween() {
    return Tween.Value(new Color(1F, 1F, 1F, 0F), Color.white, SetColor)
      .OverTime(0.2F)
      .Smooth(TweenType.SMOOTH)
      .OnReachStart(DoOnFinishedVanishing)
      .Keep();
  }

  private void DoOnFinishedVanishing() {
    RefreshVisibility();
    OnAnchorFinishDisappearing();
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
    _appearTween.Play(TweenDirection.FORWARD);
    OnAnchorBeginAppearing();
  }

  public void Vanish() {
    _appearTween.Play(TweenDirection.BACKWARD);
    OnAnchorBeginDisappearing();
  }

  public bool IsScheduledToAppear() {
    return _appearScheduled;
  }

  public bool IsPlayingAppearance() {
    return _appearTween.IsValid && _appearTween.IsRunning && _appearTween.Direction == TweenDirection.FORWARD;
  }

  public bool IsScheduledToAppearOrAppearing() {
    return IsScheduledToAppear() || IsPlayingAppearance();
  }

  public void StopAppearTween() {
    _appearTween.Stop();
    _appearTween.Progress = 0F;
  }

  public void CancelScheduledAppearance() {
    _appearScheduled = false;
  }

  #endregion

  #region Rendering

  public void SetColor(Color color) {
    if (color.a < 0.99F) {
      _anchorRingRenderer.material = _fadeMaterial;
    }
    else {
      _anchorRingRenderer.material = _opaqueMaterial;
    }
    _anchorRingRenderer.material.color = color;
  }

  #endregion

}
