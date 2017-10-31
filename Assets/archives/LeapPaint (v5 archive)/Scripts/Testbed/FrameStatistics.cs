using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity.Profiling {

  public class FrameStatistics : MonoBehaviour {

    [SerializeField, Disable]
    private float _frameTime = 0F;
    public float FrameTime { get { return _frameTime; } }

    [SerializeField, Disable]
    private float _fps = 0F;
    public float FPS { get { return _fps; } }

    private System.Diagnostics.Stopwatch _frameStopwatch = new System.Diagnostics.Stopwatch();

    void Update() {
      updateFrameTime();


    }

    private void updateFrameTime() {
      if (!_frameStopwatch.IsRunning) {
        // Initialization frame.
        _frameTime = 0F;
        _fps = 0F;
      }
      else {
        _frameStopwatch.Stop();

        _frameTime = (float)(_frameStopwatch.ElapsedTicks / (double)System.TimeSpan.TicksPerMillisecond) / 1000F;
        _fps = 1F / _frameTime;

        _frameStopwatch.Reset();
      }

      _frameStopwatch.Start();
    }

  }
  
}