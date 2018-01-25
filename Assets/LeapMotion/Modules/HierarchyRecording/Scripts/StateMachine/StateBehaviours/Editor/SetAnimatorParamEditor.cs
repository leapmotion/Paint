using UnityEngine;
using UnityEditor;

namespace Leap.Unity.Recording {

  [CanEditMultipleObjects]
  [CustomEditor(typeof(SetAnimatorParam))]
  public class SetAnimatorParamEditor : CustomEditorBase<SetAnimatorParam> {

    protected override void OnEnable() {
      base.OnEnable();

      specifyConditionalDrawing("_type", (int)AnimatorControllerParameterType.Bool, "_boolValue");
      specifyConditionalDrawing("_type", (int)AnimatorControllerParameterType.Int, "_intValue");
      specifyConditionalDrawing("_type", (int)AnimatorControllerParameterType.Float, "_floatValue");
    }
  }
}
