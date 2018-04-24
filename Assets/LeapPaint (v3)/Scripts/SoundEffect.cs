using UnityEngine;
using UnityEngine.Audio;
using System;
using Leap.Unity.Attributes;

namespace Leap.Unity.LeapPaint_v3 {


  [Serializable]
  public class SoundEffect {

    [SerializeField]
    private AudioClip[] _clips;

    [SerializeField]
    private AudioMixerGroup _mixerGroup;

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

      AudioSourceCache.instance.PlayAtPosition(getRandomClip(), _mixerGroup, position, _volume * volumeScale, getRandomPitch());
    }

    public void PlayAtPosition(Transform transform, float volumeScale = 1) {
      PlayAtPosition(transform.position, volumeScale);
    }

    public void PlayOnTransform(Transform transform, float volumeScale = 1) {
      if (_clips.Length == 0) return;

      AudioSourceCache.instance.PlayOnTransform(getRandomClip(), _mixerGroup, transform, _volume * volumeScale, getRandomPitch());
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

    private float getRandomPitch() {
      return _pitchCenter + (UnityEngine.Random.value - 0.5f) * _pitchVariance;
    }

  }


}
