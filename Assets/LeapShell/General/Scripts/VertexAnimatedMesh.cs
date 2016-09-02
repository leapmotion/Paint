using UnityEngine;
using System.Collections.Generic;

public class VertexAnimatedMesh : MonoBehaviour {
  public const int MAX_TARGETS = 3;
  private int[] _targetIds = new int[MAX_TARGETS];

  private List<MeshGroup> _groupDefs = new List<MeshGroup>();
  private List<Vector2> _values = new List<Vector2>();

  private Mesh _bakedMesh;
  private Material _material;

  public int AddMesh(Mesh mesh, Matrix4x4 baseTransform, Matrix4x4 offsetA, Matrix4x4 offsetB) {
    int id = _groupDefs.Count;
    MeshGroup group = new MeshGroup();
    group.mesh = mesh;
    group.baseTransform = baseTransform;
    group.offsetA = offsetA;
    group.offsetB = offsetB;
    _groupDefs.Add(group);

    _values.Add(new Vector2());
    return id;
  }

  public void Bake() {
    _bakedMesh = new Mesh();
    _bakedMesh.name = "Combined mesh";

    List<Vector3> groupVerts = new List<Vector3>();
    List<Vector3> groupNormals = new List<Vector3>();
    List<Color> groupColors = new List<Color>();
    List<Vector2> groupUvs = new List<Vector2>();

    List<int> groupTris = new List<int>();

    for (int i = 0; i < _groupDefs.Count; i++) {
      var group = _groupDefs[i];

      int[] tris = group.mesh.triangles;
      for (int j = 0; j < tris.Length; j++) {
        groupTris.Add(groupVerts.Count + tris[j]);
      }

      Vector3[] verts = group.mesh.vertices;
      Vector2[] uvs = group.mesh.uv;
      for (int j = 0; j < verts.Length; j++) {
        Vector3 v = verts[j];
        Vector3 vb = group.baseTransform.MultiplyPoint3x4(v);
        Vector3 v0 = group.offsetA.MultiplyPoint3x4(v) - v;
        Vector3 v1 = group.offsetB.MultiplyPoint3x4(v) - v;

        groupVerts.Add(vb);
        groupNormals.Add(v0);
        groupColors.Add(new Color(v1.x, v1.y, v1.z, i / 255.0f));
        groupUvs.Add(uvs[j]);
      }
    }

    _bakedMesh.SetVertices(groupVerts);
    _bakedMesh.SetNormals(groupNormals);
    _bakedMesh.SetColors(groupColors);
    _bakedMesh.SetUVs(0, groupUvs);

    _bakedMesh.SetTriangles(groupTris.ToArray(), 0);

    _bakedMesh.RecalculateBounds();
    _bakedMesh.Optimize();
    _bakedMesh.UploadMeshData(true);

    GetComponent<MeshFilter>().mesh = _bakedMesh;
    _material = GetComponent<MeshRenderer>().material;

    for (int i = 0; i < MAX_TARGETS; i++) {
      _targetIds[i] = Shader.PropertyToID("_Target" + i);
    }
  }

  public void SetValue(int groupIndex, float value0, float value1) {
    _values[groupIndex] = new Vector2(value0, value1);
  }

  void LateUpdate() {
    int targetNumber = 0;
    for (int i = 0; i < _values.Count; i++) {
      Vector2 value = _values[i];
      if (value.x > 0 || value.y > 0) {
        _material.SetVector(_targetIds[targetNumber], new Vector3(value.x, value.y, i / 255.0f));
        targetNumber++;
      }

      if (targetNumber == MAX_TARGETS) {
        break;
      }
    }

    //zero out other targets if they dont exist
    for (; targetNumber < MAX_TARGETS; targetNumber++) {
      _material.SetVector(_targetIds[targetNumber], new Vector3(0, 0, 0));
    }
  }

  private struct MeshGroup {
    public Mesh mesh;
    public Matrix4x4 baseTransform;
    public Matrix4x4 offsetA;
    public Matrix4x4 offsetB;
  }
}
