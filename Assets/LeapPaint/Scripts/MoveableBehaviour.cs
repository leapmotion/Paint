using UnityEngine;
using System.Collections;

public class MoveableBehaviour : MonoBehaviour {

  public Transform _A;
  public Transform _B;
  public Transform _C; // alternate A
  public bool _startAtA;

  private TweenHandle _movementTween;
  private bool _isCinsteadOfA = false;

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
    if (_isCinsteadOfA) {
      this.transform.position = Vector3.Lerp(_C.position, _B.position, value);
      this.transform.rotation = Quaternion.Slerp(_C.rotation, _B.rotation, value);
    }
    else {
      this.transform.position = Vector3.Lerp(_A.position, _B.position, value);
      this.transform.rotation = Quaternion.Slerp(_A.rotation, _B.rotation, value);
    }
  }

  public void MoveToA() {
    _isCinsteadOfA = false;
    MoveTo(_A);
  }

  public void MoveToB() {
    MoveTo(_B);
  }

  public void MoveToC() {
    _isCinsteadOfA = true;
    MoveTo(_C);
  }

  public void MoveToAorC() {
    MoveTo((_isCinsteadOfA ? _C : _A));
  }

  public void MoveTo(Transform t) {
    this.transform.position = t.position;
    this.transform.rotation = t.rotation;
  }

  public void TransitionToB() {
    _movementTween = ConstructMovementTween();
    _movementTween.Play(TweenDirection.FORWARD);
  }

  public void TransitionToA() {
    _movementTween = ConstructMovementTween();
    _movementTween.Play(TweenDirection.BACKWARD);
  }

  public void ToggleC() {
    _isCinsteadOfA = !_isCinsteadOfA;
  }

}
