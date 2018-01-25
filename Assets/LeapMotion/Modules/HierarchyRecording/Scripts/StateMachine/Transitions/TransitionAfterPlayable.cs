using UnityEngine;
using UnityEngine.Playables;

namespace Leap.Unity.Recording {
  
  public class TransitionAfterPlayable : TransitionBehaviour {

    [SerializeField]
    private PlayableDirector _director;

    private bool _hasStartedPlaying = false;

    private void OnEnable() {
      _hasStartedPlaying = false;
    }

    private void Update() {
      if (_hasStartedPlaying) {
        if (_director.state != PlayState.Playing) {
          Transition();
        }
      } else if (_director.state == PlayState.Playing) {
        _hasStartedPlaying = true;
      }
    }
  }
}
