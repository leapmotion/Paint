using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

  public Transform anchorTransform;

  public MoveableBehaviour _primaryPaletteMoveable;

  void Update() {
    if (Input.GetKeyDown(KeyCode.Space)) {
      _primaryPaletteMoveable._A.position = MirrorUtil.GetMirroredPosition(_primaryPaletteMoveable._A.position, anchorTransform);
      _primaryPaletteMoveable._A.rotation = MirrorUtil.GetMirroredRotation(_primaryPaletteMoveable._A.rotation, anchorTransform);
      _primaryPaletteMoveable.MoveToA();
    }
  }

}
