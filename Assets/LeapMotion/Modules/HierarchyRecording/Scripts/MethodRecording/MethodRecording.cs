using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Recording {

  public abstract class MethodRecording : MonoBehaviour {

    public Mode mode { get; private set; }

    protected virtual void Awake() {
      HierarchyRecorder.OnBeginRecording += EnterRecordingMode;
    }

    protected virtual void OnDestroy() {
      HierarchyRecorder.OnBeginRecording -= EnterRecordingMode;
    }

    public abstract float GetDuration();

    public virtual void EnterRecordingMode() {
      mode = Mode.Recording;
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

  public abstract class BasicMethodRecording<T> : MethodRecording {

    [SerializeField]
    private List<float> _times;

    [SerializeField]
    private List<T> _args;

    public override sealed float GetDuration() {
      if (_times.Count == 0) {
        return 0;
      } else {
        return _times[_times.Count - 1];
      }
    }

    public override sealed void SweepTime(float from, float to) {
      int startIndex = Mathf.Abs(_times.BinarySearch(from));
      int endIndex = Mathf.Abs(_times.BinarySearch(to));

      for (int i = startIndex; i < endIndex; i++) {
        InvokeArgs(_args[i]);
      }
    }

    protected void SaveArgs(T state) {
      _times.Add(HierarchyRecorder.instance.recordingTime);
      _args.Add(state);
    }

    protected abstract void InvokeArgs(T state);

    [Serializable]
    private struct TimedArgs {
      public float time;
      public T state;
    }
  }
}
