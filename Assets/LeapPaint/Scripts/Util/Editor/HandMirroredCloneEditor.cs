using UnityEngine;
using System.Collections;
using Leap.Unity;
using UnityEditor;

[CustomEditor(typeof(HandMirroredClone))]
public class HandMirroredCloneEditor : CustomEditorBase {

  public override void OnInspectorGUI() {
    base.OnInspectorGUI();

    HandMirroredClone handMirroredClone = (HandMirroredClone)target;
    if (GUILayout.Button("Create Mirrored Clone")) {
      handMirroredClone.MirrorCloneUsingYZPlane();
    }
  }

}
