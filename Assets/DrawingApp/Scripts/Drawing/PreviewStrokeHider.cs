using UnityEngine;
using System.Collections;
using Leap.Unity;

public class PreviewStrokeHider : MonoBehaviour {

  public IHandModel _previewStrokeHand;
  public PinchDrawing _pinchDrawing;
  public PalmDirectionDetector _uiDetector;

  protected void Update() {
    if (!_previewStrokeHand.IsTracked && _pinchDrawing.IsPreviewStrokeDisplaying) {
      _pinchDrawing.HidePreviewStroke();
    }
    if (!_pinchDrawing.IsPreviewStrokeDisplaying && _uiDetector.IsActive) {
      _pinchDrawing.DisplayPreviewStroke();
    }
  }

}
