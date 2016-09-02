using UnityEngine;
using System.Collections;
using System;

public class EmergeableBehaviour : MonoBehaviour {

  public bool _beginEmerged = false;

  [Header("Optional")]
  [Tooltip("If None, will use own Transform.")]
  public Transform _vanishingPoint;

  public Action OnBegunEmerging     = new Action(() => { });
  public Action OnFinishedEmerging  = new Action(() => { });
  public Action OnBegunVanishing    = new Action(() => { });
  public Action OnFinishedVanishing = new Action(() => { });

  private bool _isEmergedOrEmerging = true;
  //private bool _isFullyEmerged = true;
  //private bool _isVanishedOrVanishing = false;
  //private bool _isFullyVanished = false;

  private TweenHandle _vanishTween;

  protected virtual void Start() {
    _vanishTween = CreateVanishTween();

    if (!_beginEmerged) {
      _vanishTween.Progress = 1F;
    }
  }

  protected virtual void Update() {
    if (Input.GetKeyDown(KeyCode.Space)) {
      if (_isEmergedOrEmerging) {
        TryVanish();
      }      
      else {
        TryEmerge();
      }
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

  private void DoOnBegunEmerging() {
    _isEmergedOrEmerging = true;
    //_isFullyEmerged = false;
    //_isVanishedOrVanishing = false;
    //_isFullyVanished = false;
    OnBegunEmerging();
  }
  private void DoOnFinishedEmerging() {
    _isEmergedOrEmerging = true;
    //_isFullyEmerged = true;
    //_isVanishedOrVanishing = false;
    //_isFullyVanished = false;
    OnFinishedEmerging();
  }

  private void DoOnBegunVanishing() {
    _isEmergedOrEmerging = false;
    //_isFullyEmerged = false;
    //_isVanishedOrVanishing = true;
    //_isFullyVanished = false;
    OnBegunVanishing();
  }
  private void DoOnFinishedVanishing() {
    _isEmergedOrEmerging = false;
    //_isFullyEmerged = false;
    //_isVanishedOrVanishing = true;
    //_isFullyVanished = true;
    OnFinishedVanishing();
  }

}
