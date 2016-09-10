using UnityEngine;
using System.Collections;
using Leap.Unity;

public class ColorWearableUI : WearableUI {

  [Header("Color Wearable UI")]
  public EmergeableBehaviour _primaryPaletteEmergeable;
  public MoveableBehaviour _primaryPaletteMoveable;
  public ColorPalette _primaryPalette;

  public EmergeableBehaviour[] _workstationEmergeables;

  protected override void DoOnFingerPressedMarble() {
    base.DoOnFingerPressedMarble();

    if (!IsGrabbed && !IsWorkstation) {
      if (!_primaryPaletteEmergeable.IsEmergedOrEmerging) {
        _primaryPaletteEmergeable.TryEmerge();
      }
      else {
        _primaryPaletteEmergeable.TryVanish();
      }
    }
  }

  protected override void DoOnAnchorChiralityChanged(Chirality newChirality) {
    base.DoOnAnchorChiralityChanged(newChirality);

    if (newChirality != DisplayingChirality) {
      _primaryPaletteMoveable._A.position = MirrorUtil.GetMirroredPosition(_primaryPaletteMoveable._A.position, this.transform);
      _primaryPaletteMoveable._A.rotation = MirrorUtil.GetMirroredRotation(_primaryPaletteMoveable._A.rotation, this.transform);
      if (!IsWorkstation) {
        _primaryPaletteMoveable.MoveToA();
      }

      DisplayingChirality = newChirality;
    }
  }

  protected override void DoOnGrabbed() {
    base.DoOnGrabbed();

    _primaryPaletteEmergeable.TryVanish();

    _primaryPalette.SetSwatchModeDoNothing();

    for (int i = 0; i < _workstationEmergeables.Length; i++) {
      _workstationEmergeables[i].TryVanish();
    }
  }

  protected override void DoOnMovementToWorkstationBegan() {
    base.DoOnMovementToWorkstationBegan();

    _primaryPaletteMoveable.MoveToB();
  }

  protected override void DoOnMovementToWorkstationFinished() {
    base.DoOnMovementToWorkstationFinished();

    if (!IsGrabbed) {
      _primaryPaletteEmergeable.TryEmerge();
      _primaryPalette.SetSwatchModeReceiveColor();
      for (int i = 0; i < _workstationEmergeables.Length; i++) {
        _workstationEmergeables[i].TryEmerge();
      }
    }
  }

  protected override void DoOnReturnedToAnchor() {
    base.DoOnReturnedToAnchor();

    _primaryPaletteMoveable.MoveToA();
    _primaryPalette.SetSwatchModeAssignColor();

    //if (_primaryColorPalette != null) {
    //  EmergeableBehaviour emergeable = _primaryColorPalette.GetComponent<EmergeableBehaviour>();
    //  _primaryColorPalette.MoveToA();
    //  _primaryColorPalette.GetComponent<ColorPalette>().SetSwatchModeAssignColor();
    //}
  }

}
