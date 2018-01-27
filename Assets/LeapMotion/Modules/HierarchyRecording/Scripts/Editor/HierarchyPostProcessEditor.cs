/******************************************************************************
 * Copyright (C) Leap Motion, Inc. 2011-2017.                                 *
 * Leap Motion proprietary and  confidential.                                 *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Leap Motion and you, your company or other organization.           *
 ******************************************************************************/

using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using Leap.Unity.GraphicalRenderer;

namespace Leap.Unity.Recording {

  [CustomEditor(typeof(HierarchyPostProcess))]
  public class HierarchyPostProcessEditor : CustomEditorBase<HierarchyPostProcess> {

    private bool _expandComponentTypes = false;

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      bool isPrefab = PrefabUtility.GetPrefabType(target) == PrefabType.Prefab;
      EditorGUI.BeginDisabledGroup(isPrefab);

      if (GUILayout.Button("Clear")) {
        target.ClearComponents();
      }

      if (GUILayout.Button(new GUIContent("Build Playback Prefab",
                                          isPrefab ? "Draw this object into the scene "
                                                   + "before converting its raw recording "
                                                   + "data into AnimationClip data."
                                                   : ""))) {
        target.BuildPlaybackPrefab(new ProgressBar());
      }

      EditorGUI.EndDisabledGroup();

      _expandComponentTypes = EditorGUILayout.Foldout(_expandComponentTypes, "Component List");
      if (_expandComponentTypes) {
        EditorGUI.indentLevel++;

        var components = target.GetComponentsInChildren<Component>(includeInactive: true).
                                Select(c => c.GetType()).
                                Distinct().
                                OrderBy(m => m.Name);

        var groups = components.GroupBy(t => t.Namespace).OrderBy(g => g.Key);
        foreach(var group in groups) {
          EditorGUILayout.Space();
          EditorGUILayout.LabelField(group.Key ?? "Dev", EditorStyles.boldLabel);
          foreach (var type in group) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(type.Name);

            if (GUILayout.Button("Delete")) {
              var toDelete = target.GetComponentsInChildren(type, includeInactive: true);
              foreach (var t in toDelete) {
                Undo.DestroyObjectImmediate(t);
              }
            }

            EditorGUILayout.EndHorizontal();
          }
        }
      }
    }
  }
}
