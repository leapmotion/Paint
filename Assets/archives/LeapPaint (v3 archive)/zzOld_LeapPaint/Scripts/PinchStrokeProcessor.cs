using Leap.Unity;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Attributes;
using Leap.Unity.Interaction;

namespace Leap.Unity.LeapPaint_v3 {

  public class PinchStrokeProcessor : MonoBehaviour {

    private const float MIN_THICKNESS_MIN_SEGMENT_LENGTH = 0.001F;
    private const float MAX_THICKNESS_MIN_SEGMENT_LENGTH = 0.003F;
    private const float MAX_SEGMENT_LENGTH = 0.02F;
    private const float MIN_HAND_DRAWING_LIFETIME = 0.2F;

    public PaintCursor _paintCursor;
    [Tooltip("Used to stop drawing if the pinch detector is grabbing a UI element.")]
    public WearableManager _wearableManager;
    public HistoryManager _historyManager;
    public GameObject _ribbonParentObject;
    public Material _ribbonMaterial;
    public RibbonIO _ribbonIO;
    public FilterIndexTipColor _colorFilter;
    public FilterApplyThickness _thicknessFilter;
    public AnimationCurve _thicknessCurve;
    public InteractionHand interactionHand;

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


    private StrokeProcessor _strokeProcessor;
    private bool _firstStrokePointAdded = false;
    private Vector3 _lastStrokePointAdded = Vector3.zero;
    private float _timeSinceLastAddition = 0F;
    private bool _inDangerZone = false;
    private float _handLifetime = 0F;

    private Vector3 leftHandEulerRotation = new Vector3(0F, 180F, 0F);
    private Vector3 rightHandEulerRotation = new Vector3(0F, 180F, 0F);
    private Leap.Hand _hand;

    private IStrokeRenderer _strokeRenderer;

    private Vector3 _prevPosition;
    private SmoothedFloat _smoothedSpeed = new SmoothedFloat();
    [HideInInspector]
    public float drawTime = 0f;

    void Start() {
      _smoothedSpeed.delay = _smoothingDelay;

      _strokeProcessor = new StrokeProcessor();

      // Set up and register filters.
      _strokeProcessor.RegisterStrokeFilter(new FilterPositionMovingAverage());
      _strokeProcessor.RegisterStrokeFilter(new FilterPitchYawRoll());

      _strokeProcessor.RegisterStrokeFilter(_colorFilter);
      _strokeProcessor.RegisterStrokeFilter(_thicknessFilter);

      _strokeProcessor.RegisterStrokeFilter(new FilterConstrainThickness());
      _strokeProcessor.RegisterStrokeFilter(new FilterSmoothThickness());

      // Set up and register renderers.
      //GameObject rendererObj = new GameObject();
      //rendererObj.name = "Stroke Ribbon Renderer";
      //var ribbonRenderer = rendererObj.AddComponent<StrokeRibbonRenderer>();
      //ribbonRenderer.OnMeshStrokeFinalized += DoOnMeshStrokeFinalized;
      //_strokeRenderer = ribbonRenderer;
      //_strokeProcessor.RegisterStrokeRenderer(ribbonRenderer);

      GameObject rendererObj = new GameObject();
      rendererObj.name = "Thick Ribbon Renderer";
      var thickRibbonRenderer = rendererObj.AddComponent<ThickRibbonRenderer>();
      thickRibbonRenderer._finalizedRibbonParent = _ribbonParentObject;
      thickRibbonRenderer._ribbonMaterial = _ribbonMaterial;
      _strokeRenderer = thickRibbonRenderer;
      _strokeProcessor.RegisterStrokeRenderer(thickRibbonRenderer);

      //GameObject previewRendererObj = new GameObject();
      //previewRendererObj.name = "Stroke Preview Ribbon Renderer";
      //var previewRibbonRenderer = previewRendererObj.AddComponent<StrokeBufferRibbonRenderer>();
      //previewRibbonRenderer.previewThicknessCurve = _thicknessCurve;
      //_strokeProcessor.RegisterPreviewStrokeRenderer(previewRibbonRenderer);
    }

    void Update() {

      // Drawing Conditionals //

      if (_paintCursor.IsTracked) {
        _handLifetime += Time.deltaTime;
      }
      else {
        _handLifetime = 0F;
      }

      _inDangerZone = false;
      // old shitty danger zone system
      {
        //for (int i = 0; i < _wearableManager._wearableUIs.Length; i++) {
        //  WearableUI marble = _wearableManager._wearableUIs[i];
        //  float distance = Vector3.Distance(_paintCursor.transform.position, marble.transform.position);
        //  if (!marble._isAttached) {
        //    _inDangerZone |= distance < marble.GetWorkstationDangerZoneRadius();
        //  }
        //  else {
        //    _inDangerZone |= marble.IsDisplaying && distance < marble.GetAnchoredDangerZoneRadius();
        //  }
        //}
      }
      // slightly better system
      {
        var cursorPos = _paintCursor.transform.position;
        foreach (var collider in NoPaintZone.noPaintColliders) {
          var doesCollide = collider.ClosestPoint(cursorPos).ApproxEquals(cursorPos);
          if (doesCollide) {
            _inDangerZone = true;
            break;
          }
        }
      }

      bool isUIDisplayingOnThisHand = false;
      for (int i = 0; i < _wearableManager._wearableAnchors.Length && i < 1; i++) {
        isUIDisplayingOnThisHand |= _paintCursor.Handedness == _wearableManager._wearableAnchors[i]._chirality && _wearableManager._wearableAnchors[i].IsDisplaying;
      }

      bool isLoading = _ribbonIO.IsLoading;

      bool possibleToActualize = false;
      Color drawColor = _paintCursor.Color;
      if (drawColor.a > 0.99F
        && !_inDangerZone
        && !interactionHand.isGraspingObject
        && !isUIDisplayingOnThisHand
        && _handLifetime > MIN_HAND_DRAWING_LIFETIME
        && !isLoading
        ) {
        possibleToActualize = true;
      }
      _paintCursor.NotifyPossibleToActualize(possibleToActualize);

      // Possible to begin actualizing -- if actualization already happening, this state doesn't matter

      // Currently ignored.
      //float fistStrength = 0F;
      //if (_paintCursor._handModel != null && _paintCursor._handModel.GetLeapHand() != null) {
      //  _hand = _paintCursor._handModel.GetLeapHand();
      //  //Debug.Log(Vector3.Dot(_hand.Fingers[1].Direction.ToVector3(), _hand.Direction.ToVector3())); tracking isn't accurate enough to just use index curl.
      //  fistStrength =
      //    Vector3.Dot(_hand.Fingers[2].Direction.ToVector3(), _hand.Direction.ToVector3()) +
      //    Vector3.Dot(_hand.Fingers[3].Direction.ToVector3(), _hand.Direction.ToVector3()) +
      //    Vector3.Dot(_hand.Fingers[4].Direction.ToVector3(), _hand.Direction.ToVector3());
      //}

      float angleFromCameraLookVector = Vector3.Angle(Camera.main.transform.forward, _paintCursor.transform.position - Camera.main.transform.position);
      float acceptableFOVAngle = 50F;
      bool withinAcceptableCameraFOV = angleFromCameraLookVector < acceptableFOVAngle;

      bool possibleToBeginActualizing = false;
      if (//fistStrength > -2f
        withinAcceptableCameraFOV) {
        possibleToBeginActualizing = true;
      }
      _paintCursor.NotifyPossibleToBeginActualizing(possibleToBeginActualizing);

      // Drawing State //

      if (_paintCursor.IsTracked && !_strokeProcessor.IsBufferingStroke && !_inDangerZone && possibleToActualize && possibleToBeginActualizing) {
        BeginStroke();
      }

      if (_paintCursor.DidStartPinch && possibleToActualize && possibleToBeginActualizing && !_strokeProcessor.IsActualizingStroke) {
        StartActualizingStroke();
        _paintCursor.NotifyIsPainting(true);
      }

      if (_paintCursor.IsTracked && _strokeProcessor.IsBufferingStroke) {
        UpdateStroke();
      }

      if ((!_paintCursor.IsTracked || !_paintCursor.IsPinching) && _strokeProcessor.IsActualizingStroke) {
        StopActualizingStroke();
        _paintCursor.NotifyIsPainting(false);
      }

      if ((!_paintCursor.IsTracked || (!_strokeProcessor.IsActualizingStroke && !possibleToBeginActualizing)) && _strokeProcessor.IsBufferingStroke) {
        EndStroke();
      }
    }

    private void BeginStroke() {
      _strokeProcessor.BeginTrackingStroke();
      _prevPosition = _paintCursor.Position;
    }

    private void StartActualizingStroke() {
      _strokeProcessor.StartStroke();
      _beginEffect.PlayOnTransform(_soundEffectSource.transform);
      _soundEffectSource.Play();
    }

    private List<Vector3> _cachedPoints = new List<Vector3>();
    private List<float> _cachedDeltaTimes = new List<float>();
    private void UpdateStroke() {
      float speed = Vector3.Distance(_paintCursor.Position, _prevPosition) / Time.deltaTime;
      _prevPosition = _paintCursor.Position;
      _smoothedSpeed.Update(speed, Time.deltaTime);

      float effectPercent = Mathf.Clamp01(_smoothedSpeed.value / _maxEffectSpeed);
      _soundEffectSource.volume = effectPercent * _volumeScale;
      _soundEffectSource.pitch = Mathf.Lerp(_pitchRange.x, _pitchRange.y, effectPercent);

      Vector3 strokePosition = _paintCursor.Position;

      _cachedPoints.Clear();
      _cachedDeltaTimes.Clear();
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
            _cachedPoints.Add(curPos + segment);
            _cachedDeltaTimes.Add(curDeltaTime + segmentDeltaTime);
            curPos += segment;
            curDeltaTime += segmentDeltaTime;
          }
          _cachedPoints.Add(strokePosition);
          _cachedDeltaTimes.Add(curDeltaTime + remainderDeltaTime);
          ProcessAddStrokePoints(_cachedPoints, _cachedDeltaTimes);
        }
        else {
          _cachedPoints.Add(strokePosition);
          _cachedDeltaTimes.Add(Time.deltaTime);
          ProcessAddStrokePoints(_cachedPoints, _cachedDeltaTimes);
        }
      }
      else {
        _cachedPoints.Add(strokePosition);
        _cachedDeltaTimes.Add(Time.deltaTime);
        ProcessAddStrokePoints(_cachedPoints, _cachedDeltaTimes);
      }

      if (_strokeProcessor.IsActualizingStroke) {
        drawTime += Time.deltaTime;
      }
    }

    private List<StrokePoint> _cachedStrokePoints = new List<StrokePoint>();
    private void ProcessAddStrokePoints(List<Vector3> points, List<float> effDeltaTimes) {
      if (points.Count != effDeltaTimes.Count) {
        Debug.LogError("[PinchStrokeProcessor] Points count must match effDeltaTimes count.");
        return;
      }

      _cachedStrokePoints.Clear();
      for (int i = 0; i < points.Count; i++) {
        Vector3 point = points[i];
        float effDeltaTime = effDeltaTimes[i];

        bool shouldAdd = !_firstStrokePointAdded
          || Vector3.Distance(_lastStrokePointAdded, point)
              >= Mathf.Lerp(MIN_THICKNESS_MIN_SEGMENT_LENGTH, MAX_THICKNESS_MIN_SEGMENT_LENGTH, _thicknessFilter._lastNormalizedValue);

        _timeSinceLastAddition += effDeltaTime;

        if (shouldAdd) {
          StrokePoint strokePoint = new StrokePoint();
          strokePoint.position = point;
          strokePoint.rotation = Quaternion.identity;
          strokePoint.handOrientation = _paintCursor.Rotation * Quaternion.Euler((_paintCursor.Handedness == Chirality.Left ? leftHandEulerRotation : rightHandEulerRotation));
          strokePoint.deltaTime = _timeSinceLastAddition;

          _cachedStrokePoints.Add(strokePoint);

          _firstStrokePointAdded = true;
          _lastStrokePointAdded = strokePoint.position;
          _timeSinceLastAddition = 0F;
        }
      }
      _strokeProcessor.UpdateStroke(_cachedStrokePoints);
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
      _strokeRenderer.InitializeRenderer();
      _strokeRenderer.UpdateRenderer(stroke, stroke.Count - 1);
      _strokeRenderer.FinalizeRenderer();
    }

  }


}
