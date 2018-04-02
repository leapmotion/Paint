using UnityEngine;

namespace Leap.Unity.LeapPaint {

  public class ChangeColorWhenEnabled : MonoBehaviour {

    [Header("Color States")]

    public Color colorWhenEnabled = Color.white;
    public Color colorWhenDisabled = Color.gray;

    [Header("Renderer Reference")]

    public Renderer rendererToDrive;
    public struct RendererMaterialInstancePair {
      public Renderer renderer;
      public Material matInstance;
    }
    public RendererMaterialInstancePair _backingRendererMatPair
      = new RendererMaterialInstancePair();
    public Material materialInstance {
      get {
        // Edit-time color not supported.
        if (!Application.isPlaying) return null;

        // Cache material instance reference, but update it if the renderer changes.
        if (rendererToDrive == null) { return null; }
        else {
          var pair = _backingRendererMatPair;
          if (pair.renderer == null || pair.renderer != rendererToDrive) {
            pair.renderer = rendererToDrive;
            pair.matInstance = rendererToDrive.material; // creates material instance.
          }
          return pair.matInstance;
        }
      }
    }

    private void OnEnable() {
      materialInstance.color = colorWhenEnabled;
    }
    private void OnDisable() {
      materialInstance.color = colorWhenDisabled;
    }

  }

}
