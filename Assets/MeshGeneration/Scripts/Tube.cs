using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace MeshGeneration {

  public class Tube : IShape {
    private List<MeshPoint> _points = new List<MeshPoint>();
    private List<float> _radii = new List<float>();
    private int _resolution;

    private List<Vector3> _tangents = new List<Vector3>();

    public Tube(int resolution) {
      _resolution = resolution;
    }

    public void Clear() {
      _points.Clear();
      _radii.Clear();
    }

    public void Add(MeshPoint point, float radius) {
      _points.Add(point);
      _radii.Add(radius);
    }

    public void CreateMeshData(MeshPoints points, List<int> connections) {
      if (_points.Count <= 2) {
        return;
      }

      //Add dummy tangent at the begining and end
      _tangents.Add(Vector3.zero);

      for (int i = 1; i < _points.Count - 1; i++) {
        Vector3 a = _points[i - 1].Position;
        Vector3 b = _points[i].Position;
        Vector3 c = _points[i + 1].Position;

        Vector3 dir = c - a;
        Vector3 tangent = Vector3.Cross(dir, c - b);
        _tangents.Add(tangent.normalized);
      }

      _tangents.Add(Vector3.zero);

      //End tangents are the same as their neighbors
      _tangents[0] = _tangents[1];
      _tangents[_tangents.Count - 1] = _tangents[_tangents.Count - 2];

      for (int i = 0; i < _points.Count; i++) {
        Vector3 t = _tangents[i];

        Vector3 direction;
        if (i == 0) {
          direction = (_points[i + 1].Position - _points[i].Position).normalized;
        } else if (i == _points.Count - 1) {
          direction = (_points[i].Position - _points[i - 1].Position).normalized;
        } else {
          direction = (_points[i + 1].Position - _points[i - 1].Position).normalized;
        }

        MeshPoint centerPoint = _points[i];
        float radius = _radii[i];

        for (int j = 0; j < _resolution; j++) {
          float angle = j * 360.0f / _resolution;
          Quaternion rotation = Quaternion.AngleAxis(angle, direction);
          Vector3 spoke = rotation * t * radius;

          MeshPoint p = new MeshPoint(centerPoint.Position + spoke);
          p.Color = centerPoint.Color;
          p.Uv = new Vector2(j / (float)_resolution, i / (_points.Count - 1.0f));

          points.Add(p);
        }
      }

      _tangents.Clear();

      for (int i = 0; i < _points.Count - 1; i++) {
        int offset = i * _resolution;

        for (int j = 0; j < _resolution; j++) {
          int i0 = (j + 0) % _resolution + offset;
          int i1 = (j + 1) % _resolution + offset;

          int i2 = (j + 0) % _resolution + offset + _resolution;
          int i3 = (j + 1) % _resolution + offset + _resolution;

          connections.Add(i0);
          connections.Add(i1);
          connections.Add(i2);

          connections.Add(i1);
          connections.Add(i3);
          connections.Add(i2);
        }
      }
    }

    public MeshTopology Topology {
      get {
        return MeshTopology.Triangles;
      }
    }
  }

}