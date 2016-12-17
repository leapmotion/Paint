using Leap;
using Leap.Unity;
using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchParticleEffect : MonoBehaviour {

  public bool startEnabled = false;
  public ParticleSystemController particleSystemController;
  public Chirality whichHand = Chirality.Right;

  void Start() {
    if (startEnabled) {
      particleSystemController.EnsureEmittingEnabled();
    }
    else {
      particleSystemController.EnsureEmittingDisabled();
    }
  }

  void Update() {
    Hand hand = Hands.Get(whichHand);
    if (hand == null) {
      particleSystemController.EnsureEmittingDisabled();
    }
    else {
      this.transform.position = hand.GetPinchPosition();
      this.transform.rotation = Quaternion.AngleAxis(70F, hand.Rotation.ToQuaternion() * Vector3.right) * hand.Rotation.ToQuaternion();

      if (hand.IsPinching()) {
        particleSystemController.EnsureEmittingEnabled();
      }
      else {
        particleSystemController.EnsureEmittingDisabled();
      }
    }

  }

}
