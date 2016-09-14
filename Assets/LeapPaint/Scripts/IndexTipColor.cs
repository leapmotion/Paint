using UnityEngine;
using System.Collections;
using Leap.Unity;

public class IndexTipColor : MonoBehaviour {

  public IHandModel _hand;
  public Renderer _tipMeshRenderer;
  public Color _startingColor = Color.white;

  [Header("Color Marble")]
  public Material _colorMarbleMaterial;
  [Tooltip("The material to use for the color marble when the index tip color is transparent.")]
  public Material _transparentMarbleMaterial;
  public Renderer _colorMarbleRenderer;

  private Color _color;
  private Material _material;

  public bool IsClean {
    get { return GetColor().a < 0.001F; }
  }

  protected void OnEnable() {
    _material = new Material(_tipMeshRenderer.sharedMaterial);
    _tipMeshRenderer.material = _material;
  }

  protected void OnValidate() {
    OnEnable();
    SetColor(_startingColor);
  }

  public Color GetColor() {
    return _color;
  }

  public void SetColor(Color color) {
    _color = color;
    _material.SetColor(Shader.PropertyToID("_Color"), color);
    if (color.a < 0.01F) {
      _colorMarbleRenderer.sharedMaterial = _transparentMarbleMaterial;
    }
    else {
      _colorMarbleRenderer.sharedMaterial = _colorMarbleMaterial;
      _colorMarbleMaterial.color = color;
    }
  }

  #region Mixing Paint Colors

  protected void OnTriggerStay(Collider other) {
    ColorMixingBasin mixingLiquid = other.GetComponentInParent<ColorMixingBasin>();
    if (mixingLiquid != null && mixingLiquid.enabled) {
      if (IsClean) {
        this.SetColor(mixingLiquid.GetColor());
      }
      else {
        float handSpeed = _hand.GetLeapHand().Fingers[(int)Leap.Finger.FingerType.TYPE_INDEX].TipVelocity.ToVector3().magnitude;
        if (handSpeed < 0.1F) handSpeed = 0F;
        this.SetColor(mixingLiquid.MixWithIndexTipColor(this, handSpeed));
      }
    }
    ColorCleaningBasin cleaningLiquid = other.GetComponentInParent<ColorCleaningBasin>();
    if (cleaningLiquid != null && cleaningLiquid.enabled) {
      this.SetColor(new Color(0F, 0F, 0F, 0F));
    }
  }
  public void DoOnTriggerStay(Collider other) {
    // Callable as an event hook in case the collider/rigidbody getting the OnTriggerStay event is on a different object.
    this.OnTriggerStay(other);
  }

  #endregion

}
