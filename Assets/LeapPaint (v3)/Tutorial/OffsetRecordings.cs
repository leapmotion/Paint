using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Leap.Unity;
using Leap.Unity.Recording;

public class OffsetRecordings : MonoBehaviour {

  public Vector3 offset;

  [ContextMenu("Shift it")]
  void Tryit() {
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



}
