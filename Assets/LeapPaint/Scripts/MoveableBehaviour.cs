using UnityEngine;
using System.Collections;

public class MoveableBehaviour : MonoBehaviour {

  public Transform _A;
  public Transform _B;
  public bool _startAtA;

  private TweenHandle _movementTween;

  void Start() {
    _movementTween = ConstructMovementTween();

    if (_startAtA) {
      MoveToA();
      _movementTween.Progress = 0F;
    }
    else {
      MoveToB();
      _movementTween.Progress = 1F;
    }
  }

  private TweenHandle ConstructMovementTween() {
    return Tween.Value(0F, 1F, OnMovementValue)
      .OverTime(0.2F)
      .Smooth(TweenType.SMOOTH)
      .Keep();
  }

  private void OnMovementValue(float value) {
    this.transform.position = Vector3.Lerp(_A.position, _B.position, value);
    this.transform.rotation = Quaternion.Slerp(_A.rotation, _B.rotation, value);
  }

  public void MoveToA() {
    MoveTo(_A);
  }

  public void MoveToB() {
    MoveTo(_B);
  }

  public void SetA(Transform t) {
    _A = t;
    ConstructMovementTween();
  }

  public void MoveTo(Transform t) {
    this.transform.position = t.position;
    this.transform.rotation = t.rotation;
  }

  public void TransitionToB() {
    _movementTween.Play(TweenDirection.FORWARD);
  }

  public void TransitionToA() {
    _movementTween.Play(TweenDirection.BACKWARD);
  }

}
