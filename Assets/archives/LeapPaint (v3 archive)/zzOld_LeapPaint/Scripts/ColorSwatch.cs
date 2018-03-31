using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Leap.Unity.LeapPaint_v3 {


  public class ColorSwatch : MonoBehaviour {

    public enum SwatchMode {
      ReceiveColor,
      AssignColor,
      DoNothing
    }
    public SwatchMode _swatchMode = SwatchMode.AssignColor;

    [Header("Swatch Mode Feedback")]
    public Transform enableWhenReceivingColor = null;

    [Header("Choose one but not both; Graphic overrides the MeshRenderer.")]
    [Tooltip("The swatch's color is stored in this Graphic's color property.")]
    public Graphic _targetColorGraphic;
    [Tooltip("The swatch's color is stored in a Material instance's color property.")]
    public MeshRenderer _targetColorRenderer;

    [Header("SFX")]
    public SoundEffect soundEffect;

    private ColorPalette _palette;

    private void Update() {
      if (enableWhenReceivingColor != null) {
        enableWhenReceivingColor.gameObject
          .SetActive(_swatchMode == SwatchMode.ReceiveColor);
      }
    }

    public void SetColor(Color color) {
      if (_targetColorGraphic != null) {
        _targetColorGraphic.color = color;
      }
      else {
        _targetColorRenderer.material.color = color;
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
      if(_swatchMode != SwatchMode.DoNothing) {
        soundEffect.PlayAtPosition(transform);
      }

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


}
