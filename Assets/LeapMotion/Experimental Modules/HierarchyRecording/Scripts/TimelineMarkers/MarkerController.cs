using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Leap.Unity.Recording {

  [RequireComponent(typeof(PlayableDirector))]
  public class MarkerController : MonoBehaviour {

    private PlayableDirector _director;
    private List<MarkerClip> _markers = new List<MarkerClip>();

    private TimelineClip _currMarker;
    private Action _action;

    private void Awake() {
      _director = GetComponent<PlayableDirector>();
    }

    public void LoopAt(string markerName) {
      setCurrentMarker(markerName);
      _action = Action.Loop;
    }

    public void PauseAt(string markerName) {
      setCurrentMarker(markerName);
      _action = Action.Pause;
    }

    public void Resume() {
      //Undo current action
      switch (_action) {
        case Action.Pause:
          _director.Resume();
          break;
      }

      //Set current action to none
      _action = Action.None;
    }

    private void Update() {
      switch (_action) {
        case Action.Pause:
          if (_director.time >= _currMarker.start) {
            _director.Pause();
          }
          break;
        case Action.Loop:
          if (_director.time >= _currMarker.end) {
            _director.time = _currMarker.start;
          }
          break;
      }
    }

    private void setCurrentMarker(string name) {
      var timeline = _director.playableAsset as TimelineAsset;
      for (int i = 0; i < timeline.outputTrackCount; i++) {
        var track = timeline.GetOutputTrack(i);
        if (track is MarkerTrack) {
          foreach (var clip in track.GetClips()) {
            var marker = clip.asset as MarkerClip;
            if (marker.markerName == name) {
              _currMarker = clip;
              return;
            }
          }
        }
      }
    }

    private enum Action {
      None,
      Pause,
      Loop
    }
  }
}
