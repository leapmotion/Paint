using UnityEngine;
using System.Collections;
using Leap.Unity.Animation;
using Leap.Unity.Attributes;
using UnityEngine.Serialization;

namespace Leap.Unity.LeapPaint_v3 {


  public class MoveableBehaviour : MonoBehaviour {

    [QuickButton("Move To Now", "MoveToA")]
    [FormerlySerializedAs("_A")]
    public Transform A;

    [QuickButton("Move To Now", "MoveToB")]
    [FormerlySerializedAs("_B")]
    public Transform B;

    private Tween _movementTween;

    public void MoveToA() {
      MoveTo(A);
    }

    public void MoveToB() {
      MoveTo(B);
    }

    public void MoveTo(Transform t) {
      if (t != null) {
        this.transform.position = t.position;
        this.transform.rotation = t.rotation;
      }
    }

  }


}
