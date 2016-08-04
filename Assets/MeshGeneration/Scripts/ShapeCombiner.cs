using UnityEngine;
using System;
using System.Collections.Generic;

namespace MeshGeneration {
  public class ShapeCombiner {
    public event Action<Mesh> OnNewMesh;

    protected MeshPoints _meshPoints = new MeshPoints();
    protected MeshPoints _tempPoints = new MeshPoints();
    protected List<int> _meshIndexes = new List<int>();
    protected List<int> _tempIndexes = new List<int>();
    protected MeshTopology _currTopology = MeshTopology.Triangles;

    protected List<Mesh> _meshList = new List<Mesh>();

    protected int _maxVertices;
    protected bool _shouldOptimize;
    protected bool _shouldUpload;
    protected bool _infiniteBounds;

    public ShapeCombiner(int maxVertices, bool shouldOptimize = true, bool shouldUpload = true, bool infiniteBounds = false) {
      _maxVertices = maxVertices;
      _shouldOptimize = shouldOptimize;
      _shouldUpload = shouldUpload;
      _infiniteBounds = infiniteBounds;
    }

    public virtual IEnumerable<Mesh> CurrentMeshes {
      get {
        return _meshList;
      }
    }

    public virtual void AddShape(Shape shape) {
      shape.CreateMeshData(_tempPoints, _tempIndexes);
      if(_tempPoints.Count == 0 || _tempIndexes.Count == 0) {
        return;
      }

      bool doesNeedToFinalize = false;

      //If the new vertices will bring us over the vertex count
      if (_meshPoints.Count + _tempPoints.Count >= _maxVertices) {
        doesNeedToFinalize = true;
      }

      //If the current mesh already has data, but either
      //   the current mesh has normals and the new shape does not
      //       or
      //   the current mesh does not have normals but the new shape does
      if (_meshPoints.Count != 0 && (_meshPoints.HasNormalsDefined != _tempPoints.HasNormalsDefined)) {
        doesNeedToFinalize = true;
      }

      //If the current mesh already has data, but has a different topology
      if (_meshPoints.Count != 0 && _currTopology != shape.Topology) {
        doesNeedToFinalize = true;
      } else {
        _currTopology = shape.Topology;
      }

      if (doesNeedToFinalize) {
        FinalizeCurrentMesh();
      }


      int offset = _meshPoints.Count;

      _tempPoints.CopyTo(_meshPoints);

      for (int i = 0; i < _tempIndexes.Count; i++) {
        _meshIndexes.Add(_tempIndexes[i] + offset);
      }

      _tempPoints.Clear();
      _tempIndexes.Clear();
    }

    private List<Vector3> _cachedVec3 = new List<Vector3>();
    private List<Vector2> _cachedVec2 = new List<Vector2>();
    private List<Color> _cachedColor = new List<Color>();
    public virtual void FinalizeCurrentMesh() {
      if (_meshPoints.Count == 0) {
        return;
      }

      Mesh mesh = new Mesh();
      mesh.name = "MultiMesh";

      _meshPoints.GetPositions(_cachedVec3);
      mesh.SetVertices(_cachedVec3);

      _meshPoints.GetUvs(_cachedVec2);
      mesh.SetUVs(0, _cachedVec2);

      _meshPoints.GetColors(_cachedColor);
      mesh.SetColors(_cachedColor);

      mesh.SetIndices(_meshIndexes.ToArray(), _currTopology, 0);

      if (_meshPoints.HasNormalsDefined) {
        _meshPoints.GetNormals(_cachedVec3);
        mesh.SetNormals(_cachedVec3);
      } else {
        mesh.RecalculateNormals();
      }

      if (_infiniteBounds) {
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100000f);
      } else {
        mesh.RecalculateBounds();
      }

      if (_shouldOptimize) {
        mesh.Optimize();
      }

      if (_shouldUpload) {
        mesh.UploadMeshData(true);
      }

      _meshList.Add(mesh);

      if(OnNewMesh != null) {
        OnNewMesh(mesh);
      }

      _meshPoints.Clear();
      _meshIndexes.Clear();
    }
  }
}

