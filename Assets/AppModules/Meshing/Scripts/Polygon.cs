using Leap.Unity.Query;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing {

  /// <summary>
  /// Struct representing an N-gon face. Vertices are stored as indices, not positions.
  /// To get positions, the Polygon must exist in the context of a PolyMesh.
  /// 
  /// The positions indexed by the face must be planar and convex.
  /// </summary>
  public struct Polygon {

    public PolyMesh mesh;

    private List<int> _verts;

    #region Vertices

    public List<int> verts {
      get { return _verts; }
      set {
        _verts = value;
      }
    }
    
    /// <summary>
    /// Indexing a polygon directly has _slightly_ more overhead than indexing its verts
    /// directly, but indexes its vertices cyclically.
    /// </summary>
    public int this[int idx] {
      get { return verts[idx % verts.Count]; }
      set { verts[idx % verts.Count] = value; }
    }

    public int Count { get { return _verts.Count; } }

    #endregion

    #region Operations

    private Vector3 P(int vertIndex) {
      return mesh.GetPosition(vertIndex);
    }

    /// <summary>
    /// Returns the position of the vertex at the argument vertIndex in the _mesh_ of
    /// this polygon. This method is a shortcut for polygon.mesh.GetPosition(vertIndex);
    /// the provided index is _not_ the index of a vertex in this polygon, but the index
    /// of the vertex in the mesh!
    /// e.g. Don't do polygon.GetPosition(0), do polygon.GetPosition(polygon[0]).
    /// </summary>
    public Vector3 GetMeshPosition(int meshVertIndex) {
      return P(meshVertIndex);
    }

    /// <summary>
    /// Adds the argument amount to each vertex index in this polygon definition, and
    /// also returns this polygon for convenience. This function modifies this Polygon.
    /// 
    /// Warning: This is not guaranteed to result in a valid mesh polygon.
    /// </summary>
    public Polygon IncrementIndices(int byAmount) {
      for (int i = 0; i < _verts.Count; i++) {
        _verts[i] += byAmount;
      }

      return this;
    }

    /// <summary>
    /// Inserts a new vertex index into this Polygon between the indices specified by the
    /// argument Edge. The edge must be a valid edge of this Polygon. This function
    /// modifies this polygon and returns the polygon for convenience.
    /// 
    /// Warning: This is not guaranteed to result in a valid mesh polygon.
    /// </summary>
    public Polygon InsertEdgeVertex(Edge edge, int newVertIndex) {

      // Handle edge-case where the edge is between the first and last indices.
      if ((_verts[0] == edge.b && _verts[_verts.Count - 1] == edge.a)
          || (_verts[0] == edge.a && _verts[_verts.Count - 1] == edge.b)) {

        _verts.Add(newVertIndex);

        return this;
      }
      
      for (int i = 0; i < _verts.Count; i++) {
        if (_verts[i] == edge.a || _verts[i] == edge.b) {
          _verts.Insert(i + 1, newVertIndex);

          return this;
        }
      }

      return this;
    }

    /// <summary>
    /// Given a PolyMesh to resolve this polygon's vertex indices to positions, returns
    /// the normal vector of this polygon definition.
    /// 
    /// Obviously if the polygon vertices are non-planar, this won't work! But that's an
    /// assumption we make about all Polygons.
    /// </summary>
    public Vector3 GetNormal() {
      Vector3 normal = Vector3.zero;
      Vector3 a, b, c;
      for (int i = 0; i < _verts.Count; i++) {
        a = P(this[i]);
        b = P(this[i + 1]);
        c = P(this[i + 2]);
        normal = Vector3.Cross(b - a, c - a);
        if (normal != Vector3.zero) {
          return normal.normalized;
        }
      }
      return normal;
    }

    /// <summary>
    /// Calculates and returns whether this polygon is truly convex. The polygon's verts
    /// are assumed to be planar. (Not sure if your polygon is planar? Call IsPlanar())
    /// 
    /// Polygons that are added to meshes are always assumed to be convex; adding a
    /// non-convex polygon to a mesh via AddPolygon is an error. However, this method
    /// is useful when constructing new polygons manually.
    /// </summary>
    public bool IsConvex() {
      Maybe<Vector3> lastNonZeroCrossProduct = Maybe.None;

      if (_verts.Count < 3) {
        throw new System.InvalidOperationException("Polygons must have 3 or more vertices.");
      }

      if (_verts.Count == 3) return true;

      // Compare the cross products of (i -> i + 1) and (i -> i + 2) around the polygon;
      // if the cross products' ever flip direction with respect to one another, the
      // polygon must be non-convex. (Straight lines are OK!)
      for (int i = 0; i < _verts.Count; i++) {
        var a = P(_verts[i]);
        var b = P(_verts[(i + 1) % _verts.Count]);
        var c = P(_verts[(i + 2) % _verts.Count]);

        var ab = b - a;
        var ac = c - a;

        var abXac = Vector3.Cross(ab, ac);

        if (!lastNonZeroCrossProduct.hasValue) {
          lastNonZeroCrossProduct = abXac;
          if (lastNonZeroCrossProduct == Vector3.zero) {
            lastNonZeroCrossProduct = Maybe.None;
          }
        }
        else {
          if (abXac == Vector3.zero) continue;
          else {
            if (Vector3.Dot(abXac, lastNonZeroCrossProduct.valueOrDefault) < 0f) {
              return false;
            }
          }
        }
      }

      return false;
    }

    /// <summary>
    /// Calculates and returns whether this polygon is truly planar.
    /// 
    /// Polygons are always assumed to be planar; this method is useful for a
    /// develepor for debugging purposes when implementing new polygon or mesh operations.
    /// </summary>
    public bool IsPlanar() {
      if (verts.Count < 2) {
        throw new System.InvalidOperationException(
          "Polygon only has one or fewer vertex indices.");
      }

      if (verts.Count == 2) return true;

      if (verts.Count == 3) return true;

      Maybe<Vector3> lastCrossProduct = Maybe.None;
      for (int i = 0; i < verts.Count - 3; i++) {
        var a = P(this[i]);
        var b = P(this[i + 1]);
        var c = P(this[i + 2]);
        var ab = b - a;
        var ac = c - a;

        var curCrossProduct = Vector3.Cross(ab, ac);
        if (lastCrossProduct.hasValue) {
          // We expect every cross product of ab and ac (for a around the polygon)
          // to point in the same direction; by crossing them together, any deviation
          // in direction will produce a non-zero 'cross-cross-product.'
          var productWithLast = Vector3.Cross(curCrossProduct, lastCrossProduct.valueOrDefault);
          if (productWithLast.x > PolyMath.POSITION_TOLERANCE
              || productWithLast.y > PolyMath.POSITION_TOLERANCE
              || productWithLast.z > PolyMath.POSITION_TOLERANCE) {
            return false;
          }
        }
      }

      return true;
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
        return _curIdx + 2 < _polygon.Count;
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
        return _curIdx < _poly.Count;
      }
      public EdgeEnumerator GetEnumerator() { return this; }
    }

    #endregion

    #region Equality

    public override int GetHashCode() {
      var hash = new Hash();
      hash.Add(mesh);
      foreach (var id in verts) { hash.Add(id); }
      return hash;
    }

    public bool Equals(Polygon otherPoly) {
      if (this.mesh != otherPoly.mesh) return false;
      if (this._verts.Count != otherPoly._verts.Count) return false;

      // Utils.AreEqualUnordered(verts, otherPoly.verts); perhaps?
      // (would also need to sort before hashing)
      for (int i = 0; i < _verts.Count; i++) {
        if (_verts[i] != otherPoly._verts[i]) return false;

        // TODO DELETEME
        if (i >= 60) {
          throw new System.InvalidOperationException(
            "This polygon has WAY too many verts!");
        }
      }
      return true;
    }

    public override bool Equals(object obj) {
      if (obj is Polygon) {
        return Equals((Polygon)obj);
      }
      return base.Equals(obj);
    }

    public static bool operator ==(Polygon thisPoly, Polygon otherPoly) {
      return thisPoly.Equals(otherPoly);
    }
    public static bool operator !=(Polygon thisPoly, Polygon otherPoly) {
      return !(thisPoly.Equals(otherPoly));
    }

    #endregion

    #region Static PolyMesh Generator
    
    public static PolyMesh CreatePolyMesh(int numVerts) {
      var mesh = new PolyMesh();
      FillPolyMesh(numVerts, mesh);
      return mesh;
    }

    public static void FillPolyMesh(int numVerts, PolyMesh mesh) {
      numVerts = Mathf.Max(numVerts, 3);

      var positions = Pool<List<Vector3>>.Spawn();
      positions.Clear();
      try {
        Quaternion rot = Quaternion.AngleAxis(360f / numVerts, -Vector3.forward);
        Vector3 radial = Vector3.right;
        positions.Add(radial);
        for (int i = 1; i < numVerts; i++) {
          radial = rot * radial;
          positions.Add(radial);
        }

        var indices = Values.From(0).To(numVerts);
        var polygon = new Polygon() { mesh = mesh, verts = indices.ToList() };
        
        mesh.Fill(positions, polygon);
      }
      finally {
        positions.Clear();
        Pool<List<Vector3>>.Recycle(positions);
      }
    }

    #endregion

  }

}