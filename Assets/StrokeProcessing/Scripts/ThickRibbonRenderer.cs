using UnityEngine;
using System.Collections.Generic;
using Leap.Unity.RuntimeGizmos;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ThickRibbonRenderer : MonoBehaviour, IStrokeRenderer, IRuntimeGizmoComponent {

  public Material _ribbonMaterial;
  public GameObject _finalizedRibbonParent;

  private MeshFilter _meshFilter;
  private MeshRenderer _meshRenderer;
  private Mesh _curRibbonMesh;
  private bool _beginningRibbon;
  private int _chunkSize;
  private int _curChunkIdx;
  private List<Vector3> _curChunkVerts;
  private List<int> _curChunkIndices;

  protected void Start() {
    _meshFilter = GetComponent<MeshFilter>();
    _meshRenderer = GetComponent<MeshRenderer>();

    _meshRenderer.material = _ribbonMaterial;

    _curChunkVerts = new List<Vector3>();
    _curChunkIndices = new List<int>();
  }

  public void InitializeRenderer() {
    _meshFilter.mesh = _curRibbonMesh = new Mesh();
    _curRibbonMesh.MarkDynamic();

    _beginningRibbon = true;
    _curChunkIdx = 0;
    _curChunkVerts.Clear();
    _curChunkIndices.Clear();
  }

  private List<Vector3> deletemeStrokePositions = new List<Vector3>();
  private List<Vector3> deletemeStrokeNormals = new List<Vector3>();

  public void UpdateRenderer(List<StrokePoint> filteredStroke, int maxChangedFromEnd) {
    if (_beginningRibbon) {
      _chunkSize = maxChangedFromEnd;
      _beginningRibbon = false;
    }

    int startIdx = Mathf.Max(0, filteredStroke.Count - 1 - maxChangedFromEnd);
    int endIdx = filteredStroke.Count - 1;
    for (int i = startIdx; i < endIdx; i++) {
      StrokePoint strokePoint = filteredStroke[i];

      // TODO: remove debug logic, generate thick ribbon mesh.

      // BEGIN DEBUG: logic so far
      if (i > deletemeStrokePositions.Count - 1) {
        deletemeStrokePositions.Add(strokePoint.position);
        deletemeStrokeNormals.Add(strokePoint.normal);
      }
      else {
        deletemeStrokePositions[i] = strokePoint.position;
        deletemeStrokeNormals[i] = strokePoint.normal;
      }
      // END DEBUG

    }
  }

  public void FinalizeRenderer() {
    deletemeStrokePositions.Clear();
    deletemeStrokeNormals.Clear();
  }

  #region Gizmos

  private bool _drawGizmos = true;

  public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    if (_drawGizmos) {
      drawer.color = Color.blue;
      for (int i = 0; i < deletemeStrokePositions.Count - 1; i++) {
        drawer.DrawLine(deletemeStrokePositions[i], deletemeStrokePositions[i + 1]);
      }
      drawer.color = Color.green;
      for (int i = 0; i < deletemeStrokePositions.Count; i++) {
        drawer.DrawLine(deletemeStrokePositions[i], deletemeStrokePositions[i] + deletemeStrokeNormals[i] * 0.04F);
      }
    }
  }

  #endregion
}
