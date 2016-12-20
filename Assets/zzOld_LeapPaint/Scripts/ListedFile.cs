using UnityEngine;
using System.Collections;

public class ListedFile : MonoBehaviour {

  public FileDisplayer _displayer;

  public string FileName { get; set; }
  public string FilePath { get; set; }
  public int ListIndex { get; set; }

  public void DoOnPointerDown() {
    _displayer.NotifyPointerDown(this);
  }

}
