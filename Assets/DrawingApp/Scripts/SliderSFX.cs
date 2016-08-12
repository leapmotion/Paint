using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SliderSFX : MonoBehaviour {

  public Slider _slider;
  public AudioSource _sfxSource;
  public AudioClip _sliderLoopFX;

  private float _lastSliderValue;

  private float _sliderFXDistanceAccum = 0F;
  private float _sliderFXDistance = 0.1F;

  protected void Start() {
    _lastSliderValue = _slider.value;
  }

  public void UpdateSliderSound() {
    float curSliderValue = _slider.value;
    float deltaValue = curSliderValue - _lastSliderValue;

    _sliderFXDistanceAccum += deltaValue;
    if (Mathf.Abs(_sliderFXDistanceAccum) >= _sliderFXDistance) {
      _sliderFXDistanceAccum = 0F;
      
      float deltaValueVolumeCoefficient = 50F;
      float volumeScale = Mathf.Lerp(0F, 1F, Mathf.Abs(deltaValue) * deltaValueVolumeCoefficient);
      _sfxSource.PlayOneShot(_sliderLoopFX, volumeScale);
    }

    _lastSliderValue = curSliderValue;
  }

}
