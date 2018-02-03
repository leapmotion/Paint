using Leap.Unity.Drawing;
using UnityEngine;

namespace Leap.Unity.LeapPaint {

  public class ColorSwatch : MonoBehaviour {

    public Paintbrush brushColorReceiver;
    public Color swatchColor;

    public Renderer swatchRendererToSet;
    public string colorPropertyName = "_Color";
    private int _colorPropId = -1;
    private Material _materialInstance;

    void OnValidate() {
      if (swatchRendererToSet != null) {
        swatchRendererToSet.sharedMaterial.SetColor(colorPropertyName, swatchColor);
      }
    }

    void OnEnable() {
      if (swatchRendererToSet == null) {
        swatchRendererToSet = GetComponentInChildren<Renderer>();
      }
      if (_materialInstance == null) {
        _materialInstance = swatchRendererToSet.material;
      }

      _colorPropId = Shader.PropertyToID(colorPropertyName);

      sendColorToSwatchRenderer();
    }

    public void SendColorToBrush() {
      brushColorReceiver.color = swatchColor;

      sendColorToSwatchRenderer();
    }

    private void sendColorToSwatchRenderer() {
      if (_materialInstance != null) {
        _materialInstance.SetColor(_colorPropId, swatchColor);
      }
    }

  }

}