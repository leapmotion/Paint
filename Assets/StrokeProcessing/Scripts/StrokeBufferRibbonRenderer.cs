using UnityEngine;
using System;
using System.Collections.Generic;
using MeshGeneration;
using Leap.Unity.RuntimeGizmos;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class StrokeBufferRibbonRenderer : MonoBehaviour, IStrokeBufferRenderer, IRuntimeGizmoComponent {

  public AnimationCurve previewThicknessCurve;

  public Shader MeshShader;

  public Action<Mesh, List<StrokePoint>> OnMeshStrokeFinalized;

  [HideInInspector]
  public bool isErasing = false;
  [HideInInspector]
  public Vector3 eraserPoint = Vector3.zero;

  private Mesh _mesh;
  private MeshFilter _filter;
  private MeshRenderer _renderer;
  private Material _meshMaterial;
  private TwoSidedRibbon _ribbon = new TwoSidedRibbon();
  private List<StrokePoint> _stroke;
  private bool _canUpdateRenderer = false;

  protected void Start() {
    _filter = GetComponent<MeshFilter>();
    _renderer = GetComponent<MeshRenderer>();

    if (MeshShader == null) {
      MeshShader = Shader.Find("LeapMotion/RibbonBufferShader");
    }
    _meshMaterial = new Material(MeshShader);
    _meshMaterial.hideFlags = HideFlags.HideAndDontSave;
  }

  public void InitializeRenderer() {
    if (_mesh == null) {
      _mesh = new Mesh();
      _mesh.MarkDynamic();
    }
    else {
      _mesh.Clear();
    }
    _ribbon.Clear();
    _prevDrawRadii.Clear();
    _prevDrawOffsets.Clear();

    _filter.mesh = _mesh;
    _renderer.material = _meshMaterial;

    _lastStrokeBuffer = null;

    _canUpdateRenderer = true;
  }

  private List<float> _prevDrawRadii = new List<float>();
  private List<Vector3> _prevDrawOffsets = new List<Vector3>();
  private RingBuffer<StrokePoint> _lastStrokeBuffer;
  private float _thicknessDecayMultiplier = 1F;
  private float _movementThicknessTick = 0.1F;

  public void RefreshRenderer(RingBuffer<StrokePoint> strokeBuffer) {
    _lastStrokeBuffer = strokeBuffer;
    _thicknessDecayMultiplier = Mathf.Clamp(_thicknessDecayMultiplier + 0.3F, 0F, 1F);
  }

  protected void Update() {
    if (_lastStrokeBuffer != null && _canUpdateRenderer) {
      RefreshRenderTrail();
    }
    _thicknessDecayMultiplier = Mathf.Lerp(_thicknessDecayMultiplier, 0F, Time.deltaTime * 10F);
  }

  private void RefreshRenderTrail() {
    Vector3 endPosition = _lastStrokeBuffer.GetFromEnd(0).position;
    for (int i = 0 ; i < _lastStrokeBuffer.Size; i++) {
      StrokePoint strokePoint = _lastStrokeBuffer.Get(i);

      MeshPoint point = new MeshPoint(strokePoint.position);
      point.Normal = strokePoint.normal;
      point.Color = strokePoint.color;
      MeshPoint randomPoint = new MeshPoint(eraserPoint + (UnityEngine.Random.onUnitSphere * 0.01f));
      randomPoint.Normal = UnityEngine.Random.onUnitSphere;
      randomPoint.Color = strokePoint.color;

      if (i > _ribbon.Points.Count - 1) {
        _ribbon.Add(point, strokePoint.thickness);
      }
      else {
        if (isErasing) {
          _ribbon.Points[i] = randomPoint;
          _ribbon.Radii[i] = 0.05f;
          if (_prevDrawRadii.Count-1>i) {
            _prevDrawRadii[i] = strokePoint.thickness;
          }
        } else {
          // Offset from most recent + decay
          Vector3 offsetFromEndPosition = endPosition - point.Position;
          Vector3 targetOffset = (offsetFromEndPosition * (1 - _thicknessDecayMultiplier));
          if (i > _prevDrawOffsets.Count - 1) {
            _prevDrawOffsets.Add(targetOffset);
          }
          point.Position = point.Position + Vector3.Slerp(_prevDrawOffsets[i], targetOffset, 0.1F);
          _prevDrawOffsets[i] = Vector3.Slerp(_prevDrawOffsets[i], targetOffset, 0.1F);
          _ribbon.Points[i] = point;

          // Thickness + decay
          float targetThickness = 0F;
          if (_lastStrokeBuffer.Size > 1) {
            targetThickness = strokePoint.thickness * previewThicknessCurve.Evaluate((float)(_lastStrokeBuffer.Size - 1 - i) / (_lastStrokeBuffer.Size - 1)) * _thicknessDecayMultiplier;
          }
          if (i > _prevDrawRadii.Count - 1) {
            _prevDrawRadii.Add(targetThickness);
          }
          _ribbon.Radii[i] = Mathf.Lerp(_prevDrawRadii[i], targetThickness, 0.05F);
          _prevDrawRadii[i] = _ribbon.Radii[i];
        }
      }
    }

    SetMeshDataFromRibbon(_mesh, _ribbon);
  }

  public void StopRenderer() {

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

    mesh.SetTriangles(_meshIndices, 0);

    mesh.RecalculateNormals();
  }

  #region Gizmos

  private bool _drawGizmos = false;

  public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    if (_drawGizmos) {
      drawer.color = Color.blue;
      for (int i = 0; i < _ribbon.Points.Count - 1; i++) {
        drawer.DrawLine(_ribbon.Points[i].Position, _ribbon.Points[i + 1].Position);
      }

      drawer.color = Color.green;
      for (int i = 0; i < _ribbon.Points.Count; i++) {
        drawer.DrawLine(_ribbon.Points[i].Position, _ribbon.Points[i].Position + _ribbon.Points[i].Normal * 0.02F);
      }

      drawer.color = Color.red;
      for (int i = 0; i < _ribbon.Points.Count - 1; i++) {
        Vector3 binormal = Vector3.Cross(_ribbon.Points[i + 1].Position - _ribbon.Points[i].Position, _ribbon.Points[i + 1].Normal).normalized * 0.04F;
        drawer.DrawLine(_ribbon.Points[i + 1].Position - binormal, _ribbon.Points[i + 1].Position + binormal);
        drawer.DrawCube(_ribbon.Points[i + 1].Position + binormal, Vector3.one * 0.005F);
      }
    }
  }

  #endregion

}