using UnityEngine;
using System.Collections.Generic;

namespace zzOld_MeshGeneration_LeapPaint_v3 {

  public interface IShape {
    void CreateMeshData(MeshPoints points, List<int> connections);
    MeshTopology Topology { get; }
  }

}
