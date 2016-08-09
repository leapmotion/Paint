using UnityEngine;
using System.Collections;

[System.Serializable]
public class SavedScene {

  public TubeStroke[] _tubeStrokes;

  /// <summary> Writes this SavedScene object to a JSON string. </summary>
  public string WriteToJSON() {
    return JsonUtility.ToJson(this);
  }

  /// <summary> Loads a SavedScene object from a valid JSON string. </summary>
  public static SavedScene CreateFromJSON(string jsonString) {
    return JsonUtility.FromJson<SavedScene>(jsonString);
  }

}
