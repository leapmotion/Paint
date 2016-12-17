using Leap;
using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialUI : MonoBehaviour {

  public Transform uiBase;

  public Chirality whichHand;

  void Start() {
    DetectDoublePinch doublePinch = GetComponent<DetectDoublePinch>();
    if (doublePinch == null) {
      doublePinch = gameObject.AddComponent<DetectDoublePinch>();
    }
    doublePinch.whichHand = whichHand;
    doublePinch.OnDoublePinch += DoOnDoublePinch;
  }

  private void DoOnDoublePinch() {
    Hand hand = Hands.Get(whichHand);
    if (hand != null) {
      if (hand.PalmNormal.ToVector3().IsFacing(hand.PalmPosition.ToVector3(), Camera.main.transform.position, 30F)) {
        DoOnUISummoned(hand);
      }
    }
  }

  private void DoOnUISummoned(Hand hand) {
    this.transform.position = hand.PalmPosition.ToVector3();
    this.transform.position = uiBase.transform.position;
    this.transform.LookAt(Camera.main.transform);
    this.transform.position = hand.PalmPosition.ToVector3();
  }

}
