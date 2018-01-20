using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;

public class CreateInteractionBehaviour : MonoBehaviour {

  public Transform toFollow;

  private Rigidbody _rigidbody;

  private void Start() {
    _rigidbody = GetComponent<Rigidbody>();
    _rigidbody.isKinematic = true;
  }

  void FixedUpdate() {
    _rigidbody.MovePosition(toFollow.position);
    _rigidbody.MoveRotation(toFollow.rotation);
  }

  public void Create() {
    _rigidbody.isKinematic = false;
    gameObject.AddComponent<InteractionBehaviour>();
    Destroy(this);
  }
}
