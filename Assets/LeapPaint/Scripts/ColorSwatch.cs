using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ColorSwatch : MonoBehaviour {

  public enum SwatchMode {
    ReceiveColor,
    AssignColor
  }
  public SwatchMode _swatchMode = SwatchMode.AssignColor;

  [Tooltip("The swatch's color is stored in this graphic's color property.")]
  public Graphic _targetColorGraphic;

  private ColorPalette _palette;

  public void SetColor(Color color) {
    _targetColorGraphic.color = color;
  }

  public Color GetColor() {
    return _targetColorGraphic.color;
  }

  public void SetPalette(ColorPalette palette) {
    _palette = palette;
  }

  public void DoSwatchAction() {
    switch (_swatchMode) {
      case SwatchMode.AssignColor:
        SetNearestIndexTipColor();
        break;
      case SwatchMode.ReceiveColor: default:
        ReceiveNearestIndexTipColor();
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
