using Leap.Unity;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Attributes;

public class PinchStrokeProcessor : MonoBehaviour {

  private const float MIN_THICKNESS_MIN_SEGMENT_LENGTH = 0.001F;
  private const float MAX_THICKNESS_MIN_SEGMENT_LENGTH = 0.007F;
  private const float MAX_SEGMENT_LENGTH = 0.03F;

  public PaintCursor _paintCursor;
  [Tooltip("Used to stop drawing if the pinch detector is grabbing a UI element.")]
  public WearableManager _wearableManager;
  public HistoryManager _historyManager;
  public GameObject _ribbonParentObject;
  public RibbonIO _ribbonIO;
  public FilterIndexTipColor _colorFilter;
  public FilterApplyThickness _thicknessFilter;

  [Header("Effect Settings")]
  public SoundEffect _beginEffect;
  public AudioSource _soundEffectSource;
  [Range(0, 1)]
  public float _volumeScale = 1;
  [MinValue(0)]
  public float _maxEffectSpeed = 10;
  [MinMax(0, 2)]
  public Vector2 _pitchRange = new Vector2(0.8f, 1);
  [MinValue(0)]
  public float _smoothingDelay = 0.05f;


  private bool _paintingStroke = false;
  private bool _paintingPreviewStroke = false;
  private StrokeProcessor _strokeProcessor;
  private bool _firstStrokePointAdded = false;
  private Vector3 _lastStrokePointAdded = Vector3.zero;
  private float _timeSinceLastAddition = 0F;

  private Vector3 leftHandEulerRotation = new Vector3(0F, 180F, 0F);
  private Vector3 rightHandEulerRotation = new Vector3(0F, 180F, 0F);

  private StrokeRibbonRenderer _ribbonRenderer;
  private StrokeBufferRibbonRenderer _previewRibbonRenderer;

  private Vector3 _prevPosition;
  private SmoothedFloat _smoothedSpeed = new SmoothedFloat();
  [HideInInspector]
  public float drawTime = 0f;

  void Start() {
    _smoothedSpeed.delay = _smoothingDelay;

    _strokeProcessor = new StrokeProcessor();

    // Set up and register filters.
    FilterPositionMovingAverage movingAvgFilter = new FilterPositionMovingAverage(6);
    _strokeProcessor.RegisterStrokeFilter(movingAvgFilter);
    FilterPitchYawRoll pitchYawRollFilter = new FilterPitchYawRoll();
    _strokeProcessor.RegisterStrokeFilter(pitchYawRollFilter);

    _strokeProcessor.RegisterStrokeFilter(_colorFilter);
    _strokeProcessor.RegisterStrokeFilter(_thicknessFilter);



    // Set up and register renderers.
    GameObject rendererObj = new GameObject();
    rendererObj.name = "Stroke Ribbon Renderer";
    _ribbonRenderer = rendererObj.AddComponent<StrokeRibbonRenderer>();
    _ribbonRenderer.OnMeshStrokeFinalized += DoOnMeshStrokeFinalized;
    _strokeProcessor.RegisterStrokeRenderer(_ribbonRenderer);

    GameObject previewRendererObj = new GameObject();
    previewRendererObj.name = "Stroke Preview Ribbon Renderer";
    _previewRibbonRenderer = previewRendererObj.AddComponent<StrokeBufferRibbonRenderer>();
    _strokeProcessor.RegisterPreviewStrokeRenderer(_previewRibbonRenderer);
  }

  void Update() {
    if (_paintCursor.IsTracked && !_paintingPreviewStroke) {
      BeginStroke();
      _paintingPreviewStroke = true;
    }

    if (_paintCursor.IsActive && !_paintingStroke) {
      if (!_wearableManager.IsPinchDetectorGrabbing(_paintCursor._pinchDetector)) {
        // TODO HACK FIXME preventing drawing if IndexTipColor is transparent
        Color color = new Color(0F, 0F, 0F, 0F);
        try {
          color = _paintCursor._pinchDetector.GetComponentInParent<IHandModel>().GetComponentInChildren<IndexTipColor>().GetColor();
        }
        catch (System.NullReferenceException) { }
        if (color.a > 0.99F) {
          StartActualizingStroke();
          _paintingStroke = true;
        }
      }
    }

    if (_paintCursor.IsTracked && _paintingPreviewStroke) {
      UpdateStroke();
    }

    if (!_paintCursor.IsActive && _paintingStroke) {
      StopActualizingStroke();
      _paintingStroke = false;
    }

    if (!_paintCursor.IsTracked && _paintingPreviewStroke) {
      EndStroke();
      _paintingPreviewStroke = false;
    }
  }

  private void BeginStroke() {
    _strokeProcessor.BeginStroke();
    _prevPosition = _paintCursor.Position;
  }

  private void StartActualizingStroke() {
    _strokeProcessor.StartActualizingStroke();
    _beginEffect.PlayOnTransform(_soundEffectSource.transform);
    _soundEffectSource.Play();
  }

  private void UpdateStroke() {
    float speed = Vector3.Distance(_paintCursor.Position, _prevPosition) / Time.deltaTime;
    _prevPosition = _paintCursor.Position;
    _smoothedSpeed.Update(speed, Time.deltaTime);

    float effectPercent = Mathf.Clamp01(_smoothedSpeed.value / _maxEffectSpeed);
    _soundEffectSource.volume = effectPercent * _volumeScale;
    _soundEffectSource.pitch = Mathf.Lerp(_pitchRange.x, _pitchRange.y, effectPercent);

    Vector3 strokePosition = _paintCursor.Position;

    if (_firstStrokePointAdded) {
      float posDelta = Vector3.Distance(_lastStrokePointAdded, strokePosition);
      if (posDelta > MAX_SEGMENT_LENGTH) {
        float segmentFraction = posDelta / MAX_SEGMENT_LENGTH;
        float segmentRemainder = segmentFraction % 1F;
        int numSegments = (int)Mathf.Floor(segmentFraction);
        Vector3 segment = (strokePosition - _lastStrokePointAdded).normalized * MAX_SEGMENT_LENGTH;
        Vector3 curPos = _lastStrokePointAdded;
        float segmentDeltaTime = Time.deltaTime * segmentFraction;
        float remainderDeltaTime = Time.deltaTime * segmentRemainder;
        float curDeltaTime = 0F;
        for (int i = 0; i < numSegments; i++) {
          ProcessAddStrokePoint(curPos + segment, curDeltaTime + segmentDeltaTime);
          curPos += segment;
          curDeltaTime += segmentDeltaTime;
        }
        ProcessAddStrokePoint(strokePosition, curDeltaTime + remainderDeltaTime);
      }
      else {
        ProcessAddStrokePoint(strokePosition, Time.deltaTime);
      }
    }
    else {
      ProcessAddStrokePoint(strokePosition, Time.deltaTime);
    }
    drawTime += Time.deltaTime;
  }

  private void ProcessAddStrokePoint(Vector3 point, float effDeltaTime) {
    bool shouldAdd = !_firstStrokePointAdded
      || Vector3.Distance(_lastStrokePointAdded, point)
          >= Mathf.Lerp(MIN_THICKNESS_MIN_SEGMENT_LENGTH, MIN_THICKNESS_MIN_SEGMENT_LENGTH, _thicknessFilter._lastNormalizedValue);

    _timeSinceLastAddition += effDeltaTime;

    if (shouldAdd) {
      StrokePoint strokePoint = new StrokePoint();
      strokePoint.position = point;
      strokePoint.rotation = Quaternion.identity;
      strokePoint.handOrientation = _paintCursor.Rotation * Quaternion.Euler((_paintCursor.Handedness == Chirality.Left ? leftHandEulerRotation : rightHandEulerRotation));
      strokePoint.deltaTime = _timeSinceLastAddition;

      _strokeProcessor.UpdateStroke(strokePoint);

      _firstStrokePointAdded = true;
      _lastStrokePointAdded = strokePoint.position;
      _timeSinceLastAddition = 0F;
    }
  }

  private void StopActualizingStroke() {
    _strokeProcessor.StopActualizingStroke();
    _soundEffectSource.Pause();
    _soundEffectSource.volume = 0;
    _smoothedSpeed.reset = true;
  }

  private void EndStroke() {
    _strokeProcessor.EndStroke();
    _firstStrokePointAdded = false;
  }

  // TODO DELETEME FIXME
  private void DoOnMeshStrokeFinalized(Mesh mesh, List<StrokePoint> stroke) {
    GameObject finishedRibbonMesh = new GameObject();
    if (stroke != null && stroke.Count > 0) {
      finishedRibbonMesh.name = stroke[0].color.r + ", " + stroke[0].color.g + ", " + stroke[0].color.b;
    }
    MeshFilter filter = finishedRibbonMesh.AddComponent<MeshFilter>();
    MeshRenderer renderer = finishedRibbonMesh.AddComponent<MeshRenderer>();
    Material ribbonMat = new Material(Shader.Find("LeapMotion/RibbonShader"));
    ribbonMat.hideFlags = HideFlags.HideAndDontSave;
    renderer.material = ribbonMat;
    filter.mesh = mesh;

    _historyManager.NotifyStroke(finishedRibbonMesh, stroke);

    finishedRibbonMesh.transform.parent = _ribbonParentObject.transform;
  }

  // Used to produce strokes from stroke objects, e.g., when loading scenes.
  public void ShortcircuitStrokeToRenderer(List<StrokePoint> stroke) {
    _ribbonRenderer.InitializeRenderer();
    _ribbonRenderer.RefreshRenderer(stroke, stroke.Count - 1);
    _ribbonRenderer.FinalizeRenderer();
  }

}