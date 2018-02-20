using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.OSControl {

  public class SetCursorTest : MonoBehaviour {

    private void Update() {
      if (Input.GetKeyDown(KeyCode.Space)) {
        CursorControl.Test_MoveCursorToOrigin();
      }
    }

  }

}
