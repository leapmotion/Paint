using UnityEngine;
using System.Collections.Generic;

public class AudioSourceCache : MonoBehaviour {

  private List<AudioSource> _pool = new List<AudioSource>();

  private static AudioSourceCache _cachedInstance = null;
  public static AudioSourceCache instance {
    get {
      if (_cachedInstance == null) {
        _cachedInstance = FindObjectOfType<AudioSourceCache>();
        if (_cachedInstance == null) {
          _cachedInstance = new GameObject("__AudioSourceCache__").AddComponent<AudioSourceCache>();
        }
      }
      return _cachedInstance;
    }
  }

  public AudioSource GetAudioSource() {
    for (int i = 0; i < _pool.Count; i++) {
      if (!_pool[i].isPlaying) {
        return _pool[i];
      }
    }

    GameObject audioSourceObj = new GameObject();
    audioSourceObj.transform.parent = transform;

    var source = audioSourceObj.AddComponent<AudioSource>();
    source.spatialBlend = 1;
    source.playOnAwake = false;
    source.loop = false;
    _pool.Add(source);
    return source;
  }
}
