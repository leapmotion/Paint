using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ColorSetter : MonoBehaviour {

  public Color _color;
  public Image _image;
  public PinchDrawing _pinchDrawing;
  public PinchRibbonDrawing _pinchRibbonDrawing;
  public bool _enabled = true;

  public Image _colorIndicator;

  protected void Start() {
    if (_pinchDrawing == null) {
      _pinchDrawing = GameObject.FindObjectOfType<PinchDrawing>();
    }
    if (_pinchRibbonDrawing == null) {
      _pinchRibbonDrawing = GameObject.FindObjectOfType<PinchRibbonDrawing>();
    }
  }

  void OnValidate() {
    if (_image != null) {
      _image.color = _color;
    }
  }

  public void OnClick() {
    // Set the current drawing color to this button's color.
    if (_enabled) {
      _pinchDrawing.SetColor(_color);
      _pinchRibbonDrawing.SetColor(_color);
      if (_colorIndicator != null) {
        _colorIndicator.color = _color;
      }
    }
  }

  public void Enable() {
    _enabled = true;
  }

  public void Disable() {
    _enabled = false;
  }

}
