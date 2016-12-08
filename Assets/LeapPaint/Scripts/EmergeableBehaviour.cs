using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using Leap.Unity.Attributes;
using Leap.Unity.Animation;

public class EmergeableBehaviour : MonoBehaviour {

  public bool _beginEmerged = false;

  [Header("Enable / Disable Components")]
  [Header("(Finished Emerging / Begun Vanishing)")]
  public MonoBehaviour[] _enableDisableComponents;

  [Header("(Automatic) To Enable/Disable on Appear/Vanish")]
  [Disable]
  public Renderer[] _renderers; // includes MeshRenderers and SpriteRenderers
  [Disable]
  public Graphic[] _graphics;   // includes uGUI Images and the like

  [Header("Sounds")]
  public SoundEffect _emergeEffect;
  public SoundEffect _vanishEffect;

  public bool _seperateWorkstationEffect = false;
  [DisableIf("_seperateWorkstationEffect", isEqualTo: false)]
  public SoundEffect _workstationEmergeEffect;
  [DisableIf("_seperateWorkstationEffect", isEqualTo: false)]
  public SoundEffect _workstationVanishEffect;

  public Action OnBegunEmerging = () => { };
  public Action OnFinishedEmerging = () => { };
  public Action OnBegunVanishing = () => { };
  public Action OnFinishedVanishing = () => { };

  private Tween _vanishTween;

  public bool IsEmergedOrEmerging {
    get {
      if (!_vanishTween.isValid) {
        _vanishTween = CreateVanishTween();
      }
      return (_vanishTween.isRunning && _vanishTween.direction == Direction.Backward)
           || _vanishTween.progress == 0F;
    }
  }

  protected virtual void Start() {
    // TODO: This won't work if there are nested EmergeableBehaviours!
    // Traverse the child hierarchy and don't register any children of objects that are themselves EmergeableBehaviours.
    _renderers = GetComponentsInChildren<Renderer>();
    _graphics = GetComponentsInChildren<Graphic>();

    _vanishTween = CreateVanishTween();
    if (!_beginEmerged) {
      _vanishTween.progress = 1F;
      DisableComponents();
    }
  }

  public void TryEmerge(bool isInWorkstation) {
    if (_seperateWorkstationEffect && isInWorkstation) {
      _workstationEmergeEffect.PlayAtPosition(transform);
    } else {
      _emergeEffect.PlayAtPosition(transform);
    }

    _vanishTween.Play(Direction.Backward);
  }

  public void TryVanish(bool isInWorkstation) {
    if (_seperateWorkstationEffect && isInWorkstation) {
      _workstationVanishEffect.PlayAtPosition(transform);
    } else {
      _vanishEffect.PlayAtPosition(transform);
    }

    _vanishTween.Play(Direction.Forward);
  }

  private Tween CreateVanishTween() {
    return Tween.Persistent().Target(this.transform)
      .ToLocalScale(this.transform.localScale / 100F)
      //.Target(this.transform)
      //.ToPosition((_vanishingPoint == null? this.transform : _vanishingPoint))
      .OverTime(0.2F)
      .Smooth(SmoothType.Smooth)
      .OnLeaveEnd(DoOnBegunEmerging)
      .OnReachStart(DoOnFinishedEmerging)
      .OnLeaveStart(DoOnBegunVanishing)
      .OnReachEnd(DoOnFinishedVanishing);
  }

  protected virtual void DoOnBegunEmerging() {
    for (int i = 0; i < _renderers.Length; i++) {
      _renderers[i].enabled = true;
    }
    for (int i = 0; i < _graphics.Length; i++) {
      _graphics[i].enabled = true;
    }
    OnBegunEmerging();
  }

  protected virtual void DoOnFinishedEmerging() {
    EnableComponents();
    OnFinishedEmerging();
  }

  protected virtual void DoOnBegunVanishing() {
    DisableComponents();
    OnBegunVanishing();
  }

  protected virtual void DoOnFinishedVanishing() {
    for (int i = 0; i < _renderers.Length; i++) {
      _renderers[i].enabled = false;
    }
    for (int i = 0; i < _graphics.Length; i++) {
      _graphics[i].enabled = false;
    }
    OnFinishedVanishing();
  }

  private void EnableComponents() {
    for (int i = 0; i < _enableDisableComponents.Length; i++) {
      _enableDisableComponents[i].enabled = true;
    }
  }

  private void DisableComponents() {
    for (int i = 0; i < _enableDisableComponents.Length; i++) {
      _enableDisableComponents[i].enabled = false;
    }
  }

}
