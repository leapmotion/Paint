using UnityEngine;
using System.Collections;
using Leap.Unity;

public class WearableAnchor : MonoBehaviour, IWearable {

  [Header("Mirroring")]
  [Tooltip("The equivalent anchor to this one, with opposite chirality.")]
  public WearableAnchor _mirroredEquivalent;
  [Tooltip("This anchor's chirality.")]
  public Chirality _anchorChirality;

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
        Debug.LogWarning("Appear tween is invalid. (Why..?)");
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
    if (_isHandTracked && _isPalmFacingCamera && !_isHandPinching) {
      if (!_mirroredEquivalent.IsDisplaying) {
        ScheduleAppear();
      }
    }
    else {
      ScheduleVanish();
    }
  }

  #region Wearable Implementation

  private bool _isHandTracked = false;
  private bool _isPalmFacingCamera = false;
  private bool _isHandPinching = false;

  public void NotifyHandTracked(bool isHandTracked, Chirality whichHand) {
    if (_anchorChirality == whichHand) {
      _isHandTracked = isHandTracked;
      RefreshVisibility();
    }
  }

  public void NotifyPalmFacingCamera(bool isPalmFacingCamera, Chirality whichHand) {
    if (_anchorChirality == whichHand) {
      _isPalmFacingCamera = isPalmFacingCamera;
      RefreshVisibility();
    }
  }

  public void NotifyPinchChanged(bool isPinching, Chirality whichHand) {
    if (_anchorChirality == whichHand) {
      _isHandPinching = isPinching;
      RefreshVisibility();
    }
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

  #region Mirroring

  private Chirality _lastDisplayedChirality = Chirality.Left;

  public WearableAnchor GetChiralAnchor(Chirality whichHand) {
    if (_anchorChirality == whichHand) {
      return this;
    }
    else {
      return _mirroredEquivalent;
    }
  }

  public WearableAnchor GetRightAnchor() {
    if (_anchorChirality == Chirality.Right) {
      return this;
    }
    else {
      return _mirroredEquivalent;
    }
  }

  public WearableAnchor GetLeftAnchor() {
    if (_anchorChirality == Chirality.Left) {
      return this;
    }
    else {
      return _mirroredEquivalent;
    }
  }

  public WearableAnchor GetLastDisplayedChiralAnchor() {
    if (_lastDisplayedChirality == Chirality.Left) {
      if (_anchorChirality == Chirality.Left) {
        return this;
      }
      else {
        return _mirroredEquivalent;
      }
    }
    else {
      if (_anchorChirality == Chirality.Right) {
        return this;
      }
      else {
        return _mirroredEquivalent;
      }
    }
  }

  public void NotifyChiralEquivalentAnchorDisplayed() {
    if (_anchorChirality == Chirality.Left) {
      _lastDisplayedChirality = Chirality.Right;
    }
    else {
      _lastDisplayedChirality = Chirality.Left;
    }
  }

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
    _mirroredEquivalent.RefreshVisibility();
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
    if (_anchorChirality == Chirality.Left) {
      // Prevent both left and right anchors from appearing at once. If both are scheduled, Left gets precedence.
      if (_mirroredEquivalent.IsScheduledToAppear()) {
        _mirroredEquivalent.CancelScheduledAppearance();
      }
      else if (_mirroredEquivalent.IsPlayingAppearance()) {
        _mirroredEquivalent.StopAppearTween();
      }
    }
    else {
      if (_mirroredEquivalent.IsScheduledToAppear()) {
        this.CancelScheduledAppearance();
      }
      else if (_mirroredEquivalent.IsPlayingAppearance()) {
        this.CancelScheduledAppearance();
      }
    }

    if (_appearScheduled) {
      _lastDisplayedChirality = this._anchorChirality;
      _mirroredEquivalent.NotifyChiralEquivalentAnchorDisplayed();

      _appearTween.Play(TweenDirection.FORWARD);
    }
  }

  public void Vanish() {
    _appearTween.Play(TweenDirection.BACKWARD);
  }

  public bool IsScheduledToAppear() {
    return _appearScheduled;
  }

  public bool IsPlayingAppearance() {
    return _appearTween.IsRunning && _appearTween.Direction == TweenDirection.FORWARD;
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
