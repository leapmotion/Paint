using UnityEngine;
using System.Collections.Generic;
using System;
using Leap.zzOldPaint;
using zzOldStrokeProcessing;

public class RibbonIO : MonoBehaviour {

  public HistoryManager _historyManager;
  public FileDisplayer _fileDisplayer;
  public FileManager _fileManager;

  public PinchStrokeProcessor _replayProcessor;

  private bool _isLoading = false;
  public bool IsLoading {
    get { return _isLoading; }
  }

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
    _fileManager.Save(fileName + ".ply", PlyExporter.MakePly(_replayProcessor._ribbonParentObject));
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

  public void LoadAsync() {
    if (!_isLoading) {
      _isLoading = true;
      FlowRunner.StartNew(AsyncLoadViaDisplayerSelected());
    }
  }

  private IEnumerator<Flow> AsyncLoadViaDisplayerSelected() {
    string fileName = _fileDisplayer.GetSelectedFilename();

    yield return Flow.IntoNewThread();

    string strokesJSON = _fileManager.Load(fileName);
    Strokes strokes = JsonUtility.FromJson<Strokes>(strokesJSON);
    Debug.Log("Loaded JSON for " + strokes.strokes.Count + " strokes.");

    yield return Flow.IntoUpdate();

    _historyManager.ClearAll();

    for (int i = 0; i < strokes.strokes.Count; i++) {
      List<StrokePoint> stroke = strokes.strokes[i].strokePoints;
      _replayProcessor.ShortcircuitStrokeToRenderer(stroke);
      //yield return Flow.ForFrames(8); // one per 8 frames is a good "splash screen" speed
      yield return Flow.IfElapsed(2); // this is a good quick-load speed
    }

    _isLoading = false;
  }

}
