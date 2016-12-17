using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public class PaintIntention : IIntentionDefinition {

  public float GetIntentionConfidence(Hand hand) {

    float pinchStrength = hand.PinchStrength;

    float palmTowardsGround = (Vector3.Dot(hand.PalmNormal.ToVector3(), Vector3.down));
    palmTowardsGround = 0F;

    // The more the palm faces towards the ground, the less likely the pinch is intended to imply painting.
    // But NOT facing towards the ground shouldn't contribute to a positive pinch intention.
    return pinchStrength - Mathf.Clamp01(palmTowardsGround) * 0.5F;

  }

}
