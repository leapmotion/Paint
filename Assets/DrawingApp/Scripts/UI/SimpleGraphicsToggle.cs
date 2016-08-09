using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SimpleGraphicsToggle : MonoBehaviour {

  private Graphic[] _graphics;

  void Awake() {
    _graphics = GetComponentsInChildren<Graphic>();
  }

  public void DisableAllGraphics() {
    for (int i = 0; i < _graphics.Length; i++) {
      _graphics[i].enabled = false;
    }
  }

  public void EnableAllGraphics() {
    for (int i = 0; i < _graphics.Length; i++) {
      _graphics[i].enabled = true;
    }
  }
}
