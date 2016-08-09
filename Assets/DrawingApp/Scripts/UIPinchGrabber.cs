using UnityEngine;
using System.Collections;

public class UIPinchGrabber : QueryableTrigger {

  private PinchMoveableUI _currentlyGrabbedUI = null;

  public void QueryPinchGrab() {
    _currentlyGrabbedUI = base.Query<PinchMoveableUI>();
    if (_currentlyGrabbedUI != null) {
      _currentlyGrabbedUI.BePinchedBy(this.transform);
    }
  }

  public void ReleasePinch() {
    if (_currentlyGrabbedUI != null) {
      _currentlyGrabbedUI.ReleaseFromPinch();
    }
  }

}
