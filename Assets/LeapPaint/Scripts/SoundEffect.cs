using UnityEngine;
using System;
using Leap.Unity.Attributes;

[Serializable]
public class SoundEffect {

  [SerializeField]
  private AudioClip[] _clips;

  [Range(0, 1)]
  [SerializeField]
  private float _volume = 1;

  [MinValue(0)]
  [SerializeField]
  private float _pitchVariance = 0;

  [SerializeField]
  private float _pitchCenter = 1;

  public void PlayAtPosition(Vector3 position, float volumeScale = 1) {
    if (_clips.Length == 0) return;

    var source = prepAudioSource(volumeScale);
    source.transform.position = position;
  }

  public void PlayOnTransform(Transform transform, float volumeScale = 1) {
    if (_clips.Length == 0) return;

    var source = prepAudioSource(volumeScale);
    source.transform.parent = transform;
    source.transform.localPosition = Vector3.zero;
  }

  private AudioSource prepAudioSource(float volumeScale) {
    AudioSource source = AudioSourceCache.instance.GetAudioSource();
    source.clip = getRandomClip();
    source.volume = _volume * volumeScale;
    source.pitch = _pitchCenter + (UnityEngine.Random.value - 0.5f) * _pitchVariance;
    source.Play();
    return source;
  }

  private AudioClip getRandomClip() {
    AudioClip clip = _clips[0];

    if (_clips.Length > 1) {
      int index = UnityEngine.Random.Range(1, _clips.Length);
      _clips[0] = _clips[index];
      _clips[index] = clip;
    }

    return clip;
  }

}
