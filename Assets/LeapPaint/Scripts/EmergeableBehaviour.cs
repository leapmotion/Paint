using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class EmergeableBehaviour : MonoBehaviour {

  public bool _beginEmerged = false;

  [Header("(Automatic) To Enable/Disable on Appear/Vanish")]
  public Renderer[] _renderers; // includes MeshRenderers and SpriteRenderers
  public Graphic[] _graphics;   // includes uGUI Images and the like

  [Header("Optional")]
  [Tooltip("If None, will use own Transform. TODO: Currently unused.")]
  public Transform _vanishingPoint;

  private TweenHandle _vanishTween;

  public bool IsEmergedOrEmerging {
    get {
      return (_vanishTween.IsRunning && _vanishTween.Direction == TweenDirection.BACKWARD)
           || _vanishTween.Progress == 0F;
    }
  }

  protected virtual void Start() {
    // TODO: This won't work if there are nested EmergeableBehaviours!
    // Traverse the child hierarchy and don't register any children of objects that are themselves EmergeableBehaviours.
    _renderers = GetComponentsInChildren<Renderer>();
    _graphics = GetComponentsInChildren<Graphic>();

    _vanishTween = CreateVanishTween();
    if (!_beginEmerged) {
      _vanishTween.Progress = 1F;
    }
  }

  public void TryEmerge() {
    _vanishTween.Play(TweenDirection.BACKWARD);
  }

  public void TryVanish() {
    _vanishTween.Play(TweenDirection.FORWARD);
  }

  private TweenHandle CreateVanishTween() {
    return Tween.Target(this.transform)
      .ToLocalScale(this.transform.localScale / 100F)
      //.Target(this.transform)
      //.ToPosition((_vanishingPoint == null? this.transform : _vanishingPoint))
      .OverTime(0.2F)
      .Smooth(TweenType.SMOOTH)
      .OnLeaveEnd(DoOnBegunEmerging)
      .OnReachStart(DoOnFinishedEmerging)
      .OnLeaveStart(DoOnBegunVanishing)
      .OnReachEnd(DoOnFinishedVanishing)
      .Keep();
  }

  protected virtual void DoOnBegunEmerging() {
    for (int i = 0; i < _renderers.Length; i++) {
      _renderers[i].enabled = true;
    }
    for (int i = 0; i < _graphics.Length; i++) {
      _graphics[i].enabled = true;
    }
  }
  protected virtual void DoOnFinishedEmerging() {

  }

  protected virtual void DoOnBegunVanishing() {

  }
  protected virtual void DoOnFinishedVanishing() {
    for (int i = 0; i < _renderers.Length; i++) {
      _renderers[i].enabled = false;
    }
    for (int i = 0; i < _graphics.Length; i++) {
      _graphics[i].enabled = false;
    }
  }

}
