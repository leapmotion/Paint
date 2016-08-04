using UnityEngine;
using System.Collections.Generic;

namespace MeshGeneration {

  public abstract class Shape {

    public abstract void CreateMeshData(MeshPoints points, List<int> connections);
    public abstract MeshTopology Topology {
      get;
    }

  }

}
