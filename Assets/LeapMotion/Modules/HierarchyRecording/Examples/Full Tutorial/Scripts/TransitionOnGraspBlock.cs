using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Recording.Examples {
  using Interaction;

  public class TransitionOnGraspBlock : TransitionBehaviour {

    [SerializeField]
    private GameObject _block;

    private void Update() {
      var ie = _block.GetComponent<InteractionBehaviour>();
      if (ie.isGrasped) {
        Transition();
      }
    }
  }
}
