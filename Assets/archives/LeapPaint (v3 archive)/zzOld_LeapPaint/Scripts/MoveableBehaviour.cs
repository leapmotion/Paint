using UnityEngine;
using System.Collections;
using Leap.Unity.Animation;
using Leap.Unity.Attributes;
using UnityEngine.Serialization;
using UnityEngine.Events;

namespace Leap.Unity.LeapPaint_v3 {


  public class MoveableBehaviour : MonoBehaviour {

    [QuickButton("Move To Now", "MoveToA")]
    [FormerlySerializedAs("_A")]
    public Transform A;

    [QuickButton("Move To Now", "MoveToB")]
    [FormerlySerializedAs("_B")]
    public Transform B;

    public UnityEvent OnMoveToAEvent;
    public UnityEvent OnMoveToBEvent;

    private Tween _movementTween;

    public void MoveToA() {
      moveTo(A);
      OnMoveToAEvent.Invoke();
    }

    public void MoveToB() {
      moveTo(B);
      OnMoveToBEvent.Invoke();
    }

    private void moveTo(Transform t) {
      if (t != null) {
        this.transform.position = t.position;
        this.transform.rotation = t.rotation;
      }
    }

  }


}
