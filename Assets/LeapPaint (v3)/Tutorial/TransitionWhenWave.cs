using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;
using Leap.Unity.Recording;

public class TransitionWhenWave : TransitionBehaviour {

  [Header("Gesture")]
  public Vector3 waveDirection;
  public float waveTime;

  private float _waveTimer = 0;

  private void Update() {
    if (isWaving(Hands.Left) || isWaving(Hands.Right)) {
      _waveTimer += Time.deltaTime;
    } else {
      _waveTimer = 0;
    }

    if (_waveTimer > waveTime) {
      _waveTimer = 0;
      Transition();
      return;
    }
  }

  private bool isWaving(Hand hand) {
    if (hand == null) {
      return false;
    }

    return Vector3.Angle(waveDirection, hand.PalmNormal.ToVector3()) < 75f;
  }



}
