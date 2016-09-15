using UnityEngine;
using System.Collections.Generic;
using System;

public class RibbonIO : MonoBehaviour {

  public HistoryManager _historyManager;
  public FileDisplayer _fileDisplayer;
  public FileManager _fileManager;

  public PinchStrokeProcessor _replayProcessor;

  public Action<string> OnSaveSuccessful = (x) => { };

  private string GetHistoryAsJSON() {
    Strokes strokes = new Strokes(_historyManager.GetStrokes());
    Debug.Log("Created JSON for " + strokes.strokes.Count + " strokes.");
    return JsonUtility.ToJson(strokes);
  }

  public void Save() {
    string fileName = "My Painting " + DateTime.Now.ToString("MM-dd_HH-mm-ss");
    string json = GetHistoryAsJSON();
    _fileManager.Save(fileName + ".json", json);
    //_fileManager.Save(fileName + ".obj", ObjExporter.makeObj(true, "LeapPaintMesh", _replayProcessor._ribbonParentObject).ToString());
    OnSaveSuccessful.Invoke(fileName);
  }

  public void LoadViaDisplayerSelected() {
    string fileName = _fileDisplayer.GetSelectedFilename();
    string strokesJSON = _fileManager.Load(fileName);
    Strokes strokes = JsonUtility.FromJson<Strokes>(strokesJSON);
    Debug.Log("Loaded JSON for " + strokes.strokes.Count + " strokes.");

    _historyManager.ClearAll();

    for (int i = 0; i < strokes.strokes.Count; i++) {
      List<StrokePoint> stroke = strokes.strokes[i].strokePoints;
      _replayProcessor.ShortcircuitStrokeToRenderer(stroke);
    }
  }

}
