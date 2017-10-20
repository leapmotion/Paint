using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Gestures {

  public abstract class TwoHandedPositionalGesture : TwoHandedGesture {

    protected override bool ShouldGestureActivate(Hand leftHand, Hand rightHand) {
      throw new System.NotImplementedException();
    }

    protected override bool ShouldGestureDeactivate(Hand leftHand, Hand rightHand, out Gesture.DeactivationReason? deactivationReason) {
      throw new System.NotImplementedException();
    }
  }

}
