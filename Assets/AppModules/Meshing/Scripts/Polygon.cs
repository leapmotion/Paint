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
        if (value.Count < 3) {
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

    public int Count { get { return _verts.Count; } }

    #endregion

    #region Operations

    private Vector3 P(int vertIndex) {
      return mesh.GetPosition(vertIndex);
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
      // handle edge-case where edge is between first and last indices
      if ((_verts[0] == edge.b && _verts[_verts.Count - 1] == edge.a)
          || (_verts[0] == edge.a && _verts[_verts.Count - 1] == edge.b)) {

        _verts.Add(newVertIndex);
      }
      
      for (int i = 0; i < _verts.Count; i++) {
        if (_verts[i] == edge.a || _verts[i] == edge.b) {
          _verts.Insert(i + 1, newVertIndex);
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
    public Vector3 GetNormal(PolyMesh usingMesh) {
      return Vector3.Cross(usingMesh.GetPosition(_verts[1])
                           - usingMesh.GetPosition(_verts[0]),
                           usingMesh.GetPosition(_verts[2])
                           - usingMesh.GetPosition(_verts[0])).normalized;
    }

    /// <summary>
    /// Calculates and returns whether this polygon is convex. The polygon's verts are
    /// assumed to be planar.
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
        return _curIdx + 1 < _poly.Count;
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
      if (this.verts.Count != otherPoly.verts.Count) return false;
      for (int i = 0; i < verts.Count; i++) {
        if (verts[i] != otherPoly.verts[i]) return false;
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
      return !(thisPoly == otherPoly);
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
        var polygon = new Polygon() { verts = indices.ToList() };
        
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