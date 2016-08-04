using UnityEngine;
using System.Collections.Generic;

namespace MeshGeneration {

  public class MeshPoints {
    private List<MeshPoint> _points = new List<MeshPoint>();
    private bool _hasNormalsDefined = false;

    public int Count {
      get {
        return _points.Count;
      }
    }

    public bool HasNormalsDefined {
      get {
        return _hasNormalsDefined;
      }
    }

    public void Clear() {
      _points.Clear();
      _hasNormalsDefined = false;
    }

    public void Add(MeshPoint point) {
      if (_points.Count != 0) {
        if (_hasNormalsDefined != point.HasNormal) {
          throw new System.Exception("Could not add point to the point collection because the normals did not match!");
        }
      }

      _points.Add(point);
      _hasNormalsDefined |= point.HasNormal;
    }

    public void GetPositions(List<Vector3> positions) {
      positions.Clear();
      for(int i=0; i<_points.Count; i++) {
        positions.Add(_points[i].Position);
      }
    }

    public void GetNormals(List<Vector3> normals) {
      normals.Clear();
      for (int i = 0; i < _points.Count; i++) {
        normals.Add(_points[i].Normal);
      }
    }

    public void GetColors(List<Color> colors) {
      colors.Clear();
      for (int i = 0; i < _points.Count; i++) {
        colors.Add(_points[i].Color);
      }
    }

    public void GetUvs(List<Vector2> uvs) {
      uvs.Clear();
      for (int i = 0; i < _points.Count; i++) {
        uvs.Add(_points[i].Uv);
      }
    }

    public MeshPoint this[int index] {
      get {
        return _points[index];
      }
      set {
        _points[index] = value;
      }
    }

    public void CopyTo(MeshPoints other) {
      other._points.AddRange(_points);
      other._hasNormalsDefined = _hasNormalsDefined;
    }
  }

}