using UnityEngine;
using System.Collections;
using Leap.Unity;

namespace Leap.Unity.LeapPaint_v3 {

  public class BrushWearableUI : WearableUI {

    [Header("Brush Wearable UI")]
    public EmergeableBehaviour _brushControlsEmergeable;
    public MoveableBehaviour _brushControlsMoveable;
    public EmergeableBehaviour _brushWorkstationEmergeable;

    protected override void Start() {
      base.Start();
    }

    public override float GetWorkstationDangerZoneRadius() {
      return 0.1F;
    }

    public override float GetAnchoredDangerZoneRadius() {
      return 0.08F;
    }

    private bool _brushControlsEmerged = false;
    protected override void DoOnMarbleActivated() {
      base.DoOnMarbleActivated();

      if (!isGrasped && !IsWorkstation) {
        if (!_brushControlsEmerged) {
          _brushControlsEmergeable.TryEmerge(IsWorkstation);
          _brushControlsEmerged = true;
        }
        else {
          _brushControlsEmergeable.TryVanish(IsWorkstation);
          _brushControlsEmerged = false;
        }
      }
    }

    protected override void DoOnAnchorChiralityChanged(Chirality newChirality) {
      base.DoOnAnchorChiralityChanged(newChirality);

      if (newChirality != DisplayingChirality) {
        _brushControlsMoveable._A.localPosition = new Vector3(-_brushControlsMoveable._A.localPosition.x, _brushControlsMoveable._A.localPosition.y, _brushControlsMoveable._A.localPosition.z);
        _brushControlsMoveable._A.rotation = MirrorUtil.GetMirroredRotation(_brushControlsMoveable._A.rotation, this.transform);
        if (!IsWorkstation) {
          _brushControlsMoveable.MoveToA();
        }

        DisplayingChirality = newChirality;
      }
    }

    protected override void DoOnGrabbed() {
      base.DoOnGrabbed();

      _brushControlsEmergeable.TryVanish(IsWorkstation);
      _brushControlsEmerged = false;
      _brushWorkstationEmergeable.TryVanish(IsWorkstation);
    }

    protected override void DoOnMovementToWorkstationBegan() {
      base.DoOnMovementToWorkstationBegan();
    }

    protected override void DoOnMovementToWorkstationFinished() {
      base.DoOnMovementToWorkstationFinished();

      _brushControlsMoveable.MoveToB();

      if (!isGrasped) {
        _brushControlsEmergeable.TryEmerge(IsWorkstation);
        _brushControlsEmerged = true;
        _brushWorkstationEmergeable.TryEmerge(IsWorkstation);
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

}
