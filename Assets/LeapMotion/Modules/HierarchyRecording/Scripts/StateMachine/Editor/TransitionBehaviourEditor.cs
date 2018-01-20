using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Leap.Unity.Recording {

  [CanEditMultipleObjects]
  [CustomEditor(typeof(TransitionBehaviour), editorForChildClasses: true, isFallback = true)]
  public class TransitionBehaviourEditor : CustomEditorBase<TransitionBehaviour> {

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      if (targets.Length == 1 && Application.isPlaying) {
        if (GUILayout.Button("Execute Transition")) {
          target.Transition();
        }
      }
    }
  }
}
