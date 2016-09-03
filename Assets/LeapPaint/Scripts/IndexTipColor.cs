using UnityEngine;
using System.Collections;
using Leap.Unity;

public class IndexTipColor : MonoBehaviour {

  public Leap.Hand _leapHand;
  public Renderer _tipMeshRenderer;
  public Color _startingColor = Color.white;

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

  protected void Start() {
    IHandModel handModel = GetComponentInParent<IHandModel>();
    _leapHand = handModel.GetLeapHand();
  }

  public Color GetColor() {
    return _color;
  }

  public void SetColor(Color color) {
    _color = color;
    _material.SetColor(Shader.PropertyToID("_Color"), color);
  }

  #region Mixing Paint Colors

  protected void OnTriggerStay(Collider other) {
    ColorMixingBasin mixingLiquid = other.GetComponentInParent<ColorMixingBasin>();
    if (mixingLiquid != null) {
      if (IsClean) {
        this.SetColor(mixingLiquid.GetColor());
      }
      else {
        float handSpeed = _leapHand.Fingers[(int)Leap.Finger.FingerType.TYPE_INDEX].TipVelocity.ToVector3().magnitude;
        if (handSpeed < 0.1F) handSpeed = 0F;
        this.SetColor(mixingLiquid.MixWithIndexTipColor(this, handSpeed));
      }
    }
    ColorCleaningBasin cleaningLiquid = other.GetComponentInParent<ColorCleaningBasin>();
    if (cleaningLiquid != null) {
      this.SetColor(new Color(0F, 0F, 0F, 0F));
    }
  }
  public void DoOnTriggerStay(Collider other) {
    // Callable as an event hook in case the collider/rigidbody getting the OnTriggerStay event is on a different object.
    this.OnTriggerStay(other);
  }

  #endregion

}
