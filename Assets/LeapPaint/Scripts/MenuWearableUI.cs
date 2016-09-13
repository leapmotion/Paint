using UnityEngine;
using System.Collections;

public class MenuWearableUI : WearableUI {

  public EmergeableBehaviour _menuButtonsEmergeable;
  public PressableUI[] _menuButtons;

  private bool _emergeableCallbacksInitialized = false;
  protected override void Start() {
    base.Start();

    if (!_emergeableCallbacksInitialized) {
      DoOnMenuBeganVanishing();
      _menuButtonsEmergeable.OnFinishedEmerging += DoOnMenuFinishedEmerging;
      _menuButtonsEmergeable.OnBegunVanishing += DoOnMenuBeganVanishing;

      _emergeableCallbacksInitialized = true;
    }
  }

  private void DoOnMenuBeganVanishing() {
    for (int i = 0; i < _menuButtons.Length; i++) {
      _menuButtons[i].enabled = false;
    }
  }

  private void DoOnMenuFinishedEmerging() {
    for (int i = 0; i < _menuButtons.Length; i++) {
      _menuButtons[i].enabled = true;
    }
  }

  #region WearableUI Implementations

  protected override void DoOnFingerPressedMarble() {
    base.DoOnFingerPressedMarble();

    if (_menuButtonsEmergeable.IsEmergedOrEmerging) {
      _menuButtonsEmergeable.TryVanish();
    }
    else {
      _menuButtonsEmergeable.TryEmerge();
    }
  }



  #endregion

}
