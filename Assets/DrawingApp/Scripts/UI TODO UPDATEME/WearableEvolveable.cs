using UnityEngine;
using System.Collections;

public class WearableEvolveable : EvolveableUI {

  public WearableUI _wearableUI;

  public override Transform GetAnchor() {
    return _wearableUI._anchorPoint;
  }

  public override void Appear(bool asWhite) {

    // We should only ACTUALLY appear as a wearable if we're currently not independent and the wearable ShouldDisplay.

    if (_wearableUI.ShouldDisplay) {
      base.Appear(asWhite);
    }
    else {
      if (asWhite) {
        for (int i = 0; i < _graphics.Length; i++) {
          _graphics[i].color = Color.white;
        }
      }
    }
  }

  protected override void UpdateMovement() {
    if (_moving) {
      _movementTimer += Time.deltaTime;

      // Wearable facing (rotation) is handled by the WearableUI class, so only modify position and not rotation here for the Evolveable movement implementation.

      this.transform.position = Vector3.Lerp(_moveFrom.position, _moveTo.position, _movementCurve.Evaluate(_movementTimer / _movementDuration));

      if (_movementTimer >= _movementDuration) {
        _moving = false;

        this.transform.position = _moveTo.position;
      }
    }
  }

}
