using Leap.Unity.Attributes;
using Leap.Unity.GraphicalRenderer;
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

    [Header("Renderer Reference - Pick One (Graphic Overrides)")]

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
    public LeapGraphic graphicToDrive;

    private void OnEnable() {
      setColor(colorWhenEnabled0);
      _animT = 0f;
    }
    private void Update() {
      _animT += Time.deltaTime;
      _animT %= animPeriod;

      setColor(Color.Lerp(colorWhenEnabled0, colorWhenEnabled1, animCurve.Evaluate(_animT)));
    }
    private void OnDisable() {
      setColor(colorWhenDisabled);
    }

    private void setColor(Color c) {
      if (graphicToDrive != null) {
        var colorFeature = graphicToDrive.GetFeatureData<LeapRuntimeTintData>();
        if (colorFeature == null) {
          Debug.LogError("No runtime tint (color) feature available for "
            + graphicToDrive.name, this);
        }
        else {
          colorFeature.color = c;
        }
      }
      else {
        materialInstance.color = c;
      }
    }

  }

}
