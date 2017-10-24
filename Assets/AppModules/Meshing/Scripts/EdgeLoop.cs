using UnityEngine;

namespace Leap.Unity.Meshing {

  public struct EdgeLoop {
    PolyMesh polyMesh;
    int[] verts;

    private Vector3 P(int loopIdx) {
      return polyMesh.PositionAt(verts[loopIdx]);
    }

    public InternalPolyEnumerator insidePolys;
    public InternalPolyEnumerator outsidePolys;

    public struct InternalPolyEnumerator {
      PolyMesh polyMesh;

      public InternalPolyEnumerator(bool rightHanded) {
        throw new System.NotImplementedException();
      }

      /// <summary>
      /// Clears and fills the positions and polygons of the provided PolyMesh
      /// with the polygons defined by this polygon enumerator.
      /// </summary>
      public void Fill(PolyMesh intoMesh) {
        throw new System.NotImplementedException();
      }
    }
  }

}