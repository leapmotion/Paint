using UnityEngine;
using System.Collections;
using Leap.Unity.Animation;

public class MoveableBehaviour : MonoBehaviour {

  public Transform _A;
  public Transform _B;

  private Tween _movementTween;

  public void MoveToA() {
    MoveTo(_A);
  }

  public void MoveToB() {
    MoveTo(_B);
  }

  public void MoveTo(Transform t) {
    this.transform.position = t.position;
    this.transform.rotation = t.rotation;
  }

}
