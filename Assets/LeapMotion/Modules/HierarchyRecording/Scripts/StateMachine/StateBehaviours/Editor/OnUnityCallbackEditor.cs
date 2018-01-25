using UnityEditor;

namespace Leap.Unity.Recording {

  [CustomEditor(typeof(OnUnityCallback))]
  public class OnUnityCallbackEditor : CustomEditorBase<OnUnityCallback> {

    private SerializedProperty _tableProp;
    private EnumEventTableEditor _tableEditor;

    protected override void OnEnable() {
      base.OnEnable();

      _tableProp = serializedObject.FindProperty("_table");
      _tableEditor = new EnumEventTableEditor(_tableProp, typeof(OnUnityCallback.CallbackType));

      specifyCustomDrawer("_table", drawTable);
    }

    private void drawTable(SerializedProperty p) {
      _tableEditor.DoGuiLayout();
    }
  }
}
