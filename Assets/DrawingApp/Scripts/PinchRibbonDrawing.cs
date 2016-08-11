using UnityEngine;
using System.Collections.Generic;
using Leap.Unity;
using System.IO;
using System.Text;
using UnityEngine.Events;
using MeshGeneration;

public class PinchRibbonDrawing : MonoBehaviour {

  #region PUBLIC FIELDS

  [Tooltip("When a pinch is detected, this behaviour will initiate drawing a tube at the pinch location.")]
  public PinchDetector[] _pinchDetectors;

  [Tooltip("Before starting drawing, this detector will be checked. If it is active, drawing won't actually begin.")]
  public Detector _dontDrawDetector;

  [Header("Drawing")]

  [Tooltip("The color of newly drawn ribbons.")]
  public Color _ribbonColor = Color.red;

  [Tooltip("The material to assign to newly drawn ribbons.")]
  public Material _ribbonMaterial;

  [Tooltip("The thickness of newly drawn ribbons.")]
  [Range(0.001F, 0.01F)]
  public float _ribbonRadius = 0.002F;

  [Tooltip("Higher values require longer strokes but make the strokes smoother.")]
  [Range(0.01F, 0.15F)]
  public float _ribbonSmoothingDelay = 0.01F;

  [Tooltip("Makes ribbon rotation smoother but slower.")]
  [Range(0.01F, 0.15F)]
  public float _ribbonRotationSmoothingDelay = 0.01F;

  [Tooltip("Ribbons are drawn in segments; this is the minimum length of a segment before it is added to the ribbon currently being drawn.")]
  public float _ribbonMinSegmentLength = 0.005F;

  #endregion PUBLIC ATTRIBUTES

  #region PRIVATE FIELDS

  /// <summary>
  /// DrawState objects process beginning, creating, and
  /// finalizing the meshes from input lines as a series of
  /// Vector3s.
  /// </summary>
  private DrawState _drawState;

  /// <summary>
  /// A growable list of GameObjects holding all of the objects
  /// containing drawn meshes in the order in which they were
  /// drawn.
  /// </summary>
  private List<GameObject> _undoHistory;

  /// <summary>
  /// As the user performs Undo operations, "undone" objects
  /// are moved into Redo history to faciliate undo-ing undo operations.
  /// 
  /// This buffer is cleared whenever the user makes any new strokes
  /// (and its objects are finally deleted).
  /// </summary>
  private List<GameObject> _redoHistory;

  // Stroke history data aligned with undoHistory for saving/loading.
  private List<RibbonStroke> _ribbonStrokes = new List<RibbonStroke>();
  private List<RibbonStroke> _undoneRibbonStrokes = new List<RibbonStroke>();

  [Tooltip("Temporary fix to allow the query system to handle pinching activation for pinch-drawing.")]
  [SerializeField]
  private bool _startPinchRequestPending = false;

  #endregion

  #region PROPERTIES

  public bool IsCurrentlyDrawing {
    get;
    private set;
  }

  #endregion

  #region UNITY EVENTS

  public StringEvent OnSaveSuccessful;

  #endregion

  #region UNITY CALLBACKS

  void OnValidate() {
    _ribbonRadius = Mathf.Max(0, _ribbonRadius);
    _ribbonMinSegmentLength = Mathf.Max(0, _ribbonMinSegmentLength);
  }

  void Start() {
    // Primary DrawState and undo/redo history
    _drawState = new DrawState(this);
    _undoHistory = new List<GameObject>();
    _redoHistory = new List<GameObject>();
  }

  void Update() {
    for (int i = 0; i < _pinchDetectors.Length; i++) {
      var detector = _pinchDetectors[i];

      if (detector == null) return;

      if ((detector.IsPinching && !IsCurrentlyDrawing && _startPinchRequestPending) && (_dontDrawDetector == null || (_dontDrawDetector != null && !_dontDrawDetector.IsActive))) {
        IsCurrentlyDrawing = true;
        _drawState.BeginNewLine();
        _startPinchRequestPending = false;
      }
      if (detector.DidEndPinch && IsCurrentlyDrawing) {
        _drawState.FinishLine();
        IsCurrentlyDrawing = false;
      }
      if (detector.IsPinching && IsCurrentlyDrawing) {
        _drawState.UpdateLine(detector.Position, detector.Rotation);
      }
    }
  }

  #endregion

  #region PRIVATE METHODS

  private void ClearRedoHistory() {
    for (int i = 0; i < _redoHistory.Count; i++) {
      Destroy(_redoHistory[i]);
    }
    _redoHistory.Clear();

    // TubeStroke undo tracking
    _undoneRibbonStrokes.Clear();
  }

  #endregion

  #region PUBLIC METHODS

  /// <summary>
  /// Hides the last-draw tube object created and adds it to the redo history.
  /// More presses will hide older tubes.
  /// </summary>
  public void Undo() {
    if (_undoHistory.Count > 0) {
      GameObject toUndo = _undoHistory[_undoHistory.Count - 1];
      _undoHistory.RemoveAt(_undoHistory.Count - 1);
      toUndo.SetActive(false);
      _redoHistory.Add(toUndo);

      // TubeStroke undo tracking
      _undoneRibbonStrokes.Add(_ribbonStrokes[_undoHistory.Count]);
      _ribbonStrokes.RemoveAt(_undoHistory.Count);
    }
  }

  /// <summary>
  /// Unhides the last-undone tube object and re-adds it to the undo history.
  /// More presses will un-undo more recent tubes. Upon making a new stroke,
  /// the redo history is cleared and its objects are deleted.
  /// </summary>
  public void Redo() {
    if (_redoHistory.Count > 0) {
      GameObject toRedo = _redoHistory[_redoHistory.Count - 1];
      _redoHistory.RemoveAt(_redoHistory.Count - 1);
      toRedo.SetActive(true);
      _undoHistory.Add(toRedo);

      // TubeStroke redo tracking
      _ribbonStrokes.Add(_undoneRibbonStrokes[_redoHistory.Count]);
      _undoneRibbonStrokes.RemoveAt(_redoHistory.Count);
    }
  }

  /// <summary>
  /// Sets drawn tube color.
  /// </summary>
  public void SetColor(Color color) {
    _ribbonColor = color;
  }

  /// <summary>
  /// Sets drawn tube thickness. Expects a value from 0 (min) to 1 (max).
  /// </summary>
  public void SetThickness(float normalizedThickness) {
    _ribbonRadius = Mathf.Lerp(0.001F, 0.01F, normalizedThickness);
  }

  /// <summary>
  /// Sets the tube smoothing delay. Expects a value from 0 (min) to 1 (max);
  /// </summary>
  public void SetSmoothing(float normalizedSmoothing) {
    _ribbonSmoothingDelay = Mathf.Lerp(0F, 0.15F, normalizedSmoothing);
  }

  public void Save(string filePath) {
    SavedScene toSave = new SavedScene();
    toSave._ribbonStrokes = new RibbonStroke[_ribbonStrokes.Count];

    for (int i = 0; i < _ribbonStrokes.Count; i++) {
      toSave._ribbonStrokes[i] = _ribbonStrokes[i];
    }

    string savedSceneJSON = toSave.WriteToJSON();

    if (File.Exists(filePath)) {
      File.Delete(filePath);
    }
    using (StreamWriter writer = File.CreateText(filePath)) {
      writer.Write(savedSceneJSON);
    }

    OnSaveSuccessful.Invoke("Saved " + Path.GetFileName(filePath));
  }

  public void Load(string filePath) {
    // Clear all strokes -- TODO: make this not just use undo
    for (int i = 0; i < _undoHistory.Count; i++) {
      Undo();
    }
    ClearRedoHistory();

    // Get JSON string from filename
    StreamReader reader = new StreamReader(filePath);
    Debug.Log("Trying to read path: " + filePath);
    string json = reader.ReadToEnd();

    // Load SavedScene object from JSON
    SavedScene savedScene = SavedScene.CreateFromJSON(json);

    // Recreate each tube from stroke data
    for (int i = 0; i < savedScene._ribbonStrokes.Length; i++) {
      RibbonStroke ribbonStroke = savedScene._ribbonStrokes[i];

      _ribbonRadius = ribbonStroke._radius;
      _ribbonColor = ribbonStroke._color;
      _ribbonSmoothingDelay = ribbonStroke._smoothingDelay;
      _ribbonRotationSmoothingDelay = ribbonStroke._rotationSmoothingDelay;
      _drawState.BeginNewLine();
      for (int j = 0; j < ribbonStroke._strokePoints.Count; j++) {
        _drawState.UpdateLine(ribbonStroke._strokePoints[j], ribbonStroke._strokePointRotations[j], ribbonStroke._strokePointDeltaTimes[j]);
      }
      _drawState.FinishLine();
    }
  }

  // TODO: integrate pinch drawing with query system so this is unnecessary
  public void StartPinchDrawRequest() {
    _startPinchRequestPending = true;
  }

  #endregion

  #region DEBUG OUTPUT (TODO DELETEME)

  // TODO: REMOVE SYSTEM.IO DEPENDENCY
  // TODO: REMOVE TEXT DEPENDENCY

  // TODO: DELETEME
  // Recording a line for the preview display
  public List<Vector3> points = new List<Vector3>();
  public List<float> deltaTimes = new List<float>();
  public void RecordPoint(Vector3 point) {
    points.Add(point);
  }
  public void RecordDeltaTime(float deltaTime) {
    deltaTimes.Add(deltaTime);
  }
  public void OutputPoints() {
    return; // comment out to get output.txt

    // TODO: DELETEME DEBUG STROKE OUTPUT
    if (File.Exists("./output.txt")) {
      File.Delete("./output.txt");
    }
    StreamWriter writer = new StreamWriter("./output.txt");
    StringBuilder line = new StringBuilder();
    for (int i = 0; i < points.Count; i++) {
      Vector3 vec = points[i];
      line.Append("new Vector3(");
      line.Append(vec.x.ToString("G6") + "F, ");
      line.Append(vec.y.ToString("G6") + "F, ");
      line.Append(vec.z.ToString("G6") + "F)");
      if (i != points.Count - 1) line.Append(",");
      line.Append("\n");
    }
    line.Append("\n");
    for (int i = 0; i < points.Count; i++) {
      float dt = deltaTimes[i];
      line.Append(dt + "F");
      if (i != points.Count - 1) line.Append(",");
      line.Append("\n");
    }
    writer.Write(line.ToString());
    writer.Close();

    points.Clear();
    deltaTimes.Clear();
  }

  #endregion

  #region DRAWSTATE

  private class DrawState {

    private PinchRibbonDrawing _parent;
    
    private Ribbon _ribbon;
    private Mesh _mesh;
    private GameObject _gameObject;

    private SmoothedVector3 _smoothedPosition;
    private SmoothedQuaternion _smoothedRotation;

    private List<Vector3> _addedStrokePoints = new List<Vector3>();
    private Vector3 _lastAddedPosition;

    // quick-and-dirty I/O support
    private RibbonStroke _curRibbonStroke = null;

    public DrawState(PinchRibbonDrawing parent) {
      _parent = parent;

      _smoothedPosition = new SmoothedVector3();
      _smoothedPosition.delay = _parent._ribbonSmoothingDelay;
      _smoothedPosition.reset = true;

      _smoothedRotation = new SmoothedQuaternion();
      _smoothedRotation.delay = _parent._ribbonRotationSmoothingDelay;
      _smoothedRotation.reset = true;
    }

    private void UpdateMesh() {
      ShapeCombiner c = new ShapeCombiner(65536, shouldOptimize: false, shouldUpload: false, infiniteBounds: false);
      c.AddShape(_ribbon);
      c.FinalizeCurrentMesh();
      Mesh ribbonMesh = c.GetLastMesh();
      if (ribbonMesh != null) {
        _mesh.vertices = ribbonMesh.vertices;
        _mesh.colors = ribbonMesh.colors;
        _mesh.uv = ribbonMesh.uv;
        _mesh.SetIndices(ribbonMesh.triangles, MeshTopology.Triangles, 0);
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
      }
    }

    public GameObject BeginNewLine() {
      _addedStrokePoints.Clear();
      _lastAddedPosition = Vector3.zero;

      _ribbon = new Ribbon();

      _smoothedPosition.reset = true;
      _smoothedPosition.delay = _parent._ribbonSmoothingDelay;

      _smoothedRotation.reset = true;
      _smoothedRotation.delay = _parent._ribbonRotationSmoothingDelay;
      
      _mesh = new Mesh();
      _mesh.name = "Ribbon Mesh";
      _mesh.MarkDynamic();

      _gameObject = new GameObject();
      _gameObject.transform.position = Vector3.zero;
      _gameObject.transform.rotation = Quaternion.identity;
      _gameObject.transform.localScale = Vector3.one;
      _gameObject.AddComponent<MeshFilter>().mesh = _mesh;
      _gameObject.AddComponent<MeshRenderer>().sharedMaterial = _parent._ribbonMaterial;

      // quick-and-dirty I/O support
      if (this == _parent._drawState) { // ignore _previewDrawState
        _curRibbonStroke = new RibbonStroke();
        _curRibbonStroke._radius = _parent._ribbonRadius;
        _curRibbonStroke._color = _parent._ribbonColor;
        _curRibbonStroke._smoothingDelay = _parent._ribbonSmoothingDelay;
        _curRibbonStroke._rotationSmoothingDelay = _parent._ribbonSmoothingDelay;
      }

      return _gameObject;
    }

    public void UpdateLine(Vector3 position, Quaternion rotation, float deltaTime) {

      // quick-and-dirty I/O support
      if (this == _parent._drawState) { // ignore _previewDrawState
        _curRibbonStroke.RecordStrokePoint(position, rotation, deltaTime);
      }

      _smoothedPosition.Update(position, deltaTime);
      _smoothedRotation.Update(rotation, deltaTime);

      bool shouldAdd = false;

      shouldAdd |= _addedStrokePoints.Count == 0;
      shouldAdd |= Vector3.Distance(_lastAddedPosition, _smoothedPosition.value) >= _parent._ribbonMinSegmentLength;

      if (shouldAdd) {
        MeshPoint newMeshPoint = new MeshPoint(_smoothedPosition.value);
        newMeshPoint.Normal = _smoothedRotation.value * Vector3.up;
        newMeshPoint.Color = _parent._ribbonColor;
        _ribbon.Add(newMeshPoint, _parent._ribbonRadius);
        _addedStrokePoints.Add(_smoothedPosition.value);
        UpdateMesh();
      }
    }

    public void UpdateLine(Vector3 position, Quaternion rotation) {
      UpdateLine(position, rotation, Time.deltaTime);
    }

    public void FinishLine() {
      // quick-and-dirty I/O support
      if (this == _parent._drawState) { // ignore preview drawstate things
        _parent._ribbonStrokes.Add(_curRibbonStroke);
        _parent._undoHistory.Add(_gameObject);
        _parent.ClearRedoHistory();
      }

      _mesh.Optimize();
      _mesh.UploadMeshData(true);
    }

  }

  #endregion

}