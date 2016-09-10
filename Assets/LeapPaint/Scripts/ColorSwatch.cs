using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ColorSwatch : MonoBehaviour {

  public enum SwatchMode {
    ReceiveColor,
    AssignColor,
    DoNothing
  }
  public SwatchMode _swatchMode = SwatchMode.AssignColor;

  [Header("Choose one but not both; Graphic overrides the MeshRenderer.")]
  [Tooltip("The swatch's color is stored in this Graphic's color property.")]
  public Graphic _targetColorGraphic;
  [Tooltip("The swatch's color is stored in a Material instance's color property.")]
  public MeshRenderer _targetColorRenderer;

  private ColorPalette _palette;

  public void SetColor(Color color) {
    if (_targetColorGraphic != null) {
      _targetColorGraphic.color = color;
    }
    else {
      _targetColorRenderer.material.color = color;
      //_targetColorRenderer.material.SetColor(Shader.PropertyToID("_EmissionColor"), color / 4F);
    }
  }

  public Color GetColor() {
    if (_targetColorGraphic != null) {
      return _targetColorGraphic.color;
    }
    else {
      return _targetColorRenderer.material.color;
    }
  }

  public void SetPalette(ColorPalette palette) {
    _palette = palette;
  }

  public void DoSwatchAction() {
    switch (_swatchMode) {
      case SwatchMode.AssignColor:
        SetNearestIndexTipColor();
        break;
      case SwatchMode.ReceiveColor:
        ReceiveNearestIndexTipColor();
        break;
      default:
        break;
    }
  }

  public void SetNearestIndexTipColor() {
    IndexTipColor nearestIndexTipColor = GetNearestTipColor();
    if (nearestIndexTipColor != null) {
      nearestIndexTipColor.SetColor(this.GetColor());
    }
  }

  public void ReceiveNearestIndexTipColor() {
    IndexTipColor nearestIndexTipColor = GetNearestTipColor();
    if (nearestIndexTipColor != null) {
      if (nearestIndexTipColor.IsClean) {
        // If the index tip is clean, set its color instead of receiving its color.
        nearestIndexTipColor.SetColor(this.GetColor());
      }
      else {
        this.SetColor(nearestIndexTipColor.GetColor());
      }
    }
  }

  private IndexTipColor GetNearestTipColor() {
    IndexTipColor[] eligibleTipColors = _palette._eligibleIndexTipColors;
    IndexTipColor nearestIndexTipColor = null;
    for (int i = 0; i < eligibleTipColors.Length; i++) {
      if (nearestIndexTipColor == null
        || Vector3.Distance(this.transform.position, eligibleTipColors[i].transform.position) < Vector3.Distance(this.transform.position, nearestIndexTipColor.transform.position)) {
        nearestIndexTipColor = eligibleTipColors[i];
      }
    }
    if (nearestIndexTipColor != null) {
      return nearestIndexTipColor;
    }
    else {
      Debug.LogWarning("[ColorSwatch] No IndexTipColor found! Could not retrieve nearest one.");
      return null;
    }
  }

  public void SetMode(SwatchMode mode) {
    _swatchMode = mode;
  }

}
