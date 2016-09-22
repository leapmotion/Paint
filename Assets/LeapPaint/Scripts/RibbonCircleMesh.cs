using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class RibbonCircleMesh : MonoBehaviour {

  public float _radius = 1F;
  public float _thickness = 0.2F;

  public float Radius {
    get { return _radius; }
    set {
      _radius = value;
      _dirty = true;
    }
  }
  public float Thickness {
    get { return _thickness; }
    set {
      _thickness = value;
      _dirty = true;
    }
  }

  private Mesh _mesh;
  private MeshFilter _meshFilter;
  private const int NUM_DIVISIONS = 64;
  private Vector3[] _verts;
  private int[] _tris;
  private bool _dirty;

  protected void Start() {
    _meshFilter = GetComponent<MeshFilter>();

    _meshFilter.mesh = _mesh = new Mesh();
    _mesh.MarkDynamic();

    _verts = new Vector3[NUM_DIVISIONS * 2];
    _tris = new int[NUM_DIVISIONS * 4];

    RefreshMesh(true);
  }

  protected void Update() {
    if (_dirty) {
      RefreshMesh();
    }
  }

  protected void OnValidate() {
    RefreshMesh();
  }

  private void RefreshMesh(bool updateTris=false) {
    Vector3 r = Vector3.right;
    Quaternion rot = Quaternion.AngleAxis((360F / NUM_DIVISIONS), Vector3.up);
    for (int i = 0; i < NUM_DIVISIONS * 2; i += 2) {
      _verts[i] = (r * _radius) + (r * _thickness);
      _verts[i+1] = (r * _radius) - (r * _thickness);
      r = rot * r;
    }

    //if (updateTris) {
    //  int numVerts = NUM_DIVISIONS * 2;
    //  int triIdx = 0;
    //  for (int i = 0; i < numVerts; i += 2) {
    //    _tris[triIdx++] = i;
    //    _tris[triIdx++] = ((i - 1) + numVerts) % numVerts;
    //    _tris[triIdx++] = i + 1;
    //  }
    //}
  }

}
