//using UnityEngine;
//using System.Collections.Generic;
//using Leap.Unity;
//using System.IO;
//using System.Text;
//using UnityEngine.Events;

//[System.Serializable]
//public class StringEvent : UnityEvent<string> { }

//public class PinchRibbonDrawing : MonoBehaviour {

//  #region PUBLIC FIELDS

//  [Tooltip("When a pinch is detected, this behaviour will initiate drawing a tube at the pinch location.")]
//  public PinchDetector[] _pinchDetectors;

//  [Tooltip("Before starting drawing, this detector will be checked. If it is active, drawing won't actually begin.")]
//  public Detector _dontDrawDetector;

//  [Header("Drawing")]

//  [Tooltip("The color of newly drawn ribbons.")]
//  public Color _ribbonColor = Color.black;

//  [Tooltip("The material to assign to newly drawn ribbons.")]
//  public Material _ribbonMaterial;

//  [Tooltip("The thickness of newly drawn ribbons.")]
//  [Range(0.001F, 0.01F)]
//  public float _ribbonThickness = 0.002F;

//  [Tooltip("Higher values require longer strokes but make the strokes smoother.")]
//  private float _ribbonSmoothingDelay = 0.01F;

//  [Tooltip("Ribbons are drawn in segments; this is the minimum length of a segment before it is added to the ribbon currently being drawn.")]
//  public float _ribbonMinSegmentLength = 0.005F;

//  #endregion PUBLIC ATTRIBUTES

//  #region PRIVATE FIELDS

//  /// <summary>
//  /// DrawState objects process beginning, creating, and
//  /// finalizing the meshes from input lines as a series of
//  /// Vector3s.
//  /// </summary>
//  private DrawState _drawState;

//  /// <summary>
//  /// A growable list of GameObjects holding all of the objects
//  /// containing drawn meshes in the order in which they were
//  /// drawn.
//  /// </summary>
//  private List<GameObject> _undoHistory;

//  /// <summary>
//  /// As the user performs Undo operations, "undone" objects
//  /// are moved into Redo history to faciliate undo-ing undo operations.
//  /// 
//  /// This buffer is cleared whenever the user makes any new strokes
//  /// (and its objects are finally deleted).
//  /// </summary>
//  private List<GameObject> _redoHistory;

//  [Tooltip("Temporary fix to allow the query system to handle pinching activation for pinch-drawing.")]
//  [SerializeField]
//  private bool _startPinchRequestPending = false;

//  #endregion

//  #region PROPERTIES

//  public bool IsCurrentlyDrawing {
//    get;
//    private set;
//  }

//  #endregion

//  #region UNITY EVENTS

//  public StringEvent OnSaveSuccessful;

//  #endregion

//  #region UNITY CALLBACKS

//  void OnValidate() {
//    _ribbonThickness = Mathf.Max(0, _ribbonThickness);
//    _ribbonMinSegmentLength = Mathf.Max(0, _ribbonMinSegmentLength);
//  }

//  void Start() {
//    // Primary DrawState and undo/redo history
//    _drawState = new DrawState(this);
//    _undoHistory = new List<GameObject>();
//    _redoHistory = new List<GameObject>();
//  }

//  void Update() {
//    for (int i = 0; i < _pinchDetectors.Length; i++) {
//      var detector = _pinchDetectors[i];

//      if (detector == null) return;

//      if ((detector.IsPinching && _startPinchRequestPending) && (_dontDrawDetector != null && !_dontDrawDetector.IsActive)) {
//        IsCurrentlyDrawing = true;
//        _drawState.BeginNewLine();
//        _startPinchRequestPending = false;
//      }
//      if (detector.DidEndPinch && IsCurrentlyDrawing) {
//        _drawState.FinishLine();
//        IsCurrentlyDrawing = false;
//      }
//      if (detector.IsPinching && IsCurrentlyDrawing) {
//        _drawState.UpdateLine(detector.Position, detector.Rotation);
//      }
//    }
//  }

//  #endregion

//  #region PRIVATE METHODS

//  private void ClearRedoHistory() {
//    for (int i = 0; i < _redoHistory.Count; i++) {
//      Destroy(_redoHistory[i]);
//    }
//    _redoHistory.Clear();
//  }

//  #endregion

//  #region PUBLIC METHODS

//  /// <summary>
//  /// Hides the last-draw tube object created and adds it to the redo history.
//  /// More presses will hide older tubes.
//  /// </summary>
//  public void Undo() {
//    if (_undoHistory.Count > 0) {
//      GameObject toUndo = _undoHistory[_undoHistory.Count - 1];
//      _undoHistory.RemoveAt(_undoHistory.Count - 1);
//      toUndo.SetActive(false);
//      _redoHistory.Add(toUndo);
//    }
//  }

//  /// <summary>
//  /// Unhides the last-undone tube object and re-adds it to the undo history.
//  /// More presses will un-undo more recent tubes. Upon making a new stroke,
//  /// the redo history is cleared and its objects are deleted.
//  /// </summary>
//  public void Redo() {
//    if (_redoHistory.Count > 0) {
//      GameObject toRedo = _redoHistory[_redoHistory.Count - 1];
//      _redoHistory.RemoveAt(_redoHistory.Count - 1);
//      toRedo.SetActive(true);
//      _undoHistory.Add(toRedo);
//    }
//  }

//  /// <summary>
//  /// Sets drawn tube color.
//  /// </summary>
//  public void SetColor(Color color) {
//    _ribbonColor = color;
//  }

//  /// <summary>
//  /// Sets drawn tube thickness. Expects a value from 0 (min) to 1 (max).
//  /// </summary>
//  public void SetThickness(float normalizedThickness) {
//    _ribbonThickness = Mathf.Lerp(0.001F, 0.01F, normalizedThickness);
//  }

//  /// <summary>
//  /// Sets the tube smoothing delay. Expects a value from 0 (min) to 1 (max);
//  /// </summary>
//  public void SetSmoothing(float normalizedSmoothing) {
//    _ribbonSmoothingDelay = Mathf.Lerp(0F, 0.15F, normalizedSmoothing);
//  }

//  //public void Save(string filePath) {
//  //  SavedScene toSave = new SavedScene();
//  //  toSave._tubeStrokes = new TubeStroke[_tubeStrokes.Count];
//  //  for (int i = 0; i < _tubeStrokes.Count; i++) {
//  //    toSave._tubeStrokes[i] = _tubeStrokes[i];
//  //  }

//  //  string savedSceneJSON = toSave.WriteToJSON();

//  //  if (File.Exists(filePath)) {
//  //    File.Delete(filePath);
//  //  }
//  //  using (StreamWriter writer = File.CreateText(filePath)) {
//  //    writer.Write(savedSceneJSON);
//  //  }

//  //  OnSaveSuccessful.Invoke("Saved " + Path.GetFileName(filePath));
//  //}

//  //public void Load(string filePath) {
//  //  // Clear all strokes -- TODO: make this not just use undo
//  //  for (int i = 0; i < _undoHistory.Count; i++) {
//  //    Undo();
//  //  }
//  //  ClearRedoHistory();

//  //  // Get JSON string from filename
//  //  StreamReader reader = new StreamReader(filePath);
//  //  Debug.Log("Trying to read path: " + filePath);
//  //  string json = reader.ReadToEnd();

//  //  // Load SavedScene object from JSON
//  //  SavedScene savedScene = SavedScene.CreateFromJSON(json);

//  //  // Recreate each tube from stroke data
//  //  for (int i = 0; i < savedScene._tubeStrokes.Length; i++) {
//  //    TubeStroke tubeStroke = savedScene._tubeStrokes[i];

//  //    _tubeRadius = tubeStroke._radius;
//  //    _ribbonColor = tubeStroke._color;
//  //    _tubeResolution = tubeStroke._resolution;
//  //    _ribbonSmoothingDelay = tubeStroke._smoothingDelay;
//  //    _drawState.BeginNewLine();
//  //    for (int j = 0; j < tubeStroke._strokePoints.Count; j++) {
//  //      _drawState.UpdateLine(tubeStroke._strokePoints[j], tubeStroke._strokePointDeltaTimes[j]);
//  //    }
//  //    _drawState.FinishLine();
//  //  }
//  //}

//  // TODO: integrate pinch drawing with query system so this is unnecessary
//  public void StartPinchDrawRequest() {
//    _startPinchRequestPending = true;
//  }

//  #endregion

//  #region DEBUG OUTPUT (TODO DELETEME)

//  // TODO: REMOVE SYSTEM.IO DEPENDENCY
//  // TODO: REMOVE TEXT DEPENDENCY

//  // TODO: DELETEME
//  // Recording a line for the preview display
//  public List<Vector3> points = new List<Vector3>();
//  public List<float> deltaTimes = new List<float>();
//  public void RecordPoint(Vector3 point) {
//    points.Add(point);
//  }
//  public void RecordDeltaTime(float deltaTime) {
//    deltaTimes.Add(deltaTime);
//  }
//  public void OutputPoints() {
//    return; // comment out to get output.txt

//    // TODO: DELETEME DEBUG STROKE OUTPUT
//    if (File.Exists("./output.txt")) {
//      File.Delete("./output.txt");
//    }
//    StreamWriter writer = new StreamWriter("./output.txt");
//    StringBuilder line = new StringBuilder();
//    for (int i = 0; i < points.Count; i++) {
//      Vector3 vec = points[i];
//      line.Append("new Vector3(");
//      line.Append(vec.x.ToString("G6") + "F, ");
//      line.Append(vec.y.ToString("G6") + "F, ");
//      line.Append(vec.z.ToString("G6") + "F)");
//      if (i != points.Count - 1) line.Append(",");
//      line.Append("\n");
//    }
//    line.Append("\n");
//    for (int i = 0; i < points.Count; i++) {
//      float dt = deltaTimes[i];
//      line.Append(dt + "F");
//      if (i != points.Count - 1) line.Append(",");
//      line.Append("\n");
//    }
//    writer.Write(line.ToString());
//    writer.Close();

//    points.Clear();
//    deltaTimes.Clear();
//  }

//  #endregion

//  #region DRAWSTATE

//  private class DrawState {
//    private List<Vector3> _vertices = new List<Vector3>();
//    private List<int> _tris = new List<int>();
//    private List<Vector2> _uvs = new List<Vector2>();
//    private List<Color> _colors = new List<Color>();

//    private PinchDrawing _parent;

//    private int _rings = 0;

//    private Vector3 _prevRing0 = Vector3.zero;
//    private Vector3 _prevRing1 = Vector3.zero;

//    private Vector3 _prevNormal0 = Vector3.zero;

//    private Mesh _mesh;
//    private SmoothedVector3 _smoothedPosition;

//    // quick-and-dirty I/O support
//    private TubeStroke _curTubeStroke = null;

//    // matching I/O support with undo history
//    private GameObject _lastLineObj = null;

//    public DrawState(PinchDrawing parent) {
//      _parent = parent;

//      _smoothedPosition = new SmoothedVector3();
//      _smoothedPosition.delay = parent._ribbonSmoothingDelay;
//      _smoothedPosition.reset = true;
//    }

//    public GameObject BeginNewLine() {

//      _rings = 0;
//      _vertices.Clear();
//      _tris.Clear();
//      _uvs.Clear();
//      _colors.Clear();

//      _smoothedPosition.reset = true;
//      _smoothedPosition.delay = _parent._ribbonSmoothingDelay;

//      _mesh = new Mesh();
//      _mesh.name = "Line Mesh";
//      _mesh.MarkDynamic();

//      GameObject lineObj = new GameObject("Line Object");
//      lineObj.transform.position = Vector3.zero;
//      lineObj.transform.rotation = Quaternion.identity;
//      lineObj.transform.localScale = Vector3.one;
//      lineObj.AddComponent<MeshFilter>().mesh = _mesh;
//      lineObj.AddComponent<MeshRenderer>().sharedMaterial = _parent._ribbonMaterial;

//      // quick-and-dirty I/O support
//      if (this == _parent._drawState) { // ignore _previewDrawState
//        _curTubeStroke = new TubeStroke();
//        _curTubeStroke._radius = _parent._tubeRadius;
//        _curTubeStroke._color = _parent._ribbonColor;
//        _curTubeStroke._resolution = _parent._tubeResolution;
//        _curTubeStroke._smoothingDelay = _parent._ribbonSmoothingDelay;
//      }

//      _lastLineObj = lineObj;
//      return lineObj;
//    }

//    /// <summary>
//    /// Updates the line currently being drawn with a new position and an argument
//    /// deltaTime between the new position and the last.
//    /// Manually specifying a time is probably only necessary if you're programmatically
//    /// reconstructing a stroke; normal interactions will call UpdateLine(Vector3), which
//    /// will just use Time.deltaTime.
//    /// </summary>
//    public void UpdateLine(Vector3 position, float deltaTime) {

//      // TODO: DELETEME
//      // Recording a line for the preview display
//      _parent.RecordPoint(position);
//      _parent.RecordDeltaTime(deltaTime);

//      // quick-and-dirty I/O support
//      if (this == _parent._drawState) { // ignore _previewDrawState
//        _curTubeStroke.RecordStrokePoint(position, deltaTime);
//      }

//      _smoothedPosition.Update(position, deltaTime);

//      bool shouldAdd = false;

//      shouldAdd |= _vertices.Count == 0;
//      shouldAdd |= Vector3.Distance(_prevRing0, _smoothedPosition.value) >= _parent._ribbonMinSegmentLength;
//      shouldAdd |= (this == _parent._previewDrawState && Vector3.Distance(_prevRing0, _smoothedPosition.value) >= _parent._ribbonMinSegmentLength * _parent._previewStrokeScalingFactor);

//      if (shouldAdd) {
//        addRing(_smoothedPosition.value);
//        updateMesh();
//      }
//    }
//    public void UpdateLine(Vector3 position) {
//      UpdateLine(position, Time.deltaTime);
//    }

//    public void FinishLine() {
//      // TODO: DELETEME (outputting last drawn line)
//      _parent.OutputPoints();

//      // quick-and-dirty I/O support
//      if (this == _parent._drawState) { // ignore preview drawstate things
//        _parent._tubeStrokes.Add(_curTubeStroke);
//        _parent._undoHistory.Add(_lastLineObj);
//        _parent.ClearRedoHistory();
//      }

//      _mesh.Optimize();
//      _mesh.UploadMeshData(true);
//    }

//    private void updateMesh() {
//      _mesh.SetVertices(_vertices);
//      _mesh.SetColors(_colors);
//      _mesh.SetUVs(0, _uvs);
//      _mesh.SetIndices(_tris.ToArray(), MeshTopology.Triangles, 0);
//      _mesh.RecalculateBounds();
//      _mesh.RecalculateNormals();
//    }

//    private void addRing(Vector3 ringPosition) {
//      _rings++;

//      if (_rings == 1) {
//        addVertexRing();
//        addVertexRing();
//        addTriSegment();
//      }

//      addVertexRing();
//      addTriSegment();

//      Vector3 ringNormal = Vector3.zero;
//      if (_rings == 2) {
//        Vector3 direction = ringPosition - _prevRing0;
//        float angleToUp = Vector3.Angle(direction, Vector3.up);

//        if (angleToUp < 10 || angleToUp > 170) {
//          ringNormal = Vector3.Cross(direction, Vector3.right);
//        }
//        else {
//          ringNormal = Vector3.Cross(direction, Vector3.up);
//        }

//        ringNormal = ringNormal.normalized;

//        _prevNormal0 = ringNormal;
//      }
//      else if (_rings > 2) {
//        Vector3 prevPerp = Vector3.Cross(_prevRing0 - _prevRing1, _prevNormal0);
//        ringNormal = Vector3.Cross(prevPerp, ringPosition - _prevRing0).normalized;
//      }

//      if (_rings == 2) {
//        updateRingVerts(0,
//                        _prevRing0,
//                        ringPosition - _prevRing1,
//                        _prevNormal0,
//                        0);
//      }

//      if (_rings >= 2) {
//        updateRingVerts(_vertices.Count - _parent._tubeResolution,
//                        ringPosition,
//                        ringPosition - _prevRing0,
//                        ringNormal,
//                        0);
//        updateRingVerts(_vertices.Count - _parent._tubeResolution * 2,
//                        ringPosition,
//                        ringPosition - _prevRing0,
//                        ringNormal,
//                        1);
//        updateRingVerts(_vertices.Count - _parent._tubeResolution * 3,
//                        _prevRing0,
//                        ringPosition - _prevRing1,
//                        _prevNormal0,
//                        1);
//      }

//      _prevRing1 = _prevRing0;
//      _prevRing0 = ringPosition;

//      _prevNormal0 = ringNormal;
//    }

//    private void addVertexRing() {
//      for (int i = 0; i < _parent._tubeResolution; i++) {
//        _vertices.Add(Vector3.zero);  //Dummy vertex, is updated later
//        _uvs.Add(new Vector2(i / (_parent._tubeResolution - 1.0f), 0));
//        _colors.Add(_parent._ribbonColor);
//      }
//    }

//    //Connects the most recently added vertex ring to the one before it
//    private void addTriSegment() {
//      for (int i = 0; i < _parent._tubeResolution; i++) {
//        int i0 = _vertices.Count - 1 - i;
//        int i1 = _vertices.Count - 1 - ((i + 1) % _parent._tubeResolution);

//        _tris.Add(i0);
//        _tris.Add(i1 - _parent._tubeResolution);
//        _tris.Add(i0 - _parent._tubeResolution);

//        _tris.Add(i0);
//        _tris.Add(i1);
//        _tris.Add(i1 - _parent._tubeResolution);
//      }
//    }

//    private void updateRingVerts(int offset, Vector3 ringPosition, Vector3 direction, Vector3 normal, float radiusScale) {
//      direction = direction.normalized;
//      normal = normal.normalized;

//      for (int i = 0; i < _parent._tubeResolution; i++) {
//        float angle = 360.0f * (i / (float)(_parent._tubeResolution));
//        Quaternion rotator = Quaternion.AngleAxis(angle, direction);
//        Vector3 ringSpoke = rotator * normal * _parent._tubeRadius * radiusScale;
//        _vertices[offset + i] = ringPosition + ringSpoke;
//      }
//    }
//  }

//  #endregion

//}