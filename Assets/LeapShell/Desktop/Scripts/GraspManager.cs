using UnityEngine;
using System;
using System.Collections.Generic;
using Leap.Unity;
using Leap.Unity.Attributes;

public class GraspManager : MonoBehaviour {

  [AutoFind]
  [SerializeField]
  private Shelf _shelf;

  [AutoFind]
  [SerializeField]
  private AppList _appList;

  [SerializeField]
  private PinchDetector _leftPinch;

  [SerializeField]
  private PinchDetector _rightPinch;

  [MinValue(0)]
  [SerializeField]
  private float _maxDistToButton = 0.1f;

  private List<AppButton> _buttons = new List<AppButton>();

  private PinchDetector _graspingDetector;
  private AppGrabbable _graspedApp;

  public void AddButton(AppButton button) {
    _buttons.Add(button);
  }

  public void AddButtons(IEnumerable<AppButton> buttons) {
    _buttons.AddRange(buttons);
  }

  public void RemoveButton(AppButton button) {
    _buttons.Remove(button);
  }

  public void RemoveButtons(IEnumerable<AppButton> buttons) {
    foreach (var toRemove in buttons) {
      RemoveButton(toRemove);
    }
  }

  void Update() {
    if (_graspingDetector == null) {
      doNonGraspState();
    } else {
      doGraspedState();
    }
  }

  private void doNonGraspState() {
    if (tryStartGrasp(_leftPinch)) {
      return;
    }

    if (tryStartGrasp(_rightPinch)) {
      return;
    }
  }

  private void doGraspedState() {
    if (_graspingDetector.DidRelease) {
      //Handle release
      _shelf.EndMove(_graspedApp);
      _graspingDetector = null;
      _graspedApp = null;
      return;
    }

    _graspedApp.UpdatePosition(_graspingDetector.Position);
    _shelf.UpdateMovedAppPosition(_graspingDetector.Position);

  }

  private bool tryStartGrasp(PinchDetector detector) {
    if (!detector.DidStartHold) {
      return false;
    }

    AppButton closestButton = null;
    float closestDist = float.MaxValue;
    for (int i = 0; i < _buttons.Count; i++) {
      AppButton button = _buttons[i];
      float dist = Vector3.Distance(detector.Position, button.transform.position);
      if (dist < closestDist) {
        closestButton = button;
        closestDist = dist;
      }
    }

    if (closestDist > _maxDistToButton) {
      return false;
    }

    _graspingDetector = detector;
    _graspedApp = closestButton.InstantiateGrabbableIcon();
    _shelf.BeginMove(closestButton);
    _appList.Close();

    return true;
  }

}
