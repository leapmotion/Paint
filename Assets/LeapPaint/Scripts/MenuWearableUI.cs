using UnityEngine;
using System.Collections;
using Leap.Unity;

namespace Leap.Paint {

  public class MenuWearableUI : WearableUI {

    [Header("Menu Wearable UI")]
    public EmergeableBehaviour[] _menuButtonEmergeables;
    public MoveableBehaviour[] _menuButtonMoveables;
    public PressableUI[] _menuButtons;

    public EmergeableBehaviour _fileMenuEmergeable;
    public EmergeableBehaviour _sceneMenuEmergeable;
    public EmergeableBehaviour _clearMenuEmergeable;

    private Menu _awaitingMenu = Menu.None;

    public override float GetWorkstationDangerZoneRadius() {
      return 0.15F;
    }

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

    public override float GetAnchoredDangerZoneRadius() {
      return 0.05F;
    }

    protected override void DoOnMarbleActivated() {
      base.DoOnMarbleActivated();

      if (!IsGrabbed && !IsWorkstation) {
        if (_menuButtonEmergeables[0].IsEmergedOrEmerging) {
          for (int i = 0; i < _menuButtonEmergeables.Length; i++) {
            _menuButtonEmergeables[i].TryVanish(IsWorkstation);
          }
        }
        else {
          for (int i = 0; i < _menuButtonEmergeables.Length; i++) {
            _menuButtonEmergeables[i].TryEmerge(IsWorkstation);
          }
        }
      }
    }

    protected override void DoOnAnchorChiralityChanged(Chirality newChirality) {
      base.DoOnAnchorChiralityChanged(newChirality);

      if (newChirality != DisplayingChirality) {
        for (int i = 0; i < _menuButtonMoveables.Length; i++) {
          _menuButtonMoveables[i]._A.localPosition = new Vector3(-_menuButtonMoveables[i]._A.localPosition.x, _menuButtonMoveables[i]._A.localPosition.y, _menuButtonMoveables[i]._A.localPosition.z);
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
        _menuButtonEmergeables[i].TryEmerge(isInWorkstation: true);
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
        _menuButtonEmergeables[i].TryVanish(IsWorkstation);
      }
      _fileMenuEmergeable.TryVanish(IsWorkstation);
      _sceneMenuEmergeable.TryVanish(IsWorkstation);
      _clearMenuEmergeable.TryVanish(IsWorkstation);
    }

    protected override void DoOnReturnedToAnchor() {
      base.DoOnReturnedToAnchor();

      for (int i = 0; i < _menuButtonMoveables.Length; i++) {
        _menuButtonMoveables[i].MoveToA();
      }
    }

    #endregion

    private void EmergeFileMenu() {
      _fileMenuEmergeable.TryEmerge(isInWorkstation: false);
      _sceneMenuEmergeable.TryVanish(IsWorkstation);
      _clearMenuEmergeable.TryVanish(IsWorkstation);
    }

    private void EmergeSceneMenu() {
      _fileMenuEmergeable.TryVanish(IsWorkstation);
      _sceneMenuEmergeable.TryEmerge(isInWorkstation: false);
      _clearMenuEmergeable.TryVanish(IsWorkstation);
    }

    private void EmergeClearMenu() {
      _fileMenuEmergeable.TryVanish(IsWorkstation);
      _sceneMenuEmergeable.TryVanish(IsWorkstation);
      _clearMenuEmergeable.TryEmerge(isInWorkstation: false);
    }

    public void OpenFileMenu() {
      if (!IsWorkstation) {
        ActivateWorkstationTransitionFromAnchor();
        _awaitingMenu = Menu.File;
        for (int i = 0; i < _menuButtonEmergeables.Length; i++) {
          _menuButtonEmergeables[i].TryVanish(IsWorkstation);
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
          _menuButtonEmergeables[i].TryVanish(IsWorkstation);
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
          _menuButtonEmergeables[i].TryVanish(IsWorkstation);
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


}