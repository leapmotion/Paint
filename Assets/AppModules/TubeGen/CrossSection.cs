using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.TubeGen {

  /// <summary>
  /// Interface defining a ring of vertices.
  /// </summary>
  public interface ICrossSection {

    Slice<Slice<Vector3>> GetLocalSpaceVerts();

    void GenerateConnectionMeshData(LeapTransform otherCrossSection,
                                    ref Slice<Vector3> meshVerts,
                                    ref Slice<int>     meshIndices);

    void CorrectVerticesAtJunction(ICrossSection prevSection,
                                   ICrossSection nextSection,
                                   ref Slice<Vector3> verts);

  }

}