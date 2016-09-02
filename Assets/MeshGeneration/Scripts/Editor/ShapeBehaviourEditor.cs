using UnityEditor;
using Leap.Unity;

namespace MeshGeneration {

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