using UnityEngine;
using System.Collections;
using Leap.Unity.Interaction;
using UnityEngine.Events;

public class ThrowableUIHandle : InteractionBehaviour {

  public Transform uiElement;
  public float _throwVelocityThreshold = 1F;

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
    this.isKinematic = true;
    this.useGravity = false;
  }

  protected void Update() {
    if (_followUIElement) {
      this.transform.position = uiElement.transform.position;
      this.transform.rotation = uiElement.transform.rotation;
    }
  }

  protected override void OnGraspBegin() {
    base.OnGraspBegin();

    _followUIElement = false;
    OnGraspBeginEvent.Invoke();
  }

  protected override void OnGraspEnd(Leap.Hand lastHand) {
    base.OnGraspEnd(lastHand);

    if (lastHand.PalmVelocity.Magnitude >= _throwVelocityThreshold) {
      this.isKinematic = false;
      this.useGravity = true;
      this.rigidbody.velocity = new Vector3(lastHand.PalmVelocity.x, lastHand.PalmVelocity.y, lastHand.PalmVelocity.z);
      OnGraspThrownEvent.Invoke();
    }
    else {
      _followUIElement = true;
      OnGraspDroppedEvent.Invoke();
    }
  }

  public void FollowUIElement() {
    _followUIElement = true;
    this.isKinematic = true;
    this.useGravity = false;
  }

  public void StopFollowUIElement() {
    _followUIElement = false;
  }

}
