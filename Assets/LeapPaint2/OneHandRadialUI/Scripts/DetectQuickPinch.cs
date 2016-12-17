using Leap;
using Leap.Unity;
using Leap.Unity.Visualization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectQuickPinch : MonoBehaviour {

  public Chirality whichHand;

  public System.Action OnQuickPinch = () => { };

  private float releaseTime = 0.2F;
  private float releaseTimer = 0F;

  private DeltaFloatBuffer pinchStrengthBuffer = new DeltaFloatBuffer(5);

  public enum QuickPinchState {
    WaitingForPinch,
    WaitingForRelease
  }
  public QuickPinchState state;

  void Update() {
    Hand hand = Hands.Get(whichHand);
    if (hand != null) {
      float pinchStrength = hand.PinchStrength;
      pinchStrengthBuffer.Add(pinchStrength, Time.time);

      if (pinchStrengthBuffer.isFull) {
        float pinchStrengthVelocity = pinchStrengthBuffer.Delta();

        if (pinchStrengthVelocity > 5F && pinchStrength > 0.7F) {
          if (state == QuickPinchState.WaitingForPinch) {
            state = QuickPinchState.WaitingForRelease;
            releaseTimer = releaseTime;
          }
        }

        if (state == QuickPinchState.WaitingForRelease) {
          if (releaseTimer > 0F) {
            releaseTimer -= Time.deltaTime;
            if (releaseTimer <= 0F) {
              releaseTimer = 0F;
              state = QuickPinchState.WaitingForPinch;
            }
            else if (pinchStrengthVelocity < -5F && pinchStrength < 0.7F) {
              OnQuickPinch();
              releaseTimer = 0F;
              state = QuickPinchState.WaitingForPinch;
            }
          }
        }

        //Visualize.Float("Pinch Strength Velocity", pinchStrengthVelocity)
        //         .AtPosition(this.transform.position + this.transform.forward * 0.2F + this.transform.right * 0.4F)
        //         .MaxSamples(64)
        //         .Range(-10, 10)
        //         .Color(Color.magenta);
        //Visualize.Float("Pinch Strength", hand.PinchStrength)
        //         .AtPosition(this.transform.position + this.transform.forward * 0.2F + this.transform.right * 0.4F + this.transform.up * -0.15F)
        //         .MaxSamples(64)
        //         .Range(0, 1)
        //         .Color(Color.cyan);

      }
    }
    else {
      pinchStrengthBuffer.Clear();
    }
  }

}
