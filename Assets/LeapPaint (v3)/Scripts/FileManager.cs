using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Leap.Unity.LeapPaint_v3 {

  public class FileManager : MonoBehaviour {

    public string localSaveDir = "./Paintings/";
    public string fileNamePrefix = "Paint - ";

    public string paintingsDirTextPrefix = "";
    public Text paintingsDirText;

    public Action OnShouldRefreshFiles = () => { };

    void Start() {
      if (!Directory.Exists(localSaveDir)) {
        Directory.CreateDirectory(localSaveDir);
      }

      if (paintingsDirText != null) {
        paintingsDirText.text = paintingsDirTextPrefix + Path.GetFullPath(localSaveDir);
      }
    }

    public string[] GetFiles() {
      string[] files = Directory.GetFiles(localSaveDir);
      List<string> jsonFiles = new List<string>(files.Length);
      for (int i = 0; i < files.Length; i++) {
        if (Path.GetExtension(files[i]).Equals(".json")) {
          jsonFiles.Add(files[i]);
        }
      }
      return jsonFiles.ToArray();
    }

    public string NameFromPath(string filepath) {
      return Path.GetFileName(filepath);
    }

    public DateTime CreationDateFromPath(string path) {
      return Directory.GetCreationTime(path);
    }
    
    /// <summary>
    /// Tries to get the number directly after the prefix IF the string has the argument
    /// prefix, or returns 0 if there is no such number.
    /// </summary>
    public int TryGetNumberFromName(string name, string prefix = "Paint - ") {
      int result = 0;
      if (name.StartsWith(prefix)) {
        string fileName = Path.GetFileNameWithoutExtension(name);
        int.TryParse(fileName.Substring(prefix.Length), out result);
      }
      return result;
    }

    public void Save(string fileName, string fileContents) {
      using (StreamWriter writer = new StreamWriter(Path.Combine(localSaveDir, fileName), false)) {
        writer.Write(fileContents);
      }
    
      OnShouldRefreshFiles();
    }

    public string Load(string fileName) {
      string json = "";
      using (StreamReader reader = new StreamReader(Path.Combine(localSaveDir, fileName))) {
        json = reader.ReadToEnd();
      }
      return json;
    }

  }


}