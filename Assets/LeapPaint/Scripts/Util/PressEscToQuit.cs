using UnityEngine;
using System.Collections;

public class PressEscToQuit : MonoBehaviour {
	
	void Update() {
    if (Input.GetKeyDown(KeyCode.Escape)) {
      Application.Quit();
    }
	}

}
