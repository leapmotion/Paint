using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class UIPinchGrabber : QueryableTrigger {

  private PinchMoveableUI _currentlyGrabbedUI = null;

  public UnityEvent OnQueryFailed;

  public void QueryPinchGrab() {
    _currentlyGrabbedUI = base.Query<PinchMoveableUI>();
    if (_currentlyGrabbedUI != null) {
      _currentlyGrabbedUI.BePinchedBy(this.transform);
    }
    else {
      OnQueryFailed.Invoke();
    }
  }

  public void ReleasePinch() {
    if (_currentlyGrabbedUI != null) {
      _currentlyGrabbedUI.ReleaseFromPinch();
    }
  }

}
