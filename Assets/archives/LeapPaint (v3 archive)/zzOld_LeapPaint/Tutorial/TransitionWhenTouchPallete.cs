using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Recording;

public class TransitionWhenTouchPallete : TransitionBehaviour {

  public TutorialControl tutorialControl;

  private void Update() {
    if (tutorialControl.colorPalleteHasBeenTouched) {
      Transition();
      return;
    }
  }
}
