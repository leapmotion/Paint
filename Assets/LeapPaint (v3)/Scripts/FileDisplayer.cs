using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Leap.Unity.LeapPaint_v3 {

  public class FileDisplayer : MonoBehaviour {

    public FileManager _fileManager;
    public RectTransform _textParent;
    public Text _textPrefab;
    public RectTransform _selectorPrefab;
    public Text noFilesFoundText;

    private string[] _files;
    private List<Text> _fileTexts = new List<Text>();
    private List<ListedFile> _listedFiles = new List<ListedFile>();
    private int _selectedFileIdx = 0;
    private RectTransform _selector = null;

    void Start() {
      _textPrefab.rectTransform.pivot = new Vector2(0F, 1F);

      _fileManager.OnShouldRefreshFiles += RefreshFiles;
      RefreshFiles();
    }

    void OnEnable() {
      for (int i = 0; i < _fileTexts.Count; i++) {
        _fileTexts[i].enabled = true;
      }
    }

    void OnDisable() {
      for (int i = 0; i < _fileTexts.Count; i++) {
        _fileTexts[i].enabled = false;
      }
    }

    public void RefreshFiles() {
      _files = _fileManager.GetFiles();
      while (_fileTexts.Count > _files.Length) {
        GameObject toDestroy = _fileTexts[_fileTexts.Count - 1].gameObject;
        _fileTexts.RemoveAt(_fileTexts.Count - 1);
        _listedFiles.RemoveAt(_listedFiles.Count - 1);
        Destroy(toDestroy);
      }

      if (_files.Length == 0) {
        if (_selector != null) {
          Destroy(_selector.gameObject);
        }

        if (noFilesFoundText != null) {
          noFilesFoundText.gameObject.SetActive(true);
        }
      }
      else {
        if (_selector == null) {
          _selector = Instantiate(_selectorPrefab, _textParent.transform);
          _selector.localRotation = Quaternion.identity;
          _selector.localScale = Vector3.one;
          _selector.sizeDelta = new Vector2(_textParent.rect.width,
            _textPrefab.rectTransform.rect.height);
        }

        if (noFilesFoundText != null) {
          noFilesFoundText.gameObject.SetActive(false);
        }
      }

      _textParent.sizeDelta = new Vector2(_textParent.rect.width,
        _textPrefab.rectTransform.rect.height * _files.Length);
      float heightFromTop = 0;
      for (int i = 0; i < _files.Length; i++) {
        if (_fileTexts.Count < _files.Length) {
          Text newText = (Text)Instantiate(_textPrefab, _textParent.transform);
          newText.rectTransform.localRotation = Quaternion.identity;
          newText.rectTransform.localScale = Vector3.one;
          newText.rectTransform.sizeDelta = new Vector2(_textParent.rect.width,
            newText.rectTransform.rect.height);
          _fileTexts.Add(newText);

          ListedFile listedFile = newText.gameObject.GetComponent<ListedFile>();
          listedFile._displayer = this;
          _listedFiles.Add(listedFile);
        }
        string filename = _fileManager.NameFromPath(_files[i]);
        _fileTexts[i].text = filename;
        _fileTexts[i].rectTransform.localPosition = new Vector3(
          -_textParent.rect.width / 2F,
          _textParent.rect.height / 2F - heightFromTop, 0F);
      
        _listedFiles[i].FileName = filename;
        _listedFiles[i].FilePath = _files[i];
        _listedFiles[i].ListIndex = i;
        heightFromTop += _fileTexts[i].rectTransform.rect.height;
      }

      refreshSelected();
    }

    private void refreshSelected() {
      if (_selector != null) {
        _selector.localPosition = new Vector3(-_textParent.rect.width / 2F,
          _textParent.rect.height / 2F
            - (_textPrefab.rectTransform.rect.height * _selectedFileIdx),
          10F);
      }
    }

    public void NotifyPointerDown(ListedFile listedFile) {
      _selectedFileIdx = listedFile.ListIndex;
      refreshSelected();
    }

    public string GetSelectedFilename() {
      if (_files.Length == 0) {
        return "";
      }
      else {
        return _listedFiles[_selectedFileIdx].FileName;
      }
    }

  }


}