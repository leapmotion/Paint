using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioSourceCache : MonoBehaviour {

  [SerializeField]
  private AudioSource _template;

  private Queue<AudioSource> _pool = new Queue<AudioSource>();

  private List<KeyValuePair<AudioSource, Transform>> _active = new List<KeyValuePair<AudioSource, Transform>>();

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

  public void PlayOnTransform(AudioClip clip, AudioMixerGroup group, Transform parent, float volume, float pitch) {
    var source = getAudioSource(clip, group, volume, pitch);
    _active.Add(new KeyValuePair<AudioSource, Transform>(source, parent));
    source.Play();
  }

  public void PlayAtPosition(AudioClip clip, AudioMixerGroup group, Vector3 parent, float volume, float pitch) {
    var source = getAudioSource(clip, group, volume, pitch);
    _active.Add(new KeyValuePair<AudioSource, Transform>(source, null));
    source.gameObject.transform.position = parent;
    source.Play();
  }

  void LateUpdate() {
    for (int i = _active.Count; i-- != 0;) {
      var pair = _active[i];

      if (pair.Value != null) {
        pair.Key.transform.position = pair.Value.position;
      }

      if (!pair.Key.isPlaying) {
        _active.RemoveAt(i);
        _pool.Enqueue(pair.Key);
      }
    }
  }

  private AudioSource getAudioSource(AudioClip clip, AudioMixerGroup group, float volume, float pitch) {
    AudioSource source = null;

    if (_pool.Count > 0) {
      source = _pool.Dequeue();
    } else {
      source = Instantiate(_template);
    }

    source.clip = clip;

    if (group == null) {
      source.outputAudioMixerGroup = _template.outputAudioMixerGroup;
    } else {
      source.outputAudioMixerGroup = group;
    }

    source.volume = volume;
    source.pitch = pitch;

    return source;
  }

  private struct Pair {
    public AudioSource source;
    public Transform target;


  }
}
