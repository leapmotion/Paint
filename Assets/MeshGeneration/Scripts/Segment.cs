using UnityEngine;
using System.Collections.Generic;

namespace MeshGeneration {

  public class Segment : Shape {
    private MeshPoints _points = new MeshPoints();

    public void Clear() {
      _points.Clear();
    }

    public void Add(MeshPoint point) {
      _points.Add(point);
    }

    public override void CreateMeshData(MeshPoints points, List<int> connections) {
      if (_points.Count <= 1) {
        return;
      }
      
      _points.CopyTo(points);

      for (int i = 0; i < _points.Count - 1; i++) {
        connections.Add(i);
        connections.Add(i + 1);
      }
    }

    public override MeshTopology Topology {
      get {
        return MeshTopology.Lines;
      }
    }

  }
}


