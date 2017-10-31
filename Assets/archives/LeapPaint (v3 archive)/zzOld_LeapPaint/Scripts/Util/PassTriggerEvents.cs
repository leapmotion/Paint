using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace Leap.Unity.LeapPaint_v3 {

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


}
