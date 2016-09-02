using UnityEngine;
using System.Collections;

public class ColorWearableUI : WearableUI {

  [Header("Color Wearable UI")]
  public EmergeableBehaviour[] _emergeables;

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

  //protected override void DoOnMovementFromWorkstationBegan() {
  //  base.DoOnMovementFromWorkstationBegan();

  //  for (int i = 0; i < _emergeables.Length; i++) {
  //    _emergeables[i].TryVanish();
  //  }
  //}

}
