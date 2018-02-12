using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Leap.Unity.UserContext {

  class UconEditorWindow : EditorWindow {

    [MenuItem("Window/Leap Motion/Ucon Editor Window")]
    public static void ShowWindow() {
      var uconWindow = GetWindow(typeof(UconEditorWindow));
      uconWindow.titleContent = new GUIContent("Ucon Graph");
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded() {
      // Find all serializable
    }

    void OnGUI() {

    }

  }

}