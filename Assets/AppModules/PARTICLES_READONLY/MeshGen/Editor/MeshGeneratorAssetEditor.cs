using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Leap.Unity.Query;

namespace Leap.Unity.MeshGen {

  [CustomEditor(typeof(MeshGeneratorAsset))]
  public class MeshGeneratorAssetEditor : CustomEditorBase<MeshGeneratorAsset> {

    private List<Type> _generatorTypes = new List<Type>();

    private Editor _meshEditor;

    /// <summary>
    /// This property does a lot of asset management work under the hood.
    /// 
    /// Its getter always returns the index of the serialized generator type, or -1 if
    /// there is no serialized generator type. In other words, this getter remains
    /// correct if the user creates a new generator type.
    /// 
    /// Its setter will construct and assign a new generator type if the index provided
    /// corresponds to a different valid generator type than the one currently assigned.
    /// It will also destroy the old generator if there was one attached before.
    /// </summary>
    private int _curGeneratorTypeIdx {
      get {
        // Find and return the index of the currently-loaded generator.
        var generator = serializedObject.FindProperty("_meshGenerator")
                                        .objectReferenceValue;
        if (generator == null) {
          return -1;
        }
        else {
          string fullName = generator.GetType().FullName;
          return _generatorTypes.FindIndex(t => t.FullName.Equals(fullName));
        }
      }
      set {
        // Only do something if the new index points to a valid generator type, and i
        // that type isn't the currently-selected generator type.
        if (value >= 0 && value < _generatorTypes.Count && value != _curGeneratorTypeIdx) {

          var serializedGenerator = serializedObject.FindProperty("_meshGenerator");
          var generatorObj = serializedGenerator.objectReferenceValue;
          MeshGenerator generator = generatorObj as MeshGenerator;

          // Delete the old generator if it is non-null.
          if (generator != null) {
            DestroyImmediate(generatorObj, true);
          }
          
          // Create and assign the new generator.
          var generatorType = _generatorTypes[value];
          generator = (MeshGenerator)CreateInstance(generatorType);
          generator.name = generatorType.Name;
          generator.hideFlags = HideFlags.HideInHierarchy;
          AssetDatabase.AddObjectToAsset(generator, target);
          serializedGenerator.objectReferenceValue = generator;

          // Refresh.
          AssetDatabase.SaveAssets();
        }
      }
    }

    protected override void OnEnable() {
      base.OnEnable();

      refreshAvailableGenerators();

      var serializedMesh = serializedObject.FindProperty("_mesh");
      if (serializedMesh.objectReferenceValue != null) {
        _meshEditor = CreateEditor(serializedMesh.objectReferenceValue);
      }
    }

    public override void OnInspectorGUI() {
      base.OnInspectorGUI();

      // _curGeneratorTypeIdx handles generator indexing (getter) and creation (setter).
      int selectedGeneratorIdx = EditorGUILayout.Popup(new GUIContent("Generator Type",
                                                         "Choose a mesh generator type."),
                                                       _curGeneratorTypeIdx,
                                                       getGeneratorTypeListGUIContent());
      _curGeneratorTypeIdx = selectedGeneratorIdx;

      EditorGUILayout.Space();

      // Draw the inspector for the current mesh generator.
      var serializedGenerator = serializedObject.FindProperty("_meshGenerator");
      if (serializedGenerator.objectReferenceValue != null) {
        var generatorEditor = Editor.CreateEditor(serializedGenerator.objectReferenceValue);
        generatorEditor.OnInspectorGUI();
      }

      // Refresh the mesh representation for the target objects.
      foreach (var target in targets) {
        target.RefreshMesh();
      }
    }

    public override bool HasPreviewGUI() {
      if (_meshEditor != null) {
        return _meshEditor.HasPreviewGUI();
      }

      return false;
    }

    public override GUIContent GetPreviewTitle() {
      if (_meshEditor == null) {
        return base.GetPreviewTitle();
      }

      return _meshEditor.GetPreviewTitle();
    }

    public override void DrawPreview(Rect previewArea) {
      base.DrawPreview(previewArea);

      if (_meshEditor != null) {
        _meshEditor.DrawPreview(previewArea);
      }
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background) {
      base.OnPreviewGUI(r, background);

      if (_meshEditor != null) {
        _meshEditor.OnPreviewGUI(r, background);
      }
    }

    #region Refreshing Available Generators

    private void refreshAvailableGenerators() {
      _generatorTypes.Clear();

      var baseGeneratorType = typeof(MeshGenerator);
      foreach (var types in AppDomain.CurrentDomain.GetAssemblies()
                                     .Query()
                                     .Select(s => s.GetTypes())) {
        foreach (var type in types.Query()
                                  .Where(p => baseGeneratorType.IsAssignableFrom(p)
                                         && !p.IsAbstract)) {
          _generatorTypes.Add(type);
        }
      }
    }

    private GUIContent[] getGeneratorTypeListGUIContent() {
      var contents = new GUIContent[_generatorTypes.Count];

      for (int i = 0; i < contents.Length; i++) {
        contents[i] = getGUIContentForGeneratorType(_generatorTypes[i]);
      }

      return contents;
    }

    private GUIContent getGUIContentForGeneratorType(Type generatorType) {
      return new GUIContent(generatorType.Name);
    }

    #endregion

  }

}