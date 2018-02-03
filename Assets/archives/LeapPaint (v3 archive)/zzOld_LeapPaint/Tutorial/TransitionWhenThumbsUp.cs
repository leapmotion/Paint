using UnityEngine;
using Leap;
using Leap.Unity;
using Leap.Unity.Recording;

public class TransitionWhenThumbsUp : TransitionBehaviour {

  [Header("Gesture")]
  public float withinAngle = 70f;
  public float forDuration = 1;

  private float _time = 0;

  private void Update() {
    if (isThumbsUp(Hands.Left) || isThumbsUp(Hands.Right)) {
      _time += Time.deltaTime;
    } else {
      _time = 0;
    }

    if (_time > forDuration) {
      _time = 0;
      Transition();
      return;
    }
  }

  private bool isThumbsUp(Hand hand) {
    if (hand == null) {
      return false;
    }

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

    var thumb = hand.GetThumb();
    return Vector3.Angle(thumb.Direction.ToVector3(), Vector3.up) < withinAngle;
  }

}
