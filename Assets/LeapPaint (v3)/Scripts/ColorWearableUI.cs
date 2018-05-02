using UnityEngine;
using System.Collections;
using Leap.Unity;

namespace Leap.Unity.LeapPaint_v3 {


  public class ColorWearableUI : WearableUI {

    [Header("Color Wearable UI")]
    public EmergeableBehaviour _primaryPaletteEmergeable;
    public MoveableBehaviour _primaryPaletteMoveable;
    public ColorPalette _primaryPalette;

    public ColorPalette _workstationPalette;
    public EmergeableBehaviour _workstationPaletteEmergeable;

    public EmergeableBehaviour[] _workstationEmergeables;

    private bool _emergeableCallbacksInitialized = false;
    protected override void Start() {
      base.Start();

      if (!_emergeableCallbacksInitialized) {
        _primaryPalette.SetSwatchMode(ColorSwatch.SwatchMode.DoNothing);
        _primaryPaletteEmergeable.OnFinishedEmerging += DoOnPaletteFinishedEmerging;
        _primaryPaletteEmergeable.OnBegunVanishing += DoOnPaletteBeganVanishing;

        _workstationPalette.SetSwatchMode(ColorSwatch.SwatchMode.DoNothing);
        _workstationPaletteEmergeable.OnFinishedEmerging += DoOnWorkstationPaletteFinishedEmerging;
        _workstationPaletteEmergeable.OnBegunVanishing += DoOnWorkstationPaletteBeganVanishing;

        _emergeableCallbacksInitialized = true;
      }
    }

    private void DoOnPaletteFinishedEmerging() {
      if (IsWorkstation) {
        _primaryPalette.SetSwatchMode(ColorSwatch.SwatchMode.ReceiveColor);
      }
      else {
        _primaryPalette.SetSwatchMode(ColorSwatch.SwatchMode.AssignColor);
      }
    }

    private void DoOnPaletteBeganVanishing() {
      _primaryPalette.SetSwatchMode(ColorSwatch.SwatchMode.DoNothing);
    }

    private void DoOnWorkstationPaletteFinishedEmerging() {
      _workstationPalette.SetSwatchMode(ColorSwatch.SwatchMode.AssignColor);
    }

    private void DoOnWorkstationPaletteBeganVanishing() {
      _workstationPalette.SetSwatchMode(ColorSwatch.SwatchMode.DoNothing);
    }

    #region WearableUI Implementations

    protected override float GetOptimalWorkstationVerticalOffset() {
      return -0.23F;
    }

    protected override void DoOnMarbleActivated() {
      base.DoOnMarbleActivated();

      if (!isGrasped && !IsWorkstation) {
        if (!_primaryPaletteEmergeable.IsEmergedOrEmerging) {
          _primaryPaletteEmergeable.TryEmerge(IsWorkstation);
        }
        else {
          _primaryPaletteEmergeable.TryVanish(IsWorkstation);
        }
      }
    }

    protected override void DoOnAnchorChiralityChanged(Chirality newChirality) {
      base.DoOnAnchorChiralityChanged(newChirality);

      if (newChirality != DisplayingChirality) {
        _primaryPaletteMoveable.A.localPosition = new Vector3(-_primaryPaletteMoveable.A.localPosition.x, _primaryPaletteMoveable.A.localPosition.y, _primaryPaletteMoveable.A.localPosition.z);
        _primaryPaletteMoveable.A.rotation = MirrorUtil.GetMirroredRotation(_primaryPaletteMoveable.A.rotation, this.transform);
        if (!IsWorkstation) {
          _primaryPaletteMoveable.MoveToA();
        }

        DisplayingChirality = newChirality;
      }
    }

    protected override void DoOnGrabbed() {
      base.DoOnGrabbed();

      _primaryPaletteEmergeable.TryVanish(IsWorkstation);
      for (int i = 0; i < _workstationEmergeables.Length; i++) {
        _workstationEmergeables[i].TryVanish(IsWorkstation);
      }
    }

    protected override void DoOnMovementToWorkstationBegan() {
      base.DoOnMovementToWorkstationBegan();

      _primaryPaletteMoveable.MoveToB();
    }

    protected override void DoOnMovementToWorkstationFinished() {
      base.DoOnMovementToWorkstationFinished();

      if (!isGrasped) {
        _primaryPaletteEmergeable.TryEmerge(IsWorkstation);
        for (int i = 0; i < _workstationEmergeables.Length; i++) {
          _workstationEmergeables[i].TryEmerge(IsWorkstation);
        }
      }
    }

    protected override void DoOnReturnedToAnchor() {
      base.DoOnReturnedToAnchor();

      _primaryPaletteEmergeable.TryVanishNow(IsWorkstation);
      for (int i = 0; i < _workstationEmergeables.Length; i++) {
        _workstationEmergeables[i].TryVanishNow(IsWorkstation);
      }

      _primaryPaletteMoveable.MoveToA();
    }

    #endregion

  }


}
