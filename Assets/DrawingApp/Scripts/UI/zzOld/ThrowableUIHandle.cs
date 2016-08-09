using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class ThrowableUIHandle : MonoBehaviour {

  public Transform uiElement;
  public float _throwVelocityThreshold = 1F;

  private Rigidbody _body;
  private bool _followUIElement = true;

  /// <summary>
  /// Called when the handle is grabbed by a hand. The Behaviour automatically stops following the UI Element when grabbed.
  /// </summary>
  public UnityEvent OnGraspBeginEvent;

  /// <summary>
  /// Called when the handle is released and has a palm velocity lower than the threshold.
  /// </summary>
  public UnityEvent OnGraspDroppedEvent;

  /// <summary>
  /// Called when the handle is released and has a palm velocity higher than the threshold.
  /// </summary>
  public UnityEvent OnGraspThrownEvent;

  protected void Start() {
    _body = GetComponent<Rigidbody>();
    _body.isKinematic = true;
    _body.useGravity = false;
  }

  protected void Update() {
    if (_followUIElement) {
      this.transform.position = uiElement.transform.position;
      this.transform.rotation = uiElement.transform.rotation;
    }
  }

  public void OnGraspBegin() {
    _followUIElement = false;
    OnGraspBeginEvent.Invoke();
  }

  public void OnGraspEnd(Leap.Hand lastHand) {
    if (lastHand.PalmVelocity.Magnitude >= _throwVelocityThreshold) {
      _body.isKinematic = false;
      _body.useGravity = true;
      _body.velocity = new Vector3(lastHand.PalmVelocity.x, lastHand.PalmVelocity.y, lastHand.PalmVelocity.z);
      OnGraspThrownEvent.Invoke();
    }
    else {
      _followUIElement = true;
      OnGraspDroppedEvent.Invoke();
    }
  }

  public void FollowUIElement() {
    _followUIElement = true;
    _body.isKinematic = true;
    _body.useGravity = false;
  }

  public void StopFollowUIElement() {
    _followUIElement = false;
  }

}
