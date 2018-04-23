using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Recording;

public class TransitionWhenColorPalleteExpanded : TransitionBehaviour {

  public TutorialControl control;

  void Update() {
    if (control.colorPalleteHasBeenExpanded) {
      Transition();
      return;
    }
  }
}
