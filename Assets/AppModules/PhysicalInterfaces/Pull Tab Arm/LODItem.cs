using Leap.Unity.Animation;
using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODItem : MonoBehaviour {

  [SerializeField, ImplementsInterface(typeof(IPropertySwitch))]
  private MonoBehaviour _switch;
  public IPropertySwitch propertySwitch {
    get {
      return _switch as IPropertySwitch;
    }
  }

}
