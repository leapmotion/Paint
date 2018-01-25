using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Recording {

  public class TransitionOnKeypress : TransitionBehaviour {

    public KeyCode key;

    private void Update() {
      if (Input.GetKeyDown(key)) {
        Transition();
      }
    }
  }
}
