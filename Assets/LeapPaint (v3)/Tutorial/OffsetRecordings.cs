using System.IO;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Leap.Unity.Recording;
using Leap.Unity.Attributes;

public class OffsetRecordings : MonoBehaviour {

  [QuickButton("Apply", "ApplyTheShift")]
  public Vector3 offset;

#if UNITY_EDITOR
  [ContextMenu("Apply The Shift")]
  void ApplyTheShift() {
    foreach (var guid in AssetDatabase.FindAssets("t:VectorHandRecording")) {
      string path = AssetDatabase.GUIDToAssetPath(guid);

      if (!path.EndsWith(".asset")) {
        continue;
      }

      var recording = AssetDatabase.LoadAssetAtPath<VectorHandRecording>(path);
      if (recording == null) {
        continue;
      }

      EditorUtility.SetDirty(recording);
      recording.AddOffset(offset);
    }
  }
#endif
}
