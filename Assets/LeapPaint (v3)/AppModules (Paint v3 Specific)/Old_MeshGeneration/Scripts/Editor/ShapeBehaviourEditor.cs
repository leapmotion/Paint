using UnityEditor;
using Leap.Unity;

namespace zzOld_MeshGeneration_LeapPaint_v3 {

  [CustomEditor(typeof(ShapeBehaviour), editorForChildClasses: true)]
  public class ShapeBehaviourEditor : CustomEditorBase {

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      if (_modifiedProperties.Count != 0) {
        (target as ShapeBehaviour).GenerateMesh();
      }
    }

  }
}