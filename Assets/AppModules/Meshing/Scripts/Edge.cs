using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing {

  /// <summary>
  /// Edges are automatically constructed and managed by PolyMesh operations. They
  /// represent a connection between two position indices (vertices) of its mesh;
  /// however, they do not maintain a reference to any specific Polygon on their own!
  /// 
  /// Edges do NOT have direction. Two edges with opposite A and B indices are considered
  /// identical by PolyMesh operations. This is possible in part because edges are only
  /// "soft" data for a mesh; Polygons are not defined by edges but by an ordered
  /// sequence of mesh position indices.
  /// 
  /// The mesh itself stores and managed the data for face-adjacency with any given Edge,
  /// and an Edge struct will remain valid over polygon-modification operations to the
  /// mesh as long s those operations themselves do not destroy the existence of the edge.
  /// 
  /// (The SplitEdgeAddVertex operation is an important example of an Edge-invalidating
  /// operation. A PokePolygon operation, by contrast, will affect a Polygon but never
  /// an Edge.)
  /// </summary>
  public struct Edge {
    public PolyMesh mesh;
    public int a, b;

    public int this[int idx] {
      get { if (idx == 0) return a; return b; }
    }

    #region Equality

    public override int GetHashCode() {
      int hashA, hashB;
      if (a > b) { hashA = a; hashB = b; }
      else       { hashA = b; hashB = a; }
      return new Hash() { mesh, hashA, hashB };
    }

    public bool Equals(Edge other) {
      return (a == other.a && b == other.b)
          || (a == other.b && b == other.a);
    }
    public override bool Equals(object obj) {
      if (obj is Edge) {
        return Equals((Edge)obj);
      }
      return base.Equals(obj);
    }

    public static bool operator ==(Edge thisEdge, Edge otherEdge) {
      return thisEdge.Equals(otherEdge);
    }

    public static bool operator !=(Edge thisEdge, Edge otherEdge) {
      return !(thisEdge.Equals(otherEdge));
    }

    #endregion

    private Vector3 P(int vertIdx) { return mesh.GetPosition(vertIdx); }

    public Vector3 GetPositionAlongEdge(float amountAlongEdge, EdgeDistanceMode mode) {
      var pA = P(a);
      var pB = P(b);
      var lineVec = (pB - pA);
      var mag = lineVec.magnitude;
      var dir = lineVec / mag;

      switch (mode) {
        case EdgeDistanceMode.Normalized:
          return pA + dir * amountAlongEdge * mag;
        case EdgeDistanceMode.Absolute:
        default:
          return pA + dir * amountAlongEdge;
      }
    }

  }

  public enum EdgeDistanceMode { Normalized, Absolute }

  public struct EdgeSequence {

    // TODO: This is supposed to help in dealing with the bunch of edges that are
    // returns as the edges of a cutting operation.

    //public PolyMesh polyMesh;
    //public int[] verts;

    //public bool isSingleEdge { get { return verts.Length == 2; } }
    //public Edge ToSingleEdge() {
    //  return new Edge() {
    //    mesh = polyMesh,
    //    a = verts[0],
    //    b = verts[1]
    //  };
    //}

    ///// <summary>
    ///// Merges a soup of edges into a soup of edge sequences. Any edge that connects to
    ///// another edge will wind up in the same EdgeSequence; any edge that has no
    ///// connected edges will be placed into a degenerate single-edge EdgeSequence.
    ///// </summary>
    //public static void Merge(List<Edge> edges, List<EdgeSequence> intoSequenceList) {
    //  throw new System.NotImplementedException();
    //}

  }

  //public struct EdgeLoop {
  //  public PolyMesh polyMesh;
  //  public int[] verts;

  //  private Vector3 P(int loopIdx) {
  //    return polyMesh.GetPosition(verts[loopIdx]);
  //  }

  //  public InternalPolyEnumerator insidePolys;
  //  public InternalPolyEnumerator outsidePolys;

  //  public struct InternalPolyEnumerator {
  //    PolyMesh polyMesh;

  //    public InternalPolyEnumerator(bool rightHanded) {
  //      throw new System.NotImplementedException();
  //    }

  //    /// <summary>
  //    /// Clears and fills the positions and polygons of the provided PolyMesh
  //    /// with the polygons defined by this polygon enumerator.
  //    /// </summary>
  //    public void Fill(PolyMesh intoMesh) {
  //      throw new System.NotImplementedException();
  //    }
  //  }
  //}

}