using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;

public class CreateInteractionBehaviour : MonoBehaviour {

  public Transform toFollow;

  private Rigidbody _rigidbody;
  private InteractionBehaviour _ie;

  private void Start() {
    _rigidbody = GetComponent<Rigidbody>();
    _ie = GetComponent<InteractionBehaviour>();
    _ie.OnGraspEnd += () => _rigidbody.isKinematic = false;
    _rigidbody.isKinematic = true;
  }

  void FixedUpdate() {
    _rigidbody.MovePosition(toFollow.position);
    _rigidbody.MoveRotation(toFollow.rotation);

    if (_ie.isGrasped) {
      Destroy(this);
      _rigidbody.isKinematic = false;
    }
  }
}
