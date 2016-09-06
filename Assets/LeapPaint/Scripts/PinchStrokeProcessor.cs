using Leap.Unity;
using UnityEngine;

public class PinchStrokeProcessor : MonoBehaviour {

  private const float MIN_SEGMENT_LENGTH = 0.005F;

  public PinchDetector _pinchDetector;

  private bool _paintingStroke = false;
  private StrokeProcessor _strokeProcessor;
  private bool _firstStrokePointAdded = false;
  private Vector3 _lastStrokePointAdded = Vector3.zero;
  private float _timeSinceLastAddition = 0F;

  void Start() {
    _strokeProcessor = new StrokeProcessor();

    // Set up and register filters.
    FilterPositionMovingAverage movingAvgFilter = new FilterPositionMovingAverage(6);
    _strokeProcessor.RegisterStrokeFilter(movingAvgFilter);
    FilterPitchYawTangent pitchYawFilter = new FilterPitchYawTangent();
    _strokeProcessor.RegisterStrokeFilter(pitchYawFilter);

    // Set up and register renderers.
    GameObject rendererObj = new GameObject();
    StrokeRibbonRenderer ribbonRenderer = rendererObj.AddComponent<StrokeRibbonRenderer>();
    ribbonRenderer.Color = Color.red;
    ribbonRenderer.Thickness = 0.05F;
    _strokeProcessor.RegisterStrokeRenderer(ribbonRenderer);
  }

  void Update() {
    if (_pinchDetector.IsActive && !_paintingStroke) {
      BeginStroke();
      _paintingStroke = true;
    }
    else if (_pinchDetector.IsActive && _paintingStroke) {
      UpdateStroke();
    }
    else if (!_pinchDetector.IsActive && _paintingStroke) {
      EndStroke();
      _paintingStroke = false;
    }
  }

  private void BeginStroke() {
    _strokeProcessor.BeginStroke();
  }

  private void UpdateStroke() {
    bool shouldAdd = !_firstStrokePointAdded
      || Vector3.Distance(_lastStrokePointAdded, _pinchDetector.Position) >= MIN_SEGMENT_LENGTH;

    _timeSinceLastAddition += Time.deltaTime;

    if (shouldAdd) {
      StrokePoint strokePoint = new StrokePoint();
      strokePoint.position = _pinchDetector.Position;
      strokePoint.rotation = Quaternion.identity;
      strokePoint.handOrientation = _pinchDetector.Rotation * Quaternion.Euler(new Vector3(90F, 0F, 180F));
      strokePoint.deltaTime = _timeSinceLastAddition;

      _strokeProcessor.UpdateStroke(strokePoint);

      _firstStrokePointAdded = true;
      _lastStrokePointAdded = strokePoint.position;
      _timeSinceLastAddition = 0F;
    }
  }

  private void EndStroke() {
    _strokeProcessor.EndStroke();
  }

}