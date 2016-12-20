using UnityEngine;
using System.Collections;
using Leap.Unity;

public class ColorPalette : MonoBehaviour {

  public ColorSwatch.SwatchMode _swatchMode = ColorSwatch.SwatchMode.AssignColor;
  public ColorSwatch[] _swatches;
  public Color[] _swatchColors;
  public IndexTipColor[] _eligibleIndexTipColors;

  protected void OnValidate() {
    for (int i = 0; i < _swatchColors.Length; i++) {
      _swatchColors[i] = new Color(_swatchColors[i].r, _swatchColors[i].g, _swatchColors[i].b, 1F);
    }
  }

  protected void Awake() {
    Debug.Assert(_swatches.Length == _swatchColors.Length, "[ColorPalette] Registered swatches must be the same size as registered swatch colors!");

    for (int i = 0; i < _swatches.Length; i++) {
      _swatches[i].SetColor(_swatchColors[i]);
      _swatches[i].SetPalette(this);
      _swatches[i].SetMode(_swatchMode);
    }
  }

  public void SetSwatchMode(ColorSwatch.SwatchMode toSet) {
    for (int i = 0; i < _swatchColors.Length; i++) {
      _swatches[i].SetMode(toSet);
    }
  }

}
