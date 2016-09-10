using UnityEngine;
using System.Collections.Generic;

public class UndoRedoManager : MonoBehaviour {

  private List<GameObject> undoHistory = new List<GameObject>();
  private List<GameObject> redoHistory = new List<GameObject>();

  public void NotifyAction(GameObject action) {
    undoHistory.Add(action);
    ClearRedoHistory();
  }

  public void Undo() {
    Debug.Log("undo called");
    if (undoHistory.Count != 0) {
      GameObject undoneAction = undoHistory[undoHistory.Count - 1];
      redoHistory.Add(undoneAction);
      undoHistory.RemoveAt(undoHistory.Count - 1);
      undoneAction.SetActive(false);
    }
  }

  public void Redo() {
    if (redoHistory.Count != 0) {
      GameObject redoneAction = redoHistory[redoHistory.Count - 1];
      undoHistory.Add(redoneAction);
      redoHistory.RemoveAt(redoHistory.Count - 1);
      redoneAction.SetActive(true);
    }
  }

  private void ClearRedoHistory() {
    for (int i = 0; i < redoHistory.Count; i++) {
      Destroy(redoHistory[i]);
    }
    redoHistory.Clear();
  }

}
