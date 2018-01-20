using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Leap.Unity.Recording {

  public class SetPlayableDirector : MonoBehaviour {

    [SerializeField]
    private PlayableDirector _director;

    [SerializeField]
    private PlayableAsset _playable;

    [SerializeField]
    private DirectorWrapMode _wrapMode = DirectorWrapMode.None;

    private void OnEnable() {
      _director.extrapolationMode = _wrapMode;
      _director.Play(_playable);
    }
  }
}
