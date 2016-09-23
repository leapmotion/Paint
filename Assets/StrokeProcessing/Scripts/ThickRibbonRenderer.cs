using UnityEngine;
using System.Collections.Generic;
using Leap.Unity.RuntimeGizmos;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ThickRibbonRenderer : MonoBehaviour, IStrokeRenderer, IRuntimeGizmoComponent {

  public Material _ribbonMaterial;
  public GameObject _finalizedRibbonParent;

  private Mesh _curChunkMesh;
  private MeshFilter _meshFilter;
  private MeshRenderer _meshRenderer;
  private bool _beginningRibbon;
  private int _maxChunkSize;
  private int _curChunkIdx;
  private List<Vector3> _curChunkVerts;
  private List<int> _curChunkIndices;
  private List<Color> _curChunkColors;

  protected void Start() {
    _meshFilter = GetComponent<MeshFilter>();
    _meshRenderer = GetComponent<MeshRenderer>();

    _meshRenderer.material = _ribbonMaterial;

    _curChunkVerts = new List<Vector3>();
    _curChunkIndices = new List<int>();
    _curChunkColors = new List<Color>();
  }

  public void InitializeRenderer() {
    _meshFilter.mesh = _curChunkMesh = new Mesh();
    _curChunkMesh.MarkDynamic();

    _beginningRibbon = true;
    _curChunkIdx = 0;
    _curChunkVerts.Clear();
    _curChunkIndices.Clear();
    _curChunkColors.Clear();
  }

  public void UpdateRenderer(List<StrokePoint> filteredStroke, int maxChangedFromEnd) {
    if (_beginningRibbon) {
      _maxChunkSize = maxChangedFromEnd;
      _beginningRibbon = false;
      // TODO: Currently this logic is unused. Use _maxChunkSize to break out new Mesh objects.
    }

    int startIdx = Mathf.Max(0, filteredStroke.Count - 1 - maxChangedFromEnd);
    int endIdx = filteredStroke.Count - 1;
    bool newMeshDataAdded = false;
    for (int i = startIdx; i < endIdx - 1; i++) {
      StrokePoint beginPoint = filteredStroke[i];
      StrokePoint endPoint = filteredStroke[i + 1];

      if ((_curChunkVerts.Count / 8) - 1 < i) {
        AddPrismVerts(_curChunkVerts, beginPoint.position, endPoint.position, beginPoint.normal, beginPoint.thickness);
        AddPrismIndices(_curChunkIndices, _curChunkVerts);
        for (int j = 0; j < 8; j++) { _curChunkColors.Add(beginPoint.color); }
        newMeshDataAdded = true;
      }
      else {
        SetPrismVerts(i, _curChunkVerts, beginPoint.position, endPoint.position, beginPoint.normal, beginPoint.thickness);
        // Connections between verts remain the same; no need to modify them.
      }
    }

    _curChunkMesh.SetVertices(_curChunkVerts);
    if (newMeshDataAdded) {
      _curChunkMesh.SetTriangles(_curChunkIndices, 0, true);
      _curChunkMesh.SetColors(_curChunkColors);
    }
    _curChunkMesh.RecalculateNormals();
    _curChunkMesh.UploadMeshData(false);
  }

  public void FinalizeRenderer() {
    _curChunkMesh.Optimize();

    // TODO: Add proper history management / other finalization logic
    GameObject meshObj = new GameObject();
    meshObj.transform.parent = _finalizedRibbonParent.transform;
    MeshRenderer renderer = meshObj.AddComponent<MeshRenderer>();
    renderer.material = _meshRenderer.material;
    MeshFilter filter = meshObj.AddComponent<MeshFilter>();
    filter.mesh = _curChunkMesh;

    _meshFilter.mesh = _curChunkMesh = null;
  }

  private void AddPrismVerts(List<Vector3> verts, Vector3 startPos, Vector3 endPos, Vector3 normal, float thickness) {
    for (int i = 0; i < 8; i++) {
      verts.Add(Vector3.zero);
    }
    SetPrismVerts((verts.Count / 8) - 1, verts, startPos, endPos, normal, thickness);
  }

  private void SetPrismVerts(int prismIdx, List<Vector3> verts, Vector3 startPos, Vector3 endPos, Vector3 normal, float thickness) {
    Vector3 segment = endPos - startPos;
    Vector3 xDir = Vector3.Cross(normal, segment.normalized);
    Vector3 yDir = normal;
    float yThickness = thickness / 10F;
    int startVertIdx = prismIdx * 8;

    // Set four vertices clockwise from the normal around the start position.
    verts[startVertIdx + 0] = startPos + xDir * thickness + yDir * yThickness;
    verts[startVertIdx + 1] = startPos + xDir * thickness - yDir * yThickness;
    verts[startVertIdx + 2] = startPos - xDir * thickness - yDir * yThickness;
    verts[startVertIdx + 3] = startPos - xDir * thickness + yDir * yThickness;

    // Set four vertices clockwise from the normal around the end position.
    verts[startVertIdx + 4] = endPos + xDir * thickness + yDir * yThickness;
    verts[startVertIdx + 5] = endPos + xDir * thickness - yDir * yThickness;
    verts[startVertIdx + 6] = endPos - xDir * thickness - yDir * yThickness;
    verts[startVertIdx + 7] = endPos - xDir * thickness + yDir * yThickness;
  }

  /// <summary>
  /// Assumes AddPrismVerts was just called on "verts" and indices only need to be added for the most recent 8 verts; adds 12 tris.
  /// Also assumes clockwise vertices (with respect to the prism segment).
  /// </summary>
  private void AddPrismIndices(List<int> indices, List<Vector3> verts) {
    int i = verts.Count - 8;

    // Startcap 1
    indices.Add(i);
    indices.Add(i + 1);
    indices.Add(i + 3);

    // Startcap 2
    indices.Add(i + 3);
    indices.Add(i + 1);
    indices.Add(i + 2);

    // Side 1
    indices.Add(i);
    indices.Add(i + 3);
    indices.Add(i + 4);

    // Side 2
    indices.Add(i + 3);
    indices.Add(i + 2);
    indices.Add(i + 7);

    // Side 3
    indices.Add(i + 2);
    indices.Add(i + 1);
    indices.Add(i + 6);

    // Side 4
    indices.Add(i + 1);
    indices.Add(i);
    indices.Add(i + 5);

    // Side 5
    indices.Add(i + 3);
    indices.Add(i + 7);
    indices.Add(i + 4);

    // Side 6
    indices.Add(i + 2);
    indices.Add(i + 6);
    indices.Add(i + 7);

    // Side 7
    indices.Add(i + 1);
    indices.Add(i + 5);
    indices.Add(i + 6);

    // Side 8
    indices.Add(i);
    indices.Add(i + 4);
    indices.Add(i + 5);

    // Endcap 1
    indices.Add(i + 7);
    indices.Add(i + 5);
    indices.Add(i + 4);

    // Enedcap 2
    indices.Add(i + 7);
    indices.Add(i + 6);
    indices.Add(i + 5);
  }

  #region Gizmos

  private bool _drawGizmos = false;

  public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    if (_drawGizmos) {
      //drawer.color = Color.blue;
      //for (int i = 0; i < deletemeStrokePositions.Count - 1; i++) {
      //  drawer.DrawLine(deletemeStrokePositions[i], deletemeStrokePositions[i + 1]);
      //}
      //drawer.color = Color.green;
      //for (int i = 0; i < deletemeStrokePositions.Count; i++) {
      //  drawer.DrawLine(deletemeStrokePositions[i], deletemeStrokePositions[i] + deletemeStrokeNormals[i] * 0.04F);
      //}
    }
  }

  #endregion
}
