using UnityEngine;
using System.Collections.Generic;
using MeshGeneration;
using System;

public class StrokeMeshGenerator {

  public Action<Mesh> OnMeshChanged;

  public Color Color { get; set; }
  public float Thickness { get; set; }
  public StrokeMeshMode Mode { get; set; }

  public StrokeMeshGenerator SetMode(StrokeMeshMode mode) {
    this.Mode = mode;
    return this;
  }
  public StrokeMeshGenerator SetColor(Color color) {
    this.Color = color;
    return this;
  }
  public StrokeMeshGenerator SetThickness(float thickness) {
    this.Thickness = thickness;
    return this;
  }

  #region Stroke State

  // Smoothing config
  private const int POS_SMOOTH_RADIUS = 4;
  private const int ROT_SMOOTH_RADIUS = 16;
  private const float MAX_ROT_DELTA_PER_POINT = 20F; // degrees

  private List<MeshPoint> _rawPoints = new List<MeshPoint>();
  private List<MeshPoint> _smoothPoints = new List<MeshPoint>();
  private List<Quaternion> _handRotations = new List<Quaternion>();
  private List<bool> _flipRegisters = new List<bool>();
  private Quaternion _lastHandRotation = Quaternion.identity;
  private int _length = 0;

  public List<MeshPoint> GetSmoothMeshPoints() {
    return _smoothPoints;
  }

  public List<bool> GetFlipRegisters() {
    return _flipRegisters;
  }

  public void BeginStrokeMesh() {
    InitMeshState();

    _rawPoints.Clear();
    _smoothPoints.Clear();
    _handRotations.Clear();
    _lastHandRotation = Quaternion.identity;
    _length = 0;

    _flipRegisters.Clear();
  }

  public void AddStrokeMeshPoint(Vector3 position, Quaternion handRotation) {

    // Add the handRotation to keep track of them.
    _handRotations.Add(handRotation);

    // Add the raw point position as a MeshPoint. (No normals yet.)
    MeshPoint rawPoint = new MeshPoint(position);
    _rawPoints.Add(rawPoint);

    // Calculate smoothed points from raw points.
    if (_length == 0) {
      _smoothPoints.Add(_rawPoints[0]);
    }
    else if (_length == 1) {
      _smoothPoints[0] = _rawPoints[0];
      _smoothPoints.Add(_rawPoints[1]);
    }
    else {
      _smoothPoints.Add(rawPoint);

      Vector3 smoothedPos = Vector3.zero;
      for (int distFromEnd = POS_SMOOTH_RADIUS; distFromEnd >= 0; distFromEnd--) {
        int index = _length - 1 - distFromEnd;
        if (index < 0) continue;

        smoothedPos = CalcNeighborAverage(index, distFromEnd);

        MeshPoint smoothP = _rawPoints[index];
        smoothP.Position = smoothedPos;
        _smoothPoints[index] = smoothP;
      }
    }

    _length += 1;
    Debug.Assert(_length == _rawPoints.Count && _length == _smoothPoints.Count && _length == _handRotations.Count);

    // Use smoothed positions to calculate normals and assign them to the MeshPoints.
    if (_length == 1) {
      Vector3 initNormal = handRotation * Vector3.back; // Will be recalculated next time a point is added.

      MeshPoint p0 = _rawPoints[0];
      p0.Normal = initNormal;
      _rawPoints[0] = p0;
      _smoothPoints[0] = p0;
    }
    else {
      if (_length == 2) {
        // Recalculate a good normal for the first point.
        Vector3 firstVelocity = _smoothPoints[1].Position - _smoothPoints[0].Position;
        bool flipRegisterState = false;
        _flipRegisters.Add(false);
        Vector3 desiredFirstNormal = CalculateDesiredNormal(firstVelocity, _lastHandRotation, _flipRegisters[0]);
        _flipRegisters[0] = flipRegisterState;

        MeshPoint p0 = _rawPoints[0];
        p0.Normal = desiredFirstNormal;
        _rawPoints[0] = p0;
        _smoothPoints[0] = p0;
      }

      // Recalculate normals for the last N points and assign them.
      int numSmoothNormalsCalculated = 0;
      if (_flipRegisters.Count < _length) {
        _flipRegisters.Add(_flipRegisters[_flipRegisters.Count - 1]);
      }
      Vector3 prevDesiredNormal = (_length - 1 - ROT_SMOOTH_RADIUS - 1 < 0 ? _smoothPoints[0].Normal : _smoothPoints[_length - 1 - ROT_SMOOTH_RADIUS - 1].Normal);
      Vector3[] latestNSmoothNormals = CalculateDesiredNormalsForLastNSmoothedPoints(ROT_SMOOTH_RADIUS, out numSmoothNormalsCalculated, prevDesiredNormal);
      int smoothNormalsIdx = 0;
      for (int idx = _length - 1 - numSmoothNormalsCalculated; idx < _length - 1; idx++) {

        Vector3 normal = latestNSmoothNormals[smoothNormalsIdx];

        MeshPoint rawP = _rawPoints[idx];
        rawP.Normal = normal;
        _rawPoints[idx] = rawP;

        MeshPoint smoothP = _smoothPoints[idx];
        smoothP.Normal = normal;
        _smoothPoints[idx] = smoothP;

        smoothNormalsIdx++;
      }

      // Last index normal
      if (_length > 1) {
        MeshPoint smoothP = _smoothPoints[_length - 1];
        smoothP.Normal = _smoothPoints[_length - 2].Normal;
        _smoothPoints[_length - 1] = smoothP;
      }
    }

    _lastHandRotation = handRotation;

    RefreshMesh();
  }

  private Vector3[] CalculateDesiredNormalsForLastNSmoothedPoints(int N, out int numSmoothNormalsCalculated, Vector3 prevDesiredNormal) {
    Vector3[] desiredNormals = new Vector3[N];
    numSmoothNormalsCalculated = 0;

    bool curFlipRegister = false;

    Vector3 lastDesiredNormal = prevDesiredNormal;

    int desiredNormalArrIdx = 0;
    for (int idx = _length - 1 - N; idx < _length - 1; idx++) {
      if (idx < 1) continue;

      curFlipRegister = _flipRegisters[idx];
      Vector3 segmentVector = _smoothPoints[idx].Position - _smoothPoints[idx - 1].Position;
      Vector3 desiredNormal = CalculateDesiredNormal(segmentVector, _handRotations[idx], curFlipRegister);

      float dotNormal = Vector3.Dot(lastDesiredNormal, desiredNormal);
      Vector3 nextSegmentVector = _smoothPoints[idx + 1].Position - _smoothPoints[idx].Position;
      float dotSegment = Vector3.Dot(segmentVector.normalized, nextSegmentVector.normalized);
      if (dotNormal < 0F && dotSegment > 0F) {
        curFlipRegister = !curFlipRegister;
        _flipRegisters[idx] = curFlipRegister;
      }
      desiredNormal = Vector3.RotateTowards(lastDesiredNormal, desiredNormal, Mathf.Acos(dotSegment), 0F);

      lastDesiredNormal = desiredNormal;
      desiredNormals[desiredNormalArrIdx++] = desiredNormal;
      numSmoothNormalsCalculated++;
      
    }

    return desiredNormals;
  }

  // strokeVelocity in world space
  private Vector3 CalculateDesiredNormal(Vector3 strokeVelocity, Quaternion handRotation, bool shouldFlip) {
    Vector3 velocityInHandSpace = (Quaternion.Inverse(handRotation) * strokeVelocity).normalized;
    // Pretend X velocity is actually Y velocity in the hand's space for the purposes of ribbon normal calculation.
    Vector3 effNormalCalcVelocityInHandSpace = new Vector3(0F, LargerAbs(velocityInHandSpace.x, velocityInHandSpace.y), velocityInHandSpace.z).normalized;
    Vector3 effNormalCalcVelInWorldSpace = handRotation * effNormalCalcVelocityInHandSpace;

    Vector3 desiredNormal = Vector3.Cross(effNormalCalcVelInWorldSpace, handRotation * Vector3.right).normalized;
    return (shouldFlip ? -1 : 1) * desiredNormal;
  }

  private static float LargerAbs(float one, float two) {
    if (Mathf.Abs(one) > Mathf.Abs(two)) {
      return one;
    }
    else return two;
  }

  private Vector3 CalcNeighborAverage(int index, int R) {
    Vector3 neighborSum = Vector3.zero;
    int numPointsInRadius = 0;
    for (int r = -R; r <= R; r++) {
      if (index + r < 0) continue;
      else if (index + r > _length - 1) continue;
      else {
        neighborSum += _rawPoints[index + r].Position;
        numPointsInRadius += 1;
      }
    }
    if (numPointsInRadius == 0) {
      return _rawPoints[index].Position;
    }
    else {
      return neighborSum / numPointsInRadius;
    }
  }

  #endregion

  #region Mesh State

  private Mesh _mesh;
  public Mesh Mesh { get { return _mesh; } }

  private TwoSidedRibbon _ribbon = new TwoSidedRibbon();
  public Tube _tube = new Tube(8);

  private void InitMeshState() {
    _mesh = new Mesh();
    _mesh.MarkDynamic();
    _ribbon.Clear();
  }

  public void Clear() {
    InitMeshState();
  }

  public void RefreshMesh() {
    if (Mode == StrokeMeshMode.Ribbons) {
      RefreshRibbonMesh();
    }
    else {
      RefreshTubeMesh();
    }
  }

  public void RefreshRibbonMesh() {
    int startIdx = Mathf.Max(0, _length - 1 - Mathf.Max(POS_SMOOTH_RADIUS, ROT_SMOOTH_RADIUS));
    int endIdx = _length - 1;
    for (int i = startIdx; i <= endIdx; i++) {
      MeshPoint point = _smoothPoints[i];
      point.Color = this.Color;

      if (i > _ribbon.Points.Count - 1) {
        _ribbon.Add(point, this.Thickness);
      }
      else {
        _ribbon.Points[i] = point;
      }
    }

    SetMeshDataFromRibbon(_mesh, _ribbon);
    OnMeshChanged(_mesh);
  }

  public void RefreshTubeMesh() {
    Debug.Log("Tubes not yet supported.");
  }

  public Mesh FinalizeMesh() {
    _mesh.RecalculateBounds();
    ;
    _mesh.UploadMeshData(true);
    return _mesh;
  }

  List<Vector3> _cachedVec3 = new List<Vector3>();
  List<Vector2> _cachedVec2 = new List<Vector2>();
  List<Color> _cachedColor = new List<Color>();
  MeshPoints _meshPoints = new MeshPoints();
  List<int> _meshIndices = new List<int>();
  private void SetMeshDataFromRibbon(Mesh mesh, Ribbon ribbon) {
    mesh.Clear();
    mesh.name = "RibbonMesh";
    mesh.hideFlags = HideFlags.DontSave;

    _meshPoints.Clear();
    _meshIndices.Clear();
    ribbon.CreateMeshData(_meshPoints, _meshIndices);

    _meshPoints.GetPositions(_cachedVec3);
    mesh.SetVertices(_cachedVec3);

    _meshPoints.GetUvs(_cachedVec2);
    mesh.SetUVs(0, _cachedVec2);

    _meshPoints.GetColors(_cachedColor);
    mesh.SetColors(_cachedColor);

    mesh.SetIndices(_meshIndices.ToArray(), ribbon.Topology, 0);

    mesh.RecalculateNormals();
  }

  #endregion

}
