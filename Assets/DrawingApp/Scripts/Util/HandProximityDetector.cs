using UnityEngine;
using System.Collections;
using Leap.Unity;

public class HandProximityDetector : Detector {

  public IHandModel _hand;

  public IHandModel _otherHand;

  public float _distanceToActivate = 1F;
  public float _distanceToDeactivate = 1.2F;

  public float GetDistanceBetweenHands() {
    if (!_hand.IsTracked || !_otherHand.IsTracked) {
      return float.PositiveInfinity;
    }
    return (_hand.GetLeapHand().PalmPosition - _otherHand.GetLeapHand().PalmPosition).Magnitude;
  }

  protected void Update() {
    //Debug.Log(GetDistanceBetweenHands());
    if (!IsActive && (GetDistanceBetweenHands() <= _distanceToActivate)) {
      Activate();
    }
    else if (IsActive && (GetDistanceBetweenHands() > _distanceToDeactivate)) {
      Deactivate();
    }
  }

}
