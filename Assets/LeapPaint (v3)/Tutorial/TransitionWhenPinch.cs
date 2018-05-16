using Leap.Unity.Recording;

public class TransitionWhenPinch : TransitionBehaviour {

  public TutorialControl control;

  private void Update() {
    if(control.hasPaintedAtAll) {
      Transition();
    }
  }
}
