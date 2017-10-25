using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing {

  public struct Edge {
    public PolyMesh mesh;
    public int a, b;

    public int this[int idx] {
      get { if (idx == 0) return a; return b; }
    }

    public override bool Equals(object obj) {
      if (obj is Edge) {
        return Equals((Edge)obj);
      }
      return base.Equals(obj);
    }
    public bool Equals(Edge other) {
      return a == other.a && b == other.b;
    }
    public override int GetHashCode() {
      return new Hash() { a, b };
    }
  }
  
  public struct EdgeSequence {
    public PolyMesh polyMesh;
    public int[] verts;

    public bool isSingleEdge { get { return verts.Length == 2; } }
    public Edge ToSingleEdge() {
      return new Edge() {
        mesh = polyMesh,
        a = verts[0],
        b = verts[1]
      };
    }

    /// <summary>
    /// Merges a soup of edges into a soup of edge sequences. Any edge that connects to
    /// another edge will wind up in the same EdgeSequence; any edge that has no
    /// connected edges will be placed into a degenerate single-edge EdgeSequence.
    /// </summary>
    public static void Merge(List<Edge> edges, List<EdgeSequence> intoSequenceList) {
      throw new System.NotImplementedException();
    }
  }

  public struct EdgeLoop {
    public PolyMesh polyMesh;
    public int[] verts;

    private Vector3 P(int loopIdx) {
      return polyMesh.GetPosition(verts[loopIdx]);
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