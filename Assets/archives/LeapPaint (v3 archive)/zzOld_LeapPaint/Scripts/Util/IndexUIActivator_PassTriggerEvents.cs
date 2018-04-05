using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Leap.Unity.Interaction;

namespace Leap.Unity.LeapPaint_v3 {

  [System.Serializable]
  public class ColliderEvent : UnityEvent<Collider> { }

  public class IndexUIActivator_PassTriggerEvents : MonoBehaviour {

    public ColliderEvent PassedOnTriggerEnter = new ColliderEvent();
    public ColliderEvent PassedOnTriggerStay = new ColliderEvent();
    public ColliderEvent PassedOnTriggerExit = new ColliderEvent();

    private void Start() { }

    protected void OnTriggerEnter(Collider other) {
      if (!enabled) return;

      if (other.GetComponent<IndexUIActivator>() == null) { return; }

      PassedOnTriggerEnter.Invoke(other);
    }

    protected void OnTriggerStay(Collider other) {
      if (!enabled) return;

      if (other.GetComponent<IndexUIActivator>() == null) { return; }

      PassedOnTriggerStay.Invoke(other);
    }

    protected void OnTriggerExit(Collider other) {
      if (!enabled) return;

      if (other.GetComponent<IndexUIActivator>() == null) { return; }

      PassedOnTriggerExit.Invoke(other);
    }

  }


}
