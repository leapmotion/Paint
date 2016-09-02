using UnityEngine;
using System.Collections;
using Leap.Unity;

public class WearableAnchor : MonoBehaviour {

  #region Unity Callbacks

  protected void Start() {
    InitAppearVanish();
  }

  #endregion

  #region Mirroring

  [Header("Mirroring")]
  [Tooltip("The equivalent anchor to this one, with opposite chirality.")]
  public WearableAnchor _mirroredEquivalent;
  [Tooltip("This anchor's chirality.")]
  public Chirality _anchorChirality;

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

  #endregion

  #region Rendering

  [Header("Rendering")]
  public MeshRenderer _anchorRingRenderer;
  [Tooltip("The material to use when this object is fully opaque. Prevents tearing when multiple transparent materials overlap.")]
  public Material _opaqueMaterial;
  [Tooltip("The material to use when this object is fading in or out.")]
  public Material _fadeMaterial;

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

  #region Appear / Vanish

  private TweenHandle _appearTween;

  private void InitAppearVanish() {
    _appearTween = ConstructAppearTween();
    _appearTween.Progress = 0F;
  }

  private TweenHandle ConstructAppearTween() {
    return Tween.Value(new Color(1F, 1F, 1F, 0F), Color.white, SetColor)
      .OverTime(0.25F)
      .Smooth(TweenType.SMOOTH);
  }

  public void Appear() {
    _appearTween.Play();
  }

  public void Vanish() {
    _appearTween.Play(TweenDirection.BACKWARD);
  }

  #endregion

}
