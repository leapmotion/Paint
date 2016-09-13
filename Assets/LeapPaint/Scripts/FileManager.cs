using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileManager : MonoBehaviour {

  public string _localSaveDir = "./Paintings/";

  public Action OnShouldRefreshFiles = () => { };

  void Start() {
    if (!Directory.Exists(_localSaveDir)) {
      Directory.CreateDirectory(_localSaveDir);
    }
  }

  public string[] GetFiles() {
    string[] files = Directory.GetFiles(_localSaveDir);
    List<string> goodFiles = new List<string>(files.Length);
    foreach (string file in files) {
      string[] splitFileName = file.Split('.');
      if (splitFileName[splitFileName.Length - 1].Equals("json")) {
        goodFiles.Add(file);
      }
    }
    return goodFiles.ToArray();
  }

  public string NameFromPath(string path) {
    return Path.GetFileName(path);
  }

  public void Save(string fileName, string fileContents) {
    using (StreamWriter writer = new StreamWriter(Path.Combine(_localSaveDir, fileName), false)) {
      writer.Write(fileContents);
    }
    
    OnShouldRefreshFiles();
  }

  public string Load(string fileName) {
    string json = "";
    using (StreamReader reader = new StreamReader(Path.Combine(_localSaveDir, fileName))) {
      json = reader.ReadToEnd();
    }
    return json;
  }

}
