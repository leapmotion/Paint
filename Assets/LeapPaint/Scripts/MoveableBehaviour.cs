using UnityEngine;
using System.Collections;

public class MoveableBehaviour : MonoBehaviour {

  public Transform _A;
  public Transform _B;
  public bool _startAtA = true;

  private TweenHandle _movementTween;

  void Start() {
    if (_startAtA) {
      MoveToA();
    }
    else {
      MoveToB();
    }
  }

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
