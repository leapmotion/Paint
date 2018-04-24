using UnityEngine;
using System.Collections;

namespace Leap.Unity.LeapPaint_v3 {



  public class PressEscToQuit : MonoBehaviour {
	
	  void Update() {
      if (Input.GetKeyDown(KeyCode.Escape)) {
        Application.Quit();
      }
	  }

  }


}