using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[System.Serializable]
public class ColliderEvent : UnityEvent<Collider> { }

public class PassOnTriggerStay : MonoBehaviour {

  public ColliderEvent PassedOnTriggerStay;

  protected void OnTriggerStay(Collider other) {
    PassedOnTriggerStay.Invoke(other);
  }

}
