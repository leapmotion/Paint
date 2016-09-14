using UnityEngine;
using System.Collections.Generic;

public class AudioSourceCache : MonoBehaviour {

  [SerializeField]
  private AudioSource _template;

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

    var obj = Instantiate(_template);
    _pool.Add(obj);
    return obj;
  }
}
