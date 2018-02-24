using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseIfRightHand : MonoBehaviour {

  // Use this for initialization
  void Start() {

  }

  // Update is called once per frame
  void Update() {
    if (Leap.Unity.Hands.Right != null) {
      Debug.Break();
    }
  }
}
