using UnityEngine;
using System.Collections;

public class ColorWearableUI : WearableUI {

  [Header("Color Wearable UI")]
  public EmergeableBehaviour[] _emergeables;
  public MoveableBehaviour _primaryColorPalette;

  protected override void DoOnMovementToWorkstationFinished() {
    base.DoOnMovementToWorkstationFinished();

    if (!IsGrabbed) {
      for (int i = 0; i < _emergeables.Length; i++) {
        _emergeables[i].TryEmerge();
      }
    }
  }

  protected override void DoOnGrabbed() {
    base.DoOnGrabbed();

    for (int i = 0; i < _emergeables.Length; i++) {
      _emergeables[i].TryVanish();
    }
  }

  protected override void DoOnFingerPressedMarble() {
    base.DoOnFingerPressedMarble();

    if (!IsGrabbed && !IsWorkstation) {
      _primaryColorPalette.MoveToA();
      EmergeableBehaviour emergeable = _primaryColorPalette.GetComponent<EmergeableBehaviour>();
      if (emergeable.IsEmergedOrEmerging) {
        emergeable.TryVanish();
      }
      else {
        emergeable.TryEmerge();
      }
    }
  }

  protected override void DoOnReturnedToAnchor() {
    base.DoOnReturnedToAnchor();

    EmergeableBehaviour emergeable = _primaryColorPalette.GetComponent<EmergeableBehaviour>();
    _primaryColorPalette.MoveToA();
  }

  protected override void DoOnMovementToWorkstationBegan() {
    base.DoOnMovementToWorkstationBegan();

    _primaryColorPalette.MoveToB();
  }

}
