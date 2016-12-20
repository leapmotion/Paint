using Leap;
using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectDoublePinch : MonoBehaviour {

  public Chirality whichHand;

  public System.Action OnDoublePinch = () => { };

  private float doublePinchTime = 0.3F;
  private float doublePinchTimer = 0;
  private int successiveQuickPinchCount = 0;

  void Start() {
    DetectQuickPinch quickPinch = GetComponent<DetectQuickPinch>();
    if (quickPinch == null) {
      quickPinch = gameObject.AddComponent<DetectQuickPinch>();
    }
    quickPinch.whichHand = this.whichHand;
    quickPinch.OnQuickPinch += DoOnQuickPinch;
  }

  void Update() {
    if (doublePinchTimer > 0F) {
      doublePinchTimer -= Time.deltaTime;
      if (doublePinchTimer <= 0F) {
        doublePinchTimer = 0F;
        successiveQuickPinchCount = 0;
      }
      else {
        if (successiveQuickPinchCount == 2) {
          doublePinchTimer = 0F;
          successiveQuickPinchCount = 0;
          OnDoublePinch();
        }
      }
    }
  }

  private void DoOnQuickPinch() {
    successiveQuickPinchCount += 1;
    doublePinchTimer = doublePinchTime;
  }

}