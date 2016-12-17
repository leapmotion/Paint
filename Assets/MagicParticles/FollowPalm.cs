using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

public class FollowPalm : MonoBehaviour {

  public Chirality whichHand = Chirality.Left;

  void Update() {
    if (Hands.Left == null) return;
    Vector3 targetPosition = (Hands.Left.PalmPosition
                            + Hands.Left.PalmNormal * Hands.Left.PalmWidth
                            + Hands.Left.Direction *  Hands.Left.PalmWidth / 2F
                              ).ToVector3();
    this.transform.position = Vector3.Lerp(this.transform.position, targetPosition, 10F * Time.deltaTime);
  }

}
