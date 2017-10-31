using UnityEngine;
using System.Collections.Generic;
using zzOld_MeshGeneration_LeapPaint_v3;

namespace zzOld_MeshGeneration_LeapPaint_v3 {

  public class TwoSidedRibbon : Ribbon {

    public override void CreateMeshData(MeshPoints points, List<int> connections) {
      if (_points.Count <= 2) {
        return;
      }

      //Add dummy tangent at the begining and end
      _tangents.Add(Vector3.zero);

      for (int i = 1; i < _points.Count - 1; i++) {
        Vector3 a = _points[i - 1].Position;
        Vector3 b = _points[i].Position;
        Vector3 c = _points[i + 1].Position;

        Vector3 dir = _hasNormals ? _points[i].Normal : c - a;
        Vector3 tangent = Vector3.Cross(dir, c - b);
        _tangents.Add(tangent.normalized);
      }

      _tangents.Add(Vector3.zero);

      //End tangents are the same as their neighbors
      _tangents[0] = _tangents[1];
      _tangents[_tangents.Count - 1] = _tangents[_tangents.Count - 2];

      for (int i = 0; i < _points.Count; i++) {
        Vector3 t = _tangents[i] * _radii[i];

        MeshPoint p0 = new MeshPoint(_points[i].Position + t);
        p0.Uv = new Vector3(0, i / (_points.Count - 1.0f));
        p0.Color = _points[i].Color;
        points.Add(p0);

        MeshPoint p1 = new MeshPoint(_points[i].Position - t);
        p1.Uv = new Vector3(1, i / (_points.Count - 1.0f));
        p1.Color = _points[i].Color;
        points.Add(p1);
      }

      for (int i = 0; i < _points.Count - 1; i++) {
        int offset = i * 2;

        connections.Add(offset + 0);
        connections.Add(offset + 1);
        connections.Add(offset + 2);

        connections.Add(offset + 1);
        connections.Add(offset + 3);
        connections.Add(offset + 2);
      }


      // Second set of tris for the opposite side.

      for (int i = 0; i < _points.Count; i++) {
        Vector3 t = _tangents[i] * _radii[i];

        MeshPoint p0 = new MeshPoint(_points[i].Position + t);
        p0.Uv = new Vector3(0, i / (_points.Count - 1.0f));
        p0.Color = _points[i].Color;
        points.Add(p0);

        MeshPoint p1 = new MeshPoint(_points[i].Position - t);
        p1.Uv = new Vector3(1, i / (_points.Count - 1.0f));
        p1.Color = _points[i].Color;
        points.Add(p1);
      }

      for (int i = _points.Count; i < (_points.Count * 2) - 1; i++) {
        int offset = i * 2;

        connections.Add(offset + 0);
        connections.Add(offset + 2);
        connections.Add(offset + 1);

        connections.Add(offset + 1);
        connections.Add(offset + 2);
        connections.Add(offset + 3);
      }


      _tangents.Clear();
    }

  }

}