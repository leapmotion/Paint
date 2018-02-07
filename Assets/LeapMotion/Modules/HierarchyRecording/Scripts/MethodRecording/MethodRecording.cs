/******************************************************************************
 * Copyright (C) Leap Motion, Inc. 2011-2018.                                 *
 * Leap Motion proprietary and  confidential.                                 *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Leap Motion and you, your company or other organization.           *
 ******************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Recording {

  public abstract class MethodRecording : MonoBehaviour {
    public Mode mode { get; private set; }

    public abstract float GetDuration();

    public abstract void SaveDataToFile(string file);
    public abstract void LoadDataFromFile();

    public virtual void EnterRecordingMode() {
      mode = Mode.Recording;
    }

    public virtual void ExitRecordingMode(string savePath) {
      mode = Mode.None;
    }

    public virtual void EnterPlaybackMode() {
      mode = Mode.Playback;
    }

    public abstract void SweepTime(float from, float to);

    public enum Mode {
      None = 0,
      Recording = 1,
      Playback = 2
    }
  }

  public abstract class MethodRecording<T> : MethodRecording where T : new() {

    [SerializeField]
    private string recordedDataPath;

    public T data { get; private set; }

    protected virtual void Awake() {
      HierarchyRecorder.OnBeginRecording += EnterRecordingMode;
    }

    protected virtual void OnDestroy() {
      HierarchyRecorder.OnBeginRecording -= EnterRecordingMode;
    }

    public override void LoadDataFromFile() {
      data = JsonUtility.FromJson<T>(File.ReadAllText(recordedDataPath));
    }

    public override void SaveDataToFile(string file) {
      recordedDataPath = file;
      File.WriteAllText(file, JsonUtility.ToJson(data));
    }

    public override void EnterRecordingMode() {
      base.EnterRecordingMode();
      data = new T();
    }

    public override void ExitRecordingMode(string savePath) {
      base.ExitRecordingMode(savePath);
      SaveDataToFile(savePath);
    }

    public override void EnterPlaybackMode() {
      base.EnterPlaybackMode();
      LoadDataFromFile();
    }
  }

  public abstract class BasicMethodRecording<T> : MethodRecording<BasicMethodRecording<T>.RecordedData> {

    public override sealed float GetDuration() {
      if (data.times.Count == 0) {
        return 0;
      } else {
        return data.times[data.times.Count - 1];
      }
    }

    public override sealed void SweepTime(float from, float to) {
      int startIndex = data.times.BinarySearch(from);
      int endIndex = data.times.BinarySearch(to);

      if (startIndex < 0) {
        startIndex = ~startIndex;
      }

      if (endIndex < 0) {
        endIndex = ~endIndex;
      }

      Debug.Log("Sweep from " + from + " (" + startIndex + ") to " + to + " (" + endIndex + ")");

      for (int i = startIndex; i < endIndex; i++) {
        InvokeArgs(data.args[i]);
      }
    }

    protected void SaveArgs(T state) {
      if (data.times == null) data.times = new List<float>();
      if (data.args == null) data.args = new List<T>();

      data.times.Add(HierarchyRecorder.instance.recordingTime);
      data.args.Add(state);
    }

    protected abstract void InvokeArgs(T state);

    [Serializable]
    public class RecordedData {
      public List<float> times;
      public List<T> args;
    }
  }
}
