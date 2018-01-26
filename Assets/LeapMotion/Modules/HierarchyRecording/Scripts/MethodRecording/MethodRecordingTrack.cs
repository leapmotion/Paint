using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Leap.Unity.Recording {

  [TrackColor(0.1f, 0.1f, 1.0f)]
  [TrackClipType(typeof(MethodRecordingClip))]
  [TrackBindingType(typeof(MethodRecording))]
  public class MethodRecordingTrack : TrackAsset { }
}
