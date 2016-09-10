using UnityEngine;
using System.Collections;
using Leap.Unity;

public class BrushWearableUI : WearableUI {

  [Header("Brush Wearable UI")]
  public EmergeableBehaviour[] _emergeables;

  private bool _brushControlsEmerged = false;

  protected override void DoOnFingerPressedMarble() {
    base.DoOnFingerPressedMarble();

    if (!IsGrabbed && !IsWorkstation) {
      if (!_brushControlsEmerged) {
        for (int i = 0; i < _emergeables.Length; i++) {
          _emergeables[i].TryEmerge();
        }
        _brushControlsEmerged = true;
      }
      else {
        for (int i = 0; i < _emergeables.Length; i++) {
          _emergeables[i].TryVanish();
        }
        _brushControlsEmerged = false;
      }
    }

    if (!IsGrabbed) {
    }
  }

  protected override void DoOnGrabbed() {
    base.DoOnGrabbed();

    for (int i = 0; i < _emergeables.Length; i++) {
      _emergeables[i].TryVanish();
    }
  }

  protected override void DoOnMovementToWorkstationBegan() {
    base.DoOnMovementToWorkstationBegan();

  }

  protected override void DoOnMovementToWorkstationFinished() {
    base.DoOnMovementToWorkstationFinished();

    if (!IsGrabbed) {
      for (int i = 0; i < _emergeables.Length; i++) {
        _emergeables[i].TryEmerge();
      }
    }
  }

  protected override void DoOnReturnedToAnchor() {
    base.DoOnReturnedToAnchor();


  }

}
