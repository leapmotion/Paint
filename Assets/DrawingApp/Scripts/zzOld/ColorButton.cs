using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour {

  public Color _color;
  public Image _image;
  public PinchDrawing _pinchDrawing;

  void OnValidate() {
    if (_image != null) {
      _image.color = _color;
    }
  }

  public void OnClick() {
    // Set the current drawing color to this button's color.
    _pinchDrawing.SetColor(_color);
  }

}
