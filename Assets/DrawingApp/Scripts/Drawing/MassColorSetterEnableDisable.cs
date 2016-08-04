using UnityEngine;
using System.Collections;

public class MassColorSetterEnableDisable : MonoBehaviour {

  ColorSetter[] setters;

  protected void Start() {
    setters = GetComponentsInChildren<ColorSetter>();
  }

  public void EnableAllColorSetters() {
    if (setters != null) {
      for (int i = 0; i < setters.Length; i++) {
        setters[i].Enable();
      }
    }
  }

  public void DisableAllColorSetters() {
    if (setters != null) {
      for (int i = 0; i < setters.Length; i++) {
        setters[i].Disable();
      }
    }
  }

}
