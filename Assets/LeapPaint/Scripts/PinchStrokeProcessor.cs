using Leap.Unity;
using System.Collections.Generic;
using UnityEngine;

public class PinchStrokeProcessor : MonoBehaviour {

  private const float MIN_SEGMENT_LENGTH = 0.005F;

  public PinchDetector _pinchDetector;
  [Tooltip("Used to stop drawing if the pinch detector is grabbing a UI element.")]
  public WearableManager _wearableManager;
  public UndoRedoManager _undoRedoManager;

  public bool _usePitchYaw = true;

  private bool _paintingStroke = false;
  private StrokeProcessor _strokeProcessor;
  private bool _firstStrokePointAdded = false;
  private Vector3 _lastStrokePointAdded = Vector3.zero;
  private float _timeSinceLastAddition = 0F;

  private StrokeRibbonRenderer _ribbonRenderer;

  void Start() {
    _strokeProcessor = new StrokeProcessor();

    // Set up and register filters.
    //FilterDebugLogMemory debugLogFilter = new FilterDebugLogMemory();
    //_strokeProcessor.RegisterStrokeFilter(debugLogFilter);
    FilterPositionMovingAverage movingAvgFilter = new FilterPositionMovingAverage(6);
    _strokeProcessor.RegisterStrokeFilter(movingAvgFilter);
    if (_usePitchYaw) {
      FilterPitchYawTangent pitchYawFilter = new FilterPitchYawTangent();
      _strokeProcessor.RegisterStrokeFilter(pitchYawFilter);
      //FilterRollToCanvasAlignment rollCanvasAlignFilter = new FilterRollToCanvasAlignment();
      //_strokeProcessor.RegisterStrokeFilter(rollCanvasAlignFilter);
    }
    else {
      FilterNaiveCanvasAlignment canvasAlignmentFilter = new FilterNaiveCanvasAlignment();
      _strokeProcessor.RegisterStrokeFilter(canvasAlignmentFilter);
    }

    // Set up and register renderers.
    GameObject rendererObj = new GameObject();
    _ribbonRenderer = rendererObj.AddComponent<StrokeRibbonRenderer>();
    _ribbonRenderer.Color = Color.red; 
    _ribbonRenderer.Thickness = 0.02F;
    _ribbonRenderer.OnMeshFinalized += DoOnMeshFinalized;
    _strokeProcessor.RegisterStrokeRenderer(_ribbonRenderer);
  }

  void Update() {
    if (_pinchDetector.IsActive && !_paintingStroke) {
      if (!_wearableManager.IsPinchDetectorGrabbing(_pinchDetector)) {
        BeginStroke();
        _paintingStroke = true;
      }
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
    // TODO HACK FIXME
    _ribbonRenderer.Color = _pinchDetector.GetComponentInParent<IHandModel>().GetComponentInChildren<IndexTipColor>().GetColor();
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

  // TODO DELETEME FIXME
  private void DoOnMeshFinalized(Mesh mesh) {
    GameObject finishedRibbonMesh = new GameObject();
    MeshFilter filter = finishedRibbonMesh.AddComponent<MeshFilter>();
    MeshRenderer renderer = finishedRibbonMesh.AddComponent<MeshRenderer>();
    Material ribbonMat = new Material(Shader.Find("LeapMotion/RibbonShader"));
    ribbonMat.hideFlags = HideFlags.HideAndDontSave;
    renderer.material = ribbonMat;
    filter.mesh = mesh;
    _undoRedoManager.NotifyAction(finishedRibbonMesh);
  }

}