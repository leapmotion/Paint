using Leap.Unity.Query;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  [CustomEditor(typeof(HandledObject), editorForChildClasses: true)]
  public class HandledObjectEditor : CustomEditorBase<HandledObject> {

    protected override void OnEnable() {
      base.OnEnable();
    }

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      EditorGUILayout.LabelField("Attached Handles", EditorStyles.boldLabel);
      foreach (var handleBehaviour in target.attachedHandles
                                            .Query()
                                            .Select(h => h as MonoBehaviour)
                                            .Where(b => b != null)) {
        EditorGUILayout.LabelField(handleBehaviour.name);
      }
    }

  }

}