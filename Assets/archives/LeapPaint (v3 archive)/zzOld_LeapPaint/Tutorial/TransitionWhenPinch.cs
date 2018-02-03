using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using Leap.Unity.Recording;

public class TransitionWhenPinch : TransitionBehaviour {

  [Header("Gesture")]
  public float forDuration = 1;

  private float _time = 0;

  private void Update() {
    if (isPinching(Hands.Left) || isPinching(Hands.Right)) {
      _time += Time.deltaTime;
    } else {
      _time = 0;
    }

    if (_time > forDuration) {
      _time = 0;
      Transition();
      return;
    }
  }

  private bool isPinching(Hand hand) {
    if (hand == null) {
      return false;
    }

    return hand.IsPinching();
  }

}
