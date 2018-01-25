using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Recording.Examples {

  public class TransitionOnThumbsUp : TransitionBehaviour {

    private int _timer = 0;

    private void Update() {
      var left = Hands.Left;
      var right = Hands.Right;

      if ((left != null && isThumbsUp(left)) ||
          (right != null && isThumbsUp(right))) {
        _timer++;
      } else if (_timer > 0) {
        _timer--;
      }

      if (_timer > 60) {
        _timer = 0;
        Transition();
      }
    }

    private bool isThumbsUp(Hand hand) {
      foreach (var finger in hand.Fingers) {
        if (finger.Type == Finger.FingerType.TYPE_THUMB) {
          if (!finger.IsExtended) {
            return false;
          }
        } else {
          if (finger.IsExtended) {
            return false;
          }
        }
      }

      if (Vector3.Angle(hand.GetThumb().Direction.ToVector3(), Vector3.up) > 45) {
        return false;
      }

      return true;
    }
  }
}
