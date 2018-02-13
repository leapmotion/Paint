using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Reflection;
using Leap.Unity.Query;
using System.Collections.Generic;

namespace Leap.Unity.UserContext {
  
  class UconGraphWindow : EditorWindow {

    [MenuItem("Window/Leap Motion/Ucon Graph")]
    public static void ShowWindow() {
      var uconWindow = GetWindow(typeof(UconGraphWindow));
      uconWindow.titleContent = new GUIContent("Ucon Graph");
    }

    void OnGUI() {
      drawUconTypes();
    }

    private void drawUconTypes() {

      foreach (var analyzedUconType in UconAnalysis.uconChannelTypes) {
        GUILayout.Box(new GUIContent(analyzedUconType.type.Name), EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical();
        foreach (var uconChannelField in analyzedUconType.channelFields) {
          EditorGUILayout.LabelField(new GUIContent(uconChannelField.Name));
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
      }

    }

  }

}