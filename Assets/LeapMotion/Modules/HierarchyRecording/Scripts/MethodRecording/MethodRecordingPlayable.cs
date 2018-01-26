using UnityEngine.Playables;

namespace Leap.Unity.Recording {

  public class MethodRecordingPlayable : PlayableBehaviour {
    private double _prevTime = double.NaN;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
      var recording = playerData as MethodRecording;
      if (recording == null) {
        return;
      }

      if (recording.mode != MethodRecording.Mode.Playback) {
        recording.EnterPlaybackMode();
      }

      float prevTime = (float)playable.GetPreviousTime();
      float nowTime = (float)playable.GetTime();
      bool didSeek = _prevTime != playable.GetPreviousTime() || nowTime < prevTime;

      if (!didSeek) {
        recording.SweepTime(prevTime, nowTime);
      }

      _prevTime = playable.GetTime();
    }
  }
}
