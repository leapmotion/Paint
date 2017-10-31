using UnityEngine;
using System.Collections.Generic;
using Leap.Unity.Attributes;

[RequireComponent(typeof(MeshFilter))]
public class NinePatchPanelMesh : MonoBehaviour {

  [MinValue(0)]
  public float _width;
  [MinValue(0)]
  public float _height;
  [MinValue(2)]
  public int _horizontalResolution;
  [MinValue(2)]
  public int _verticalResolution;

  [Header("Nine Patching")]
  public bool _ninePatch;
  [MinValue(0)]
  public float _edgeSize;
  [Range(0, 0.5f)]
  public float _edgeUvSize;

  private MeshFilter _meshFilter;

  protected virtual void OnValidate() {
    _meshFilter = GetComponent<MeshFilter>();
    RefreshMesh();
  }

  protected virtual void Start() {
    _meshFilter = GetComponent<MeshFilter>();
    RefreshMesh();
  }

  private void RefreshMesh() {
    Mesh mesh = _meshFilter.sharedMesh;
    if (mesh == null) {
      _meshFilter.sharedMesh = mesh = new Mesh();
      mesh.name = "PanelMesh";
      mesh.hideFlags = HideFlags.HideAndDontSave;
    }
    GenerateMesh(mesh);
  }

  private List<Vector3> _cachedVerts = new List<Vector3>();
  private List<int> _cachedIndices = new List<int>();
  private List<Vector2> _cachedUVs = new List<Vector2>();
  private List<Vector2> _cachedUVs2 = new List<Vector2>();
  private void GenerateMesh(Mesh mesh) {

    mesh.Clear();
    mesh.MarkDynamic();

    //mesh.PreallocateTris((horizontalResolution - 1) * (verticalResolution - 1) * 2);
    //mesh.PreallocateVerts(horizontalResolution * verticalResolution, VertexAttributes.Uv0 | VertexAttributes.Uv1);

    _cachedVerts.Clear();
    _cachedIndices.Clear();
    _cachedUVs.Clear();
    _cachedUVs2.Clear();

    float left = -_width * 0.5f;
    float right = _width * 0.5f;
    float bot = -_height * 0.5f;
    float top = _height * 0.5f;

    for (int dy = 0; dy < _verticalResolution; dy++) {
      for (int dx = 0; dx < _horizontalResolution; dx++) {
        Vector2 pos;

        if (_ninePatch) {
          float uvX, uvY;

          if (dx == 0) {
            pos.x = left;
            uvX = 0;
          }
          else if (dx == _horizontalResolution - 1) {
            pos.x = right;
            uvX = 1;
          }
          else {
            float xPercent = (dx - 1.0f) / (_horizontalResolution - 3.0f);
            pos.x = Mathf.Lerp(left + _edgeSize, right - _edgeSize, xPercent);
            uvX = Mathf.Lerp(_edgeUvSize, 1.0f - _edgeUvSize, xPercent);
          }

          if (dy == 0) {
            pos.y = bot;
            uvY = 0;
          }
          else if (dy == _verticalResolution - 1) {
            pos.y = top;
            uvY = 1;
          }
          else {
            float yPercent = (dy - 1.0f) / (_verticalResolution - 3.0f);
            pos.y = Mathf.Lerp(bot + _edgeSize, top - _edgeSize, yPercent);
            uvY = Mathf.Clamp01(Mathf.Lerp(_edgeUvSize, 1.0f - _edgeUvSize, yPercent));
          }

          _cachedUVs.Add(new Vector2(uvX, uvY));
          //_cachedUVs2.Add(new Vector2(Mathf.InverseLerp(left, right, pos.x), Mathf.InverseLerp(bot, top, pos.y)));
          //mesh.uv0.Add(new Vector2(uvX, uvY));
          //mesh.uv1.Add(new Vector2(Mathf.InverseLerp(left, right, pos.x), Mathf.InverseLerp(bot, top, pos.y)));
        }
        else {
          float uvX = dx / (_horizontalResolution - 1.0f);
          float uvY = dy / (_verticalResolution - 1.0f);

          pos.x = _width * (uvX - 0.5f);
          pos.y = _height * (uvY - 0.5f);

          _cachedUVs.Add(new Vector2(uvX, uvY));
          //mesh.uv0.Add(new Vector2(uvX, uvY));
        }

        if (dy != 0 && dx != 0) {
          _cachedIndices.Add(_cachedVerts.Count);
          _cachedIndices.Add(_cachedVerts.Count - _horizontalResolution);
          _cachedIndices.Add(_cachedVerts.Count - 1);
          //mesh.indexes.Add(mesh.verts.Count);
          //mesh.indexes.Add(mesh.verts.Count - _horizontalResolution);
          //mesh.indexes.Add(mesh.verts.Count - 1);

          _cachedIndices.Add(_cachedVerts.Count - 1);
          _cachedIndices.Add(_cachedVerts.Count - _horizontalResolution);
          _cachedIndices.Add(_cachedVerts.Count - 1 - _horizontalResolution);
          //mesh.indexes.Add(mesh.verts.Count - 1);
          //mesh.indexes.Add(mesh.verts.Count - _horizontalResolution);
          //mesh.indexes.Add(mesh.verts.Count - 1 - _horizontalResolution);
        }

        _cachedVerts.Add(pos);
        //mesh.verts.Add(pos);
      }
    }

    mesh.SetVertices(_cachedVerts);
    mesh.SetTriangles(_cachedIndices, 0);
    mesh.SetUVs(0, _cachedUVs);
    //mesh.SetUVs(1, _cachedUVs2);

    mesh.UploadMeshData(false);
    mesh.RecalculateBounds();
    mesh.RecalculateNormals();
  }

}