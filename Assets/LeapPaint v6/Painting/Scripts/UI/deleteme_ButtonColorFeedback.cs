using Leap.Unity.Attributes;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class deleteme_ButtonColorFeedback : MonoBehaviour {

  public InteractionButton button;

  public Color maxEmissionColor = Color.white;

  public float idleEmissionAmount = 0f;
  public float primaryHoverEmissionAmount = 0.1f;
  public float pressedEmissionAmount = 1f;

  public float lerpCoeffPerSec = 20f;

  public Renderer rendererToSet;
  private Material _materialInstance = null;

  [EditTimeOnly]
  public string emissionPropertyName = "_EmissionColor";
  private int _shaderPropId = -1;
  
  [SerializeField]
  [Disable]
  private Color _emissionColor;

  public Color targetIdleColor {
    get { return Color.Lerp(Color.black, maxEmissionColor, idleEmissionAmount); }
  }
  public Color targetPrimaryHoverColor {
    get { return Color.Lerp(Color.black, maxEmissionColor, primaryHoverEmissionAmount); }
  }
  public Color targetPressedColor {
    get { return Color.Lerp(Color.black, maxEmissionColor, pressedEmissionAmount); }
  }

  void Reset() {
    if (rendererToSet == null) {
      rendererToSet = GetComponentInChildren<Renderer>();
    }

    if (button == null) {
      button = GetComponent<InteractionButton>();
    }
  }

  void OnEnable() {
    _emissionColor = targetIdleColor;

    if (_materialInstance == null) {
      _materialInstance = rendererToSet.material;
    }

    _shaderPropId = Shader.PropertyToID(emissionPropertyName);
  }

  void Update() {
    Color targetEmissionColor;
    if (button.isPressed) {
      targetEmissionColor = targetPressedColor;
    }
    else if (button.isPrimaryHovered) {
      targetEmissionColor = targetPrimaryHoverColor;
    }
    else {
      targetEmissionColor = targetIdleColor;
    }

    _emissionColor = Color.Lerp(_emissionColor, targetEmissionColor,
                                lerpCoeffPerSec * Time.deltaTime);

    _materialInstance.SetColor(_shaderPropId, targetEmissionColor);
  }

}
