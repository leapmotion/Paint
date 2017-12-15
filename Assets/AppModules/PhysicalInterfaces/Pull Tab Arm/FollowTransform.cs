using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  [AddComponentMenu("")]
  public class FollowTransform : MonoBehaviour {

    public Transform target;

    private void Update() {
      if (target != null && target.gameObject.activeInHierarchy) {
        this.transform.position = target.transform.position;
        this.transform.rotation = target.transform.rotation;
      }
    }

  }

}
