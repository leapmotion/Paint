using UnityEngine;
using System;
using System.Collections.Generic;
using MeshGeneration;
using Leap.Unity.RuntimeGizmos;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class StrokeBufferRibbonRenderer : MonoBehaviour, IStrokeBufferRenderer, IRuntimeGizmoComponent {

  public Shader MeshShader;

  public Action<Mesh, List<StrokePoint>> OnMeshStrokeFinalized;

  private Mesh _mesh;
  private MeshFilter _filter;
  private MeshRenderer _renderer;
  private Material _meshMaterial;
  private TwoSidedRibbon _ribbon = new TwoSidedRibbon();
  private List<StrokePoint> _stroke;

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

    _filter.mesh = _mesh;
    _renderer.material = _meshMaterial;
  }

  public void RefreshRenderer(RingBuffer<StrokePoint> strokeBuffer) {
    for (int i = 0; i < strokeBuffer.Size; i++) {
      StrokePoint strokePoint = strokeBuffer.Get(i);

      MeshPoint point = new MeshPoint(strokePoint.position);
      point.Normal = strokePoint.normal;
      point.Color = strokePoint.color;

      if (i > _ribbon.Points.Count - 1) {
        _ribbon.Add(point, strokePoint.thickness);
      }
      else {
        _ribbon.Points[i] = point;
        _ribbon.Radii[i] = strokePoint.thickness;
      }
    }

    SetMeshDataFromRibbon(_mesh, _ribbon);
  }

  public void StopRenderer() {
    _mesh.Clear();
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