using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing {

  public interface IPolyMesher<T> {

    /// <summary>
    /// Given the input object, fill the positions, polygons, and smooth edges
    /// lists that are sufficient to construct a PolyMesh mesh representation of
    /// the input object.
    /// </summary>
    void FillPolyMeshData(T inputObject,
                          List<Vector3> outStrokePositions,
                          List<Polygon> outStrokePolygons,
                          List<Edge> outStrokeSmoothEdges);

  }

}
