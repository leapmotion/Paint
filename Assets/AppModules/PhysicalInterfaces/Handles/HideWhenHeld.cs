using Leap.Unity.Animation;
using Leap.Unity.Attributes;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class HideWhenHeld : MonoBehaviour {

    [SerializeField, ImplementsInterface(typeof(IHandle))]
    private MonoBehaviour _handle;
    public IHandle handle {
      get {
        return _handle as IHandle;
      }
    }

    [SerializeField, ImplementsInterface(typeof(IPropertySwitch))]
    private MonoBehaviour _objectVisibleSwitch;
    public IPropertySwitch objectVisibleSwitch {
      get {
        return _objectVisibleSwitch as IPropertySwitch;
      }
    }

    void OnDisable() {
      if (objectVisibleSwitch.GetIsOffOrTurningOff() && handle.isHeld) {
        objectVisibleSwitch.On();
      }
    }

    void Update() {

      if (handle.isHeld && objectVisibleSwitch.GetIsOnOrTurningOn()) {
        objectVisibleSwitch.Off();
      }

      if (!handle.isHeld && objectVisibleSwitch.GetIsOffOrTurningOff()) {
        objectVisibleSwitch.On();
      }

    }

  }

}
