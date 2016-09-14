using UnityEngine;
using System.Collections;
using Leap.Unity;

public class MenuWearableUI : WearableUI {

  public EmergeableBehaviour[] _menuButtonEmergeables;
  public MoveableBehaviour[] _menuButtonMoveables;
  public PressableUI[] _menuButtons;

  public EmergeableBehaviour _fileMenuEmergeable;
  public EmergeableBehaviour _sceneMenuEmergeable;
  public EmergeableBehaviour _clearMenuEmergeable;

  private Menu _awaitingMenu = Menu.None;

  private bool _emergeableCallbacksInitialized = false;
  protected override void Start() {
    base.Start();

    if (!_emergeableCallbacksInitialized) {
      DoOnMenuBeganVanishing();
      _menuButtonEmergeables[0].OnFinishedEmerging += DoOnMenuFinishedEmerging;
      _menuButtonEmergeables[0].OnBegunVanishing += DoOnMenuBeganVanishing;

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

  protected override void DoOnMarbleActivated() {
    base.DoOnMarbleActivated();

    if (!IsGrabbed && !IsWorkstation) {
      if (_menuButtonEmergeables[0].IsEmergedOrEmerging) {
        for (int i = 0; i < _menuButtonEmergeables.Length; i++) {
          _menuButtonEmergeables[i].TryVanish();
        }
      }
      else {
        for (int i = 0; i < _menuButtonEmergeables.Length; i++) {
          _menuButtonEmergeables[i].TryEmerge();
        }
      }
    }
  }

  protected override void DoOnAnchorChiralityChanged(Chirality newChirality) {
    base.DoOnAnchorChiralityChanged(newChirality);

    if (newChirality != DisplayingChirality) {
      for (int i = 0; i < _menuButtonMoveables.Length; i++) {
        _menuButtonMoveables[i]._A.position = MirrorUtil.GetMirroredPosition(_menuButtonMoveables[i]._A.position, _menuButtonMoveables[i].transform.parent);
        _menuButtonMoveables[i]._A.rotation = MirrorUtil.GetMirroredRotation(_menuButtonMoveables[i]._A.rotation, _menuButtonMoveables[i].transform.parent);
        if (!IsWorkstation) {
          _menuButtonMoveables[i].MoveToA();
        }
      }
      DisplayingChirality = newChirality;
    }
  }

  protected override void DoOnMovementToWorkstationFinished() {
    base.DoOnMovementToWorkstationFinished();

    for (int i = 0; i < _menuButtonMoveables.Length; i++) {
      _menuButtonMoveables[i].MoveToB();
    }
    for (int i = 0; i < _menuButtonEmergeables.Length; i++) {
      _menuButtonEmergeables[i].TryEmerge();
    }

    if (_awaitingMenu != Menu.None) {
      if (_awaitingMenu == Menu.File) {
        EmergeFileMenu();
      }
      else if (_awaitingMenu == Menu.Scene) {
        EmergeSceneMenu();
      }
      else {
        EmergeClearMenu();
      }
    }
  }

  protected override void DoOnGrabbed() {
    base.DoOnGrabbed();
    _awaitingMenu = Menu.None;

    for (int i = 0; i < _menuButtonEmergeables.Length; i++) {
      _menuButtonEmergeables[i].TryVanish();
    }
    _fileMenuEmergeable.TryVanish();
    _sceneMenuEmergeable.TryVanish();
    _clearMenuEmergeable.TryVanish();
  }

  protected override void DoOnReturnedToAnchor() {
    base.DoOnReturnedToAnchor();

    for (int i = 0; i < _menuButtonMoveables.Length; i++) {
      _menuButtonMoveables[i].MoveToA();
    }
  }

  #endregion

  private void EmergeFileMenu() {
    _fileMenuEmergeable.TryEmerge();
    _sceneMenuEmergeable.TryVanish();
    _clearMenuEmergeable.TryVanish();
  }

  private void EmergeSceneMenu() {
    _fileMenuEmergeable.TryVanish();
    _sceneMenuEmergeable.TryEmerge();
    _clearMenuEmergeable.TryVanish();
  }

  private void EmergeClearMenu() {
    _fileMenuEmergeable.TryVanish();
    _sceneMenuEmergeable.TryVanish();
    _clearMenuEmergeable.TryEmerge();
  }

  public void OpenFileMenu() {
    if (!IsWorkstation) {
      ActivateWorkstationTransitionFromAnchor();
      _awaitingMenu = Menu.File;
      for (int i = 0; i < _menuButtonEmergeables.Length; i++) {
        _menuButtonEmergeables[i].TryVanish();
      }
    }
    else {
      EmergeFileMenu();
    }
  }

  public void OpenSceneMenu() {
    if (!IsWorkstation) {
      ActivateWorkstationTransitionFromAnchor();
      _awaitingMenu = Menu.Scene;
      for (int i = 0; i < _menuButtonEmergeables.Length; i++) {
        _menuButtonEmergeables[i].TryVanish();
      }
    }
    else {
      EmergeSceneMenu();
    }
  }

  public void OpenClearMenu() {
    if (!IsWorkstation) {
      ActivateWorkstationTransitionFromAnchor();
      _awaitingMenu = Menu.Clear;
      for (int i = 0; i < _menuButtonEmergeables.Length; i++) {
        _menuButtonEmergeables[i].TryVanish();
      }
    }
    else {
      EmergeClearMenu();
    }
  }

}

public enum Menu {
  None,
  File,
  Scene,
  Clear
}
