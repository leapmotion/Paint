using UnityEngine;
using System.Collections.Generic;
using Leap.Unity.RuntimeGizmos;

public class ColorMixingBasin : MonoBehaviour {

  public MeshRenderer _liquidMeshRenderer;

  private float _mixingCoefficient = 0.05F;

  protected void Start() {
    SetColor(Color.white);
  }

  /// <summary>
  /// Lerps this mixing liquid's color towards the index tip's color, and returns the index tip's color lerped towards the mixing liquid's color.
  /// </summary>
  public Color MixWithIndexTipColor(IndexTipColor indexTipColor, float multiplier=1F) {
    Color mixColor = indexTipColor.GetColor();
    Color liquidColor = _liquidMeshRenderer.material.GetColor(Shader.PropertyToID("_Color"));
    _liquidMeshRenderer.material.SetColor("_Color", Color.Lerp(liquidColor, mixColor, _mixingCoefficient * multiplier));
    liquidColor = _liquidMeshRenderer.material.GetColor(Shader.PropertyToID("_Color"));
    return Color.Lerp(mixColor, liquidColor, _mixingCoefficient * multiplier);
  }

  public Color GetColor() {
    return _liquidMeshRenderer.material.GetColor(Shader.PropertyToID("_Color"));
  }

  public void SetColor(Color color) {
    _liquidMeshRenderer.material.SetColor(Shader.PropertyToID("_Color"), color);
  }

}