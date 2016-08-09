using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class FileBrowser : MonoBehaviour {

  #region PUBLIC FIELDS

  [Tooltip("The LayoutGroup responsible for laying out each text element (one per file)")]
  public LayoutGroup _fileListLayout;

  [Tooltip("The prefab that will be used to instantiate text elements. Its text will be set to the filename, but the may prefab use things like LayoutElement components to override layout settings.")]
  public Text _fileListTextPrefab;

  [Tooltip("The prefab image to overlay on top of the currently selected text element. The only logical operation the FileBrowser performs is changing the parent of the selection Image to the currently selected text element; appropriate layout behavior can be specified by the parent text element prefab using HorizontalLayoutGroup or similar components.")]
  public Image _fileSelectionPrefab;

  #endregion

  #region PRIVATE FIELDS

  private string[] _files;
  private Text[] _childrenTextObjs = new Text[0];
  private Image _fileSelectionObj;
  private FileSelectionClaimer _selected;

  private const string _saveDir = "Drawing Saves";

  #endregion

  #region UNITY EVENTS

  [System.Serializable]
  public class FilePathEvent : UnityEvent<string> { }
  public FilePathEvent OnSaveRequested;
  public FilePathEvent OnLoadRequested;

  #endregion

  #region UNITY CALLBACKS

  protected void Start() {
    if (!Directory.Exists("./" + _saveDir)) {
      Directory.CreateDirectory("./" + _saveDir);
    }
    RefreshFileList();
  }

#endregion

  #region PRIVATE METHODS

  private void RefreshFileList() {

    // Remove old text objects
    ClearChildrenTextObjs();

    // Create text objects per file
    _files =  Directory.GetFiles("./" + _saveDir + "/");
    Debug.Log("[FileBrowser] " + _files.Length + " files found");
    _childrenTextObjs = new Text[_files.Length];
    for (int i = 0; i < _files.Length; i++) {
      var textObj = Instantiate<Text>(_fileListTextPrefab);
      textObj.transform.parent = _fileListLayout.transform;
      textObj.transform.localScale = Vector3.one;
      textObj.rectTransform.localPosition = new Vector3(textObj.rectTransform.localPosition.x, textObj.rectTransform.localPosition.y, 0F);
      textObj.transform.localRotation = Quaternion.identity;
      textObj.text = Path.GetFileName(_files[i]);
      textObj.gameObject.AddComponent<FileSelectionClaimer>()._toClaimSelectionFrom = this;
      _childrenTextObjs[i] = textObj;
    }

    // Selection
    if (_files.Length > 0) {
      if (_fileSelectionObj == null) {
        _fileSelectionObj = Instantiate(_fileSelectionPrefab);
      }
      _fileSelectionObj.transform.parent = _childrenTextObjs[0].transform;
      _fileSelectionObj.transform.localScale = Vector3.one;
      _fileSelectionObj.GetComponent<RectTransform>().localPosition = new Vector3(_fileSelectionObj.GetComponent<RectTransform>().localPosition.x, _fileSelectionObj.GetComponent<RectTransform>().localPosition.y, 0F);
      _fileSelectionObj.GetComponent<Image>().enabled = true;

      _selected = _childrenTextObjs[0].GetComponent<FileSelectionClaimer>();
    }
    else {
      Destroy(_fileSelectionObj);
    }
  }

  private void ClearChildrenTextObjs() {
    for (int i = 0; i < _childrenTextObjs.Length; i++) {
      Destroy(_childrenTextObjs[i].gameObject);
    }
    _childrenTextObjs = new Text[0];
  }

  private int GetSelectedIdx() {
    var textObjToFind = _selected.GetComponent<Text>();
    if (textObjToFind != null) {
      for (int i = 0; i < _childrenTextObjs.Length; i++) {
        if (textObjToFind == _childrenTextObjs[i]) {
          return i;
        }
      }
    }
    return -1;
  }

  #endregion

  #region PUBLIC METHODS

  public void Refresh() {
    RefreshFileList();
  }

  public void Save() {
    string filename = System.DateTime.Now.ToString("yyyy'-'MM'-'dd'_'HH'-'mm'-'ss") + ".txt";
    OnSaveRequested.Invoke("./" + _saveDir + "/" + filename);
  }

  public void Load() {
    if (_selected != null) {
      string filename = _childrenTextObjs[GetSelectedIdx()].text;
      OnLoadRequested.Invoke("./" + _saveDir + "/" + filename);
    }
    else {
      Debug.LogWarning("[FileBrowser] Load requested but selection is null.");
    }
  }

  public void Select(FileSelectionClaimer toSelect) {
    _fileSelectionObj.transform.parent = toSelect.transform;
    _selected = toSelect;
  }

  #endregion

}
