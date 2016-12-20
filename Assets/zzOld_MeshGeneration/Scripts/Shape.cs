using UnityEngine;
using System.Collections.Generic;

namespace MeshGeneration {

  public interface IShape {
    void CreateMeshData(MeshPoints points, List<int> connections);
    MeshTopology Topology { get; }
  }

}
