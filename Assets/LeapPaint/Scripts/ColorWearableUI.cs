using UnityEngine;
using System.Collections;
using Leap.Unity;

public class ColorWearableUI : WearableUI {

  [Header("Color Wearable UI")]
  public EmergeableBehaviour[] _emergeables;
  public MoveableBehaviour _primaryColorPalette;

  private Chirality _chirality = Chirality.Left;

  protected override void DoOnFingerPressedMarble() {
    base.DoOnFingerPressedMarble();

    if (!IsGrabbed && !IsWorkstation) {
      _primaryColorPalette.MoveToAorC();
      EmergeableBehaviour emergeable = _primaryColorPalette.GetComponent<EmergeableBehaviour>();
      if (emergeable.IsEmergedOrEmerging) {
        emergeable.TryVanish();
      }
      else {
        emergeable.TryEmerge();
      }
    }
  }

  protected override void DoOnAnchorChiralityChanged(Chirality whichHand) {
    base.DoOnAnchorChiralityChanged(whichHand);

    if (whichHand != _chirality) {
      // Opposite hand; switch to other root transform
      _primaryColorPalette.ToggleC();
      _primaryColorPalette.MoveToAorC();
      _chirality = whichHand;
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

    _primaryColorPalette.MoveToB();
    _primaryColorPalette.GetComponent<ColorPalette>().SetSwatchModeReceiveColor();
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

    EmergeableBehaviour emergeable = _primaryColorPalette.GetComponent<EmergeableBehaviour>();
    _primaryColorPalette.MoveToA();
    _primaryColorPalette.GetComponent<ColorPalette>().SetSwatchModeAssignColor();
  }

}
