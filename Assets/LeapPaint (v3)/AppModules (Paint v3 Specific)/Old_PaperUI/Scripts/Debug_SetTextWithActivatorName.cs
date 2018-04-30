using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LeapPaint_v3 {

  public class Debug_SetTextWithActivatorName : MonoBehaviour {

    public PressableUI pressableUI;
    public TextMesh textMesh;

    private void Reset() {
      if (pressableUI == null) pressableUI = GetComponentInParent<PressableUI>();
      if (textMesh == null) textMesh = GetComponent<TextMesh>();
    }

    private void Update() {
      if (pressableUI != null && textMesh != null) {
        var activator = pressableUI.activator;
        textMesh.text = (activator == null ? "<null>" : activator.name);
      }
    }

  }

}
