using System;
using UnityEngine;

namespace Leap.Unity.Recording {

  public class StateMachine : MonoBehaviour {

    public GameObject activeState {
      get {
        for (int i = 0; i < transform.childCount; i++) {
          var child = transform.GetChild(i).gameObject;
          if (child.activeInHierarchy) {
            return child;
          }
        }
        return null;
      }
    }

    private void Awake() {
      int enabledCount = 0;
      for (int i = 0; i < transform.childCount; i++) {
        if (transform.GetChild(i).gameObject.activeSelf) {
          enabledCount++;
        }
      }

      //If there is not one state currently enabled, disable all but the first
      if (enabledCount != 1) {
        for (int i = 0; i < transform.childCount; i++) {
          transform.GetChild(i).gameObject.SetActive(i == 0);
        }
      }
    }
  }
}
