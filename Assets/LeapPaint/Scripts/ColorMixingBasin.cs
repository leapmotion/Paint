using UnityEngine;
using System.Collections.Generic;
using Leap.Unity.RuntimeGizmos;

public class ColorMixingBasin : MonoBehaviour {

  public MeshRenderer _liquidMeshRenderer;

  public SoundEffect _mixEffect;
  public float _effectPeriod = 0.2f;
  public float _maxEffectValue = 1;

  private float _mixingCoefficient = 0.05F;
  private float _nextEffectTime = 0;

  protected void Start() {
    SetColor(Color.white);
  }

  /// <summary>
  /// Lerps this mixing liquid's color towards the index tip's color, and returns the index tip's color lerped towards the mixing liquid's color.
  /// </summary>
  public Color MixWithIndexTipColor(IndexTipColor indexTipColor, float multiplier = 1F) {
    Debug.Log(multiplier);
    int extra = (int)((Time.time - _nextEffectTime) / _effectPeriod);
    if (extra > 1) {
      _nextEffectTime += extra * _effectPeriod;
      _mixEffect.PlayOnTransform(transform, Mathf.Clamp01(multiplier / _maxEffectValue));
    }

    Color mixColor = indexTipColor.GetColor();
    Color liquidColor = _liquidMeshRenderer.material.GetColor(Shader.PropertyToID("_Color"));
    _liquidMeshRenderer.material.SetColor("_Color", Color.Lerp(liquidColor, mixColor, _mixingCoefficient * multiplier));
    //Debug.Log("Liquid lerped to index tip distance: " + GetColorDistance(mixColor, Color.Lerp(liquidColor, mixColor, _mixingCoefficient * multiplier)));
    liquidColor = _liquidMeshRenderer.material.GetColor(Shader.PropertyToID("_Color"));
    return Color.Lerp(mixColor, liquidColor, _mixingCoefficient * multiplier);
  }

  public Color GetColor() {
    return _liquidMeshRenderer.material.GetColor(Shader.PropertyToID("_Color"));
  }

  public void SetColor(Color color) {
    _liquidMeshRenderer.material.SetColor(Shader.PropertyToID("_Color"), color);
  }

  private float GetColorDistance(Color a, Color b) {
    return (Mathf.Sqrt((a.r - b.r) * (a.r - b.r) + (a.g - b.g) * (a.g - b.g) + (a.b - b.b) * (a.b - b.b)));
  }

}