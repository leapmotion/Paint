using UnityEngine;
using UnityEngine.Audio;
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

  public AudioSource GetAudioSource(AudioMixerGroup group = null) {
    AudioSource source = null;

    for (int i = 0; i < _pool.Count; i++) {
      if (!_pool[i].isPlaying) {
        source = _pool[i];
        break;
      }
    }

    if (source == null) {
      source = Instantiate(_template);
      _pool.Add(source);
    }

    if (group == null) {
      source.outputAudioMixerGroup = _template.outputAudioMixerGroup;
    } else {
      source.outputAudioMixerGroup = group;
    }

    return source;
  }
}
