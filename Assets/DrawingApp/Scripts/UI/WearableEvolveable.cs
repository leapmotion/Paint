using UnityEngine;
using System.Collections;

public class WearableEvolveable : EvolveableUI {

  public WearableUI _wearableUI;

  public override Transform GetAnchor() {
    return _wearableUI._anchorPoint;
  }

  public override void Appear() {
    // We should only ACTUALLY appear as a wearable if we're currently not independent and the wearable ShouldDisplay.
    if (_wearableUI.ShouldDisplay) {
      base.Appear();
    }
  }

  protected override void UpdateMovement() {
    // Wearable facing (rotation) is handled by the WearableUI class, so only modify position and not rotation here for the Evolveable movement implementation.

    if (_moving) {
      _movementTimer += Time.deltaTime;

      if (_movementDuration > 0F) {
        this.transform.position = Vector3.LerpUnclamped(_moveFrom.position, _moveTo.position, _movementCurve.Evaluate(_movementTimer / _movementDuration));
        //this.transform.rotation = Quaternion.SlerpUnclamped(_moveFrom.rotation, _moveTo.rotation, _movementCurve.Evaluate(_movementTimer / _movementDuration));
      }

      if (_movementTimer >= _movementDuration) {
        _moving = false;

        this.transform.position = _moveTo.position;
        //this.transform.rotation = _moveTo.rotation;
      }
    }
  }

}
