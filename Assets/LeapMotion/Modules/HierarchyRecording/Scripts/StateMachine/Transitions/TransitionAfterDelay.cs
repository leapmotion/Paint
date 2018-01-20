using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Recording {

  public class TransitionAfterDelay : TransitionBehaviour {

    public float delay = 1;

    private float _enterTime;

    private void OnEnable() {
      _enterTime = Time.time;
    }

    private void Update() {
      if ((Time.time - _enterTime) > delay) {
        Transition();
      }
    }
  }
}
