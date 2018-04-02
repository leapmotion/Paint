using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity.LeapPaint {

  public class AnimateColorWhenEnabled : MonoBehaviour {

    [Header("Color States")]

    public Color colorWhenEnabled0 = Color.white;
    public Color colorWhenEnabled1 = Color.yellow;
    public AnimationCurve animCurve = DefaultCurve.SigmoidUpDown;
    [MinValue(0.01f)]
    public float animPeriod = 2.0f;
    private float _animT = 0f;
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
      materialInstance.color = colorWhenEnabled0;
      _animT = 0f;
    }
    private void Update() {
      _animT += Time.deltaTime;
      _animT %= animPeriod;

      materialInstance.color
        = Color.Lerp(colorWhenEnabled0, colorWhenEnabled1, animCurve.Evaluate(_animT));
    }
    private void OnDisable() {
      materialInstance.color = colorWhenDisabled;
    }

  }

}
