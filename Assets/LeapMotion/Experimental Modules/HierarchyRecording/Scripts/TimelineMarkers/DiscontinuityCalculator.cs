using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Leap.Unity.Recording {

  [RequireComponent(typeof(PlayableDirector))]
  public class DiscontinuityCalculator : MonoBehaviour {
    public Action<bool> OnUpdate;

    public List<string> tracksToWatch = new List<string>();

    private PlayableDirector _director;
    private List<TimelineClip> _clipsToWatch = new List<TimelineClip>();

    private PlayableAsset _prevAsset;
    private double _prevTime;

    private void Awake() {
      _director = GetComponent<PlayableDirector>();
    }

    private void LateUpdate() {
      bool isDiscontinuity = false;

      //If we switched to a different asset
      if (_director.playableAsset != _prevAsset) {
        isDiscontinuity = true;
        recalculateClips();
      }

      //If we moved back in time
      if (_director.time < _prevTime) {
        isDiscontinuity = true;
      }

      //Or if we moved forward in time more than we should have
      if (_director.time - _prevTime > Time.deltaTime * 3) {
        isDiscontinuity = true;
      }

      //Or if we entered or left any clips that we are watching
      foreach (var clip in _clipsToWatch) {
        if (didEnterOrLeaveClip(clip)) {
          isDiscontinuity = true;
          break;
        }
      }

      if (OnUpdate != null) {
        OnUpdate(isDiscontinuity);
      }

      _prevTime = _director.time;
      _prevAsset = _director.playableAsset;
    }

    private void recalculateClips() {
      _clipsToWatch.Clear();

      var timeline = _director.playableAsset as TimelineAsset;
      if (timeline == null) {
        return;
      }

      for (int i = 0; i < timeline.outputTrackCount; i++) {
        var track = timeline.GetOutputTrack(i);
        Debug.Log(track.name);
        if (tracksToWatch.Contains(track.name)) {
          _clipsToWatch.AddRange(track.GetClips());
        }
      }
    }

    private bool didEnterOrLeaveClip(TimelineClip clip) {
      bool wasInside = _prevTime > clip.start && _prevTime < clip.end;
      bool isInside = _director.time > clip.start && _director.time < clip.end;
      return wasInside != isInside;
    }
  }
}
