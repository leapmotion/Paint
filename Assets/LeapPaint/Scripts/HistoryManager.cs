using UnityEngine;
using System.Collections.Generic;

public class HistoryManager : MonoBehaviour {

  public GameObject ribbonObjectParent;
  public GameObject undoneObjectParent;

  private List<GameObject> _undoHistory = new List<GameObject>();
  private List<GameObject> _redoHistory = new List<GameObject>();

  private List<Stroke> _strokes = new List<Stroke>();
  private List<Stroke> _undoneStrokes = new List<Stroke>();

  public void NotifyStroke(GameObject strokeResult, List<StrokePoint> strokePoints) {
    _undoHistory.Add(strokeResult);
    Stroke stroke = new Stroke();
    stroke.strokePoints = strokePoints;
    _strokes.Add(stroke);
    ClearRedoHistory();
  }

  public void Undo() {
    if (_undoHistory.Count != 0) {
      GameObject undoneAction = _undoHistory[_undoHistory.Count - 1];
      _redoHistory.Add(undoneAction);
      _undoHistory.RemoveAt(_undoHistory.Count - 1);
      undoneAction.SetActive(false);
      undoneAction.transform.parent = undoneObjectParent.transform;

      Stroke undoneStroke = _strokes[_strokes.Count - 1];
      _undoneStrokes.Add(undoneStroke);
      _strokes.RemoveAt(_strokes.Count - 1);
    }
  }

  public void Redo() {
    if (_redoHistory.Count != 0) {
      GameObject redoneAction = _redoHistory[_redoHistory.Count - 1];
      _undoHistory.Add(redoneAction);
      _redoHistory.RemoveAt(_redoHistory.Count - 1);
      redoneAction.SetActive(true);
      redoneAction.transform.parent = ribbonObjectParent.transform;

      Stroke redoneStroke = _undoneStrokes[_undoneStrokes.Count - 1];
      _strokes.Add(redoneStroke);
      _undoneStrokes.RemoveAt(_undoneStrokes.Count - 1);
    }
  }

  public void ClearAll() {
    int numUndos = _undoHistory.Count;
    for (int i = 0; i < numUndos; i++) {
      Undo();
    }
  }

  private void ClearRedoHistory() {
    for (int i = 0; i < _redoHistory.Count; i++) {
      Destroy(_redoHistory[i]);
    }
    _redoHistory.Clear();
    _undoneStrokes.Clear();
  }

  public List<Stroke> GetStrokes() {
    return _strokes;
  }

}
