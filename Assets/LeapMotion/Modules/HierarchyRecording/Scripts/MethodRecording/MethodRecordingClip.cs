using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Leap.Unity.Recording {

  public class MethodRecordingClip : PlayableAsset, ITimelineClipAsset {

    public ClipCaps clipCaps {
      get {
        return ClipCaps.ClipIn | ClipCaps.SpeedMultiplier;
      }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
      return ScriptPlayable<MethodRecordingPlayable>.Create(graph);
    }
  }
}
