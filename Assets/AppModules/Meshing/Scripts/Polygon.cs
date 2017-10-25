using UnityEngine;

namespace Leap.Unity.Meshing {

  /// <summary>
  /// Struct representing an N-gon face. Vertices are stored as indices, not positions.
  /// To get positions, the Polygon must exist in the context of a PolyMesh.
  /// 
  /// The positions indexed by the face must be planar and convex.
  /// 
  /// </summary>
  public struct Polygon {

    public PolyMesh mesh;

    private int[] _verts;

    #region Vertices

    public int[] verts {
      get { return _verts; }
      set {
        if (value.Length < 3) {
          throw new System.InvalidOperationException(
            "Polygons must have at least 3 vertices.");
        }

        _verts = value;
      }
    }

    public int this[int idx] {
      get { return verts[idx]; }
      set { verts[idx] = value; }
    }

    public int Length { get { return _verts.Length; } }

    #endregion

    #region Operations

    /// <summary>
    /// Adds the argument amount to each vertex index in this polygon definition, and
    /// also returns this polygon for convenience.
    /// </summary>
    public Polygon IncrementIndices(int byAmount) {
      for (int i = 0; i < _verts.Length; i++) {
        _verts[i] += byAmount;
      }

      return this;
    }

    /// <summary>
    /// Given a PolyMesh to resolve this polygon's vertex indices to positions, returns
    /// the normal vector of this polygon definition.
    /// </summary>
    public Vector3 GetNormal(PolyMesh usingMesh) {
      return Vector3.Cross(usingMesh.GetPosition(_verts[1])
                           - usingMesh.GetPosition(_verts[0]),
                           usingMesh.GetPosition(_verts[2])
                           - usingMesh.GetPosition(_verts[0])).normalized;
    }

    #endregion

    #region Triangulation

    public TriangleEnumerator tris {
      get { return new TriangleEnumerator(this); }
    }

    public struct TriangleEnumerator {

      private Polygon _polygon;
      private int _curIdx;

      public TriangleEnumerator(Polygon polygon) {
        _polygon = polygon;

        _curIdx = -1;
      }

      public Triangle Current {
        get {
          return new Triangle() {
            a = _polygon[0],
            b = _polygon[_curIdx + 1],
            c = _polygon[_curIdx + 2]
          };
        }
      }

      public bool MoveNext() {
        _curIdx += 1;
        return _curIdx + 2 < _polygon.Length;
      }

      public TriangleEnumerator GetEnumerator() { return this; }

    }

    #endregion

    #region Edge Traversal

    public EdgeEnumerator edges { get { return new EdgeEnumerator(this); } }

    public struct EdgeEnumerator {
      Polygon _poly;
      int _curIdx;
      public EdgeEnumerator(Polygon polygon) {
        _poly = polygon;
        _curIdx = -1;
      }
      public Edge Current {
        get { return new Edge() {
                mesh = _poly.mesh,
                a    = _poly[_curIdx],
                b    = _poly[_curIdx + 1]
              };
        }
      }
      public bool MoveNext() {
        _curIdx += 1;
        return _curIdx + 1 < _poly.Length;
      }
      public EdgeEnumerator GetEnumerator() { return this; }
    }

    #endregion

  }

}