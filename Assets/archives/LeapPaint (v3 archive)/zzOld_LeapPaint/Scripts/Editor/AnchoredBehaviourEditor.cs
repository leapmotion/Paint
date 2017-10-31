using UnityEngine;
using System.Collections;
using Leap.Unity;
using UnityEditor;

namespace Leap.Unity.LeapPaint_v3 {


  [CustomEditor(typeof(AnchoredBehaviour))]
  public class AnchoredBehaviourEditor : CustomEditorBase {

    protected override void OnEnable() {
      base.OnEnable();

      specifyConditionalDrawing("_shouldAnchorRotation", "_alignToAnchorRotation");
    }

  }


}
