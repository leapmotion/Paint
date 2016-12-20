using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[System.Serializable]
public class ColliderEvent : UnityEvent<Collider> { }

public class PassTriggerEvents : MonoBehaviour {

  public ColliderEvent PassedOnTriggerEnter = new ColliderEvent();
  public ColliderEvent PassedOnTriggerStay = new ColliderEvent();
  public ColliderEvent PassedOnTriggerExit = new ColliderEvent();

  protected void OnTriggerEnter(Collider other) {
    PassedOnTriggerEnter.Invoke(other);
  }

  protected void OnTriggerStay(Collider other) {
    PassedOnTriggerStay.Invoke(other);
  }

  protected void OnTriggerExit(Collider other) {
    PassedOnTriggerExit.Invoke(other);
  }

}
