using UnityEngine;
using System.Collections;
using Leap.Unity;

public class BrushWearableUI : WearableUI {

  [Header("Brush Wearable UI")]
  public EmergeableBehaviour _brushControlsEmergeable;
  public MoveableBehaviour _brushControlsMoveable;
  public EmergeableBehaviour _brushWorkstationEmergeable;

  protected override void Start() {
    base.Start();
  }

  private bool _brushControlsEmerged = false;
  protected override void DoOnFingerPressedMarble() {
    base.DoOnFingerPressedMarble();

    if (!IsGrabbed && !IsWorkstation) {
      if (!_brushControlsEmerged) {
        _brushControlsEmergeable.TryEmerge();
        _brushControlsEmerged = true;
      }
      else {
        _brushControlsEmergeable.TryVanish();
        _brushControlsEmerged = false;
      }
    }
  }

  protected override void DoOnAnchorChiralityChanged(Chirality newChirality) {
    base.DoOnAnchorChiralityChanged(newChirality);

    if (newChirality != DisplayingChirality) {
      _brushControlsMoveable._A.position = MirrorUtil.GetMirroredPosition(_brushControlsMoveable._A.position, this.transform);
      _brushControlsMoveable._A.rotation = MirrorUtil.GetMirroredRotation(_brushControlsMoveable._A.rotation, this.transform);
      if (!IsWorkstation) {
        _brushControlsMoveable.MoveToA();
      }

      DisplayingChirality = newChirality;
    }
  }

  protected override void DoOnGrabbed() {
    base.DoOnGrabbed();

    _brushControlsEmergeable.TryVanish();
    _brushWorkstationEmergeable.TryVanish();
  }

  protected override void DoOnMovementToWorkstationBegan() {
    base.DoOnMovementToWorkstationBegan();
  }

  protected override void DoOnMovementToWorkstationFinished() {
    base.DoOnMovementToWorkstationFinished();

    _brushControlsMoveable.MoveToB();

    if (!IsGrabbed) {
      _brushControlsEmergeable.TryEmerge();
      _brushWorkstationEmergeable.TryEmerge();
    }
  }

  protected override float GetOptimalWorkstationVerticalOffset() {
    return -0.1F;
  }

  protected override void DoOnReturnedToAnchor() {
    base.DoOnReturnedToAnchor();

    _brushControlsMoveable.MoveToA();
  }

}
