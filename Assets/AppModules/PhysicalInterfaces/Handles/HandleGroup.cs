using Leap.Unity.Interaction;
using Leap.Unity.Query;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  using IntObj = InteractionBehaviour;

  public class HandleGroup : MonoBehaviour {

    public IntObj[] handles;

    private HashSet<IntObj> _heldHandles
      = new HashSet<IntObj>();

    void Start() {
      foreach (var handle in handles) {
        handle.OnObjectGraspBegin += onHandleGraspBegin;
        handle.OnObjectGraspEnd += onHandleGraspEnd;
      }
    }

    private void onHandleGraspBegin(IntObj handle) {
      _heldHandles.Add(handle);

      foreach (var otherHandle in handles.Query()
                                         .Where(h => h != handle && !h.isGrasped)) {
        otherHandle.rigidbody.isKinematic = false;
      }
    }

    private void onHandleGraspEnd(IntObj handle) {
      if (_heldHandles.Count == 1) {
        handle.rigidbody.isKinematic = true;
      }

      _heldHandles.Remove(handle);
    }

  }

}
