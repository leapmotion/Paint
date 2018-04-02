using UnityEngine;
using System.Collections;
using Leap.Unity;
using UnityEngine.Serialization;

namespace Leap.Unity.LeapPaint_v3 {

  public class ColorPalette : MonoBehaviour {

    [FormerlySerializedAs("_swatchMode")]
    public ColorSwatch.SwatchMode swatchMode = ColorSwatch.SwatchMode.AssignColor;

    [FormerlySerializedAs("_swatches")]
    public ColorSwatch[] swatches;

    [FormerlySerializedAs("_swatchColors")]
    public Color[] swatchColors;

    [FormerlySerializedAs("_eligibleIndexTipColors")]
    public IndexTipColor[] eligibleIndexTipColors;

    [Header("Swatch Mode Feedback")]
    public GameObject enabledWhenReceivingColor = null;

    protected void OnValidate() {
      for (int i = 0; i < swatchColors.Length; i++) {
        swatchColors[i] = new Color(swatchColors[i].r, swatchColors[i].g, swatchColors[i].b, 1F);
      }
    }

    protected void Awake() {
      Debug.Assert(swatches.Length == swatchColors.Length, "[ColorPalette] Registered swatches must be the same size as registered swatch colors!");

      for (int i = 0; i < swatches.Length; i++) {
        swatches[i].SetColor(swatchColors[i]);
        swatches[i].SetPalette(this);
        swatches[i].SetMode(swatchMode);
      }
    }

    protected void Update() {
      var receivingColorStateEnabled = swatchMode == ColorSwatch.SwatchMode.ReceiveColor;

      if (enabledWhenReceivingColor != null) {
        enabledWhenReceivingColor.SetActive(receivingColorStateEnabled);
      }
    }

    public void SetSwatchMode(ColorSwatch.SwatchMode toSet) {
      swatchMode = toSet;
      for (int i = 0; i < swatchColors.Length; i++) {
        swatches[i].SetMode(toSet);
      }
    }

  }


}
