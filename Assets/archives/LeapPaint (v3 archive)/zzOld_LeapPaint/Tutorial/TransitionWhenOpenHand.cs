using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using Leap.Unity.Query;
using Leap.Unity.Recording;


public class TransitionWhenOpenHand : TransitionBehaviour {

  [Header("Gesture")]
  public float forDuration = 0;

  private float _time = 0;

  private void Update() {
    if (isOpen(Hands.Right)) {
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

  private bool isOpen(Hand hand) {
    if (hand == null) {
      return false;
    }

    return hand.Fingers.Query().All(h => h.IsExtended);
  }




}
