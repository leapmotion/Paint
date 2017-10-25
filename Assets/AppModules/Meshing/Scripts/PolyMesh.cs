using Leap.Unity.Query;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing {

  public class PolyMesh {

    #region Data

    private List<Vector3> _positions;
    public ReadonlyList<Vector3> positions {
      get { return _positions; }
    }

    private List<Polygon> _polygons;
    public ReadonlyList<Polygon> polygons {
      get { return _polygons; }
    }

    /// <summary> Updated when AddPolygon is called. </summary>
    private Dictionary<Edge, List<Polygon>> _edgeFaces;
    public Dictionary<Edge, List<Polygon>> edgeFaces {
      get { return _edgeFaces; }
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new, empty PolyMesh.
    /// </summary>
    public PolyMesh() {
      _positions = new List<Vector3>();
      _polygons = new List<Polygon>();
      _edgeFaces = new Dictionary<Edge, List<Polygon>>();
    }

    /// <summary>
    /// Creates a new PolyMesh using copies of the provided positions and polygons
    /// lists.
    /// </summary>
    public PolyMesh(ReadonlyList<Vector3> positions, ReadonlyList<Polygon> polygons)
      : this() {
      AddPositions(positions);
      AddPolygons(polygons);
    }

    /// <summary>
    /// Creates a new PolyMesh by copying the elements of the positions and polygons
    /// enumerables.
    /// </summary>
    public PolyMesh(IEnumerable<Vector3> positions, IEnumerable<Polygon> polygons) 
      : this() {
      foreach (var position in positions) {
        AddPosition(position);
      }
      foreach (var polygon in polygons) {
        AddPolygon(polygon);
      }
    }

    #endregion

    #region Basic Operations

    /// <summary>
    /// Clears the PolyMesh.
    /// </summary>
    public void Clear() {
      _positions.Clear();
      _polygons.Clear();
      _edgeFaces.Clear();
    }

    /// <summary>
    /// Returns the position from the positions array of the argument vertex index.
    /// </summary>
    private Vector3 P(int vertIdx) {
      return positions[vertIdx];
    }

    /// <summary>
    /// Returns the position from the positions array of the argument vertex index.
    /// </summary>
    public Vector3 GetPosition(int vertIdx) {
      return P(vertIdx);
    }

    /// <summary>
    /// Adds a new position to this PolyMesh. Optionally provide the index of the added
    /// position.
    /// </summary>
    public void AddPosition(Vector3 position) {
      _positions.Add(position);
    }

    /// <summary>
    /// Adds a new position to this PolyMesh. Optionally provide the index of the added
    /// position.
    /// </summary>
    public void AddPosition(Vector3 position, int addedIndex) {
      addedIndex = _positions.Count;
      _positions.Add(position);
    }

    /// <summary>
    /// Adds new positions to this PolyMesh. Optionally provide a list of indices to fill
    /// if you'd like to have the indices of the positions that were added.
    /// </summary>
    public void AddPositions(ReadonlyList<Vector3> positions) {
      _positions.AddRange(positions);
    }

    /// <summary>
    /// Adds new positions to this PolyMesh. Optionally provide a list of indices to fill
    /// if you'd like to have the indices of the positions that were added.
    /// </summary>
    public void AddPositions(ReadonlyList<Vector3> positions, List<int> addedIndicesToFill) {
      int startCount = _positions.Count;

      AddPositions(positions);

      addedIndicesToFill.Clear();
      foreach (var n in Values.From(startCount).To(_positions.Count)) {
        addedIndicesToFill.Add(n);
      }
    }

    /// <summary>
    /// Adds a list of polygon definitions to this PolyMesh, one at a time.
    /// </summary>
    public void AddPolygons(ReadonlyList<Polygon> polygons) {
      foreach (var poly in polygons) {
        AddPolygon(poly);
      }
    }

    /// <summary>
    /// Adds a polygon to this PolyMesh.
    /// </summary>
    public void AddPolygon(Polygon polygon) {
      if (polygon.mesh != this) {
        // We assume that a polygon passed to a new mesh is supposed to be a part of that
        // new mesh; this is common if e.g. combining two meshes into one mesh.
        polygon.mesh = this;
      }

      _polygons.Add(polygon);

      addEdges(polygon);
    }

    #endregion

    #region Edge Data

    /// <summary>
    /// Adds edge data based on the polygon.
    /// 
    /// This should be called right after the polygon is added to the mesh.
    /// </summary>
    private void addEdges(Polygon poly) {
      foreach (var edge in poly.edges) {
        List<Polygon> adjFaces;
        if (edgeFaces.TryGetValue(edge, out adjFaces)) {
          adjFaces.Add(poly);
        }
        else {
          edgeFaces[edge] = new List<Polygon>() { poly };
        }
      }
    }

    #endregion

    #region Operations

    /// <summary>
    /// High-level PolyMesh operations.
    /// </summary>
    public static class Op {

      public static void Combine(PolyMesh A, PolyMesh B, PolyMesh intoPolyMesh) {
        intoPolyMesh.Clear();

        intoPolyMesh.AddPositions(A.positions);
        intoPolyMesh.AddPolygons(A.polygons);

        intoPolyMesh.AddPositions(B.positions);
        foreach (var poly in B.polygons) {
          intoPolyMesh.AddPolygon(poly.IncrementIndices(A.positions.Count));
        }
      }

      public static PolyMesh Combine(PolyMesh A, PolyMesh B) {
        var result = new PolyMesh();

        Combine(A, B, result);

        return result;
      }
      
      public static void Subtract(PolyMesh A, PolyMesh B, PolyMesh intoPolyMesh) {

        throw new System.NotImplementedException();

        // Cut edge loops corresponding to each mesh at their intersections.
        //var aEdgeLoops = Pool<List<EdgeLoop>>.Spawn();
        //aEdgeLoops.Clear();
        //var bEdgeLoops = Pool<List<EdgeLoop>>.Spawn();
        //bEdgeLoops.Clear();
        //try {
        //  CutIntersectionLoops(A, B, ref aEdgeLoops, ref bEdgeLoops);
        //}
        //finally {
        //  aEdgeLoops.Clear();
        //  Pool<List<EdgeLoop>>.Recycle(aEdgeLoops);
        //  bEdgeLoops.Clear();
        //  Pool<List<EdgeLoop>>.Recycle(bEdgeLoops);
        //}
        
        // Fill the result PolyMesh with the combined polygons inside each edge loop on
        // A and outside each edge loop on B.
        //var aTemp = Pool<PolyMesh>.Spawn();
        //var bTemp = Pool<PolyMesh>.Spawn();
        //try {
        //  foreach (var edgeLoop in aEdgeLoops) {
        //    aTemp.AddPolygons(edgeLoop.insidePolys);
        //  }
        //  foreach (var edgeLoop in bEdgeLoops) {
        //    bTemp.AddPolygons(edgeLoop.outsidePolys);
        //  }

        //  Combine(aTemp, bTemp, intoPolyMesh);
        //}
        //finally {
        //  Pool<PolyMesh>.Recycle(aTemp);
        //  Pool<PolyMesh>.Recycle(bTemp);
        //}
      }

      public static PolyMesh Subtract(PolyMesh A, PolyMesh B) {
        var result = new PolyMesh();

        Subtract(A, B, result);

        return result;
      }

      public static void CutIntersectionLoops(PolyMesh A, PolyMesh B,
                                              out EdgeLoop aIntersection,
                                              out EdgeLoop bIntersection) {
        // Next steps.. this structure isn't right,
        // PolyMesh A and B may well have multiple edge loops that define their
        // intersections.
        throw new System.NotImplementedException();
      }

    }

    /// <summary>
    /// Mid-level PolyMesh operations, lower-level than Op.
    /// </summary>
    public static class MidOp {

      /// <summary>
      /// Cuts into A, using PolyMesh B's polygon b. This operation modifies A, producing
      /// extra positions if necessary, and increasing the number of faces (polygons) if
      /// the cut succeeds.
      /// 
      /// The cut is "successful" if the cut attempt produces at least one new edge of
      /// non-zero length between two position indices, which may be added to the
      /// positions list for the purposes of the cut.
      /// </summary>
      public static bool TryCut(PolyMesh A,
                                PolyMesh B, Polygon b,
                                List<EdgeSequence> cutResult) {

        var edges = Pool<List<Edge>>.Spawn();
        edges.Clear();
        try {

          // Each polygon in A needs to be checked against B's polygon b to see if
          // b can successfully cut it. Each time a successful cut is constructed,
          // we need to immediately apply the cut, which is guaranteed to destroy and
          // replace the polygon at the current polygon index with at least two new
          // polygons. We apply each cut operation immediately after we determine a
          // successful cut can occur, and assume that any polygon modifications will
          // only affect the polygon at the current index (which will be destroyed) or
          // later indices (e.g. new polygons will be added), so we don't have to restart
          // through the whole list -- we merely have to start from the current polygon
          // index.
          Maybe<PolyCutOp> cutOp = Maybe.None;
          int fromPolyIdx = 0;
          while (fromPolyIdx < A.polygons.Count) {
            foreach (var poly in A.polygons.FromIndex(fromPolyIdx)) {

              // We're so close! Fundamentally, this problem is easier to solve if I
              // express its solution not with atomic "cuts" but with atomic "cut points,"
              // the application of which is MIGHT poke a face or split and edge.
              // Once that happens, the mesh polygons will have been changed,
              // and I have to jump back to HERE, but currently being HERE expects a
              // whole CUT to have taken place.
              
              // In effect, this function slowly reduces cuts to their most trivial form:
              // The cut already existing!

              if (TryCut(A, poly, fromPolyIdx,  B, poly, out cutOp)) {
                edges.Add(cutOp.valueOrDefault.cutEdge);
                break;
              }

              fromPolyIdx += 1;
            }

            // Apply a cut operation if we have one, then consume it.
            if (cutOp.hasValue) {
              throw new System.NotImplementedException();
              //ApplyCut(A, cutOp.valueOrDefault);

              cutOp = Maybe.None;
            }
          }

          if (edges.Count > 0) {
            EdgeSequence.Merge(edges, cutResult);

            return true;
          }
        }
        finally {
          edges.Clear();
          Pool<List<Edge>>.Recycle(edges);
        }

        return false;
      }

      /// <summary>
      /// A data object representing the result of a cut on a single polygon.
      /// </summary>
      public struct PolyCutOp {
        public PolyMesh  polyMesh;
        public int       removePolyIdx;
        public Vector3[] newPositions;
        public Polygon[] newPolygons;
        public Edge      cutEdge;
      }

      /// <summary>
      /// A point produced during Cut operation logic; it can either represent an
      /// existing point on a mesh (by storing that point's index) or a new point
      /// to be added.
      /// </summary>
      public struct CutPoint {
        public PolyMesh mesh;
        public Polygon  poly;

        private Maybe<Vector3> _maybeNewPoint;
        private Maybe<int>     _maybeExistingPoint;

        public CutPoint(Vector3 desiredPoint, PolyMesh inMesh, Polygon onPoly) {
          mesh = inMesh;
          poly = onPoly;
          _maybeNewPoint = Maybe.None;
          _maybeExistingPoint = Maybe.None;

          bool isCurPoint = false;
          int vertIdx = 0;
          foreach (var vert in poly.verts) {
            var pos = mesh.GetPosition(vert);
            if (pos == desiredPoint) {
              _maybeExistingPoint = vertIdx;
              isCurPoint = true;
              break;
            }
            vertIdx++;
          }

          if (!isCurPoint) {
            _maybeNewPoint = desiredPoint;
          }
        }

        public bool isNewPoint { get { return _maybeNewPoint.hasValue; } }
        public Vector3 newPoint { get { return _maybeNewPoint.valueOrDefault; } }
        public bool isExistingPoint { get { return _maybeExistingPoint.hasValue; } }
        public int existingPoint { get { return _maybeExistingPoint.valueOrDefault; } }
      }

      /// <summary>
      /// Cuts into A's polygon a using B's polygon b.
      /// 
      /// A successful cut indicates that the cut operation would require replacing
      /// A's polygon a with two or more new polygons and zero or more new positions.
      /// 
      /// This does not modify A, but if "successful," returns a PolyCutOp that can be
      /// applied to A to produce the result of the cut.
      /// </summary>
      public static bool TryCut(PolyMesh A, Polygon aPoly, int aPolyIdx,
                                PolyMesh B, Polygon bPoly,
                                out Maybe<PolyCutOp> maybeCutOp) {

        var bPolyPlane = Plane.FromPoly(B, bPoly);

        var cutOp = new PolyCutOp() {
          polyMesh = A,
          removePolyIdx = -1
        };

        var newPositions = Pool<List<Vector3>>.Spawn();
        newPositions.Clear();
        var newPolygons = Pool<List<Polygon>>.Spawn();
        newPolygons.Clear();
        var cutPoints = Pool<List<CutPoint>>.Spawn();
        cutPoints.Clear();
        try {

          // Construct cut points. After removing trivially similar cutpoints, we expect
          // a maximum of two.
          foreach (var aEdge in aPoly.edges) {
            float intersectionTime = 0f;
            var onPolyBPlane = PolyMath.Intersect(Line.FromEdge(A, aEdge),
                                                  bPolyPlane, out intersectionTime);

            if (onPolyBPlane.hasValue) {
              if (intersectionTime > 0f && intersectionTime < 1f) {
                cutPoints.Add(new CutPoint(
                                onPolyBPlane.valueOrDefault.ClampedTo(A, aPoly),
                                A, aPoly));
              }
            }
          }

          // Make sure there aren't multiple cut points for a single position index.
          var intCutPointDict = Pool<Dictionary<int, CutPoint>>.Spawn();
          intCutPointDict.Clear();
          try {
            foreach (var cutPoint in cutPoints) {
              if (!cutPoint.isNewPoint) {
                intCutPointDict[cutPoint.existingPoint] = cutPoint;
              }
            }

            cutPoints.RemoveAll(c => (!c.isNewPoint)
                                      && !intCutPointDict.ContainsKey(c.existingPoint));
          }
          finally {
            intCutPointDict.Clear();
            Pool<Dictionary<int, CutPoint>>.Recycle(intCutPointDict);
          }
          
          if (cutPoints.Count > 2) {
            Debug.LogError("Logic error somewhere during cut. Cut points length is "
                           + cutPoints.Count);
          }

          throw new System.NotImplementedException();

          // Evaluate the cut points; a valid cut will have two cut points on the polygon
          // being cut.
          

          //Maybe<int> cutEdgeA = Maybe.None;
          //Maybe<int> cutEdgeB = Maybe.None;
          //int newPointIndex = A.positions.Count;
          //foreach (var cutPoint in cutPoints) {
          //  cutOp.removePolyIdx = aPolyIdx;

          //  if (cutPoint.isNewPoint) {
          //    newPositions.Add(cutPoint.newPoint);

          //    if (cutEdgeA.hasValue) {
          //      cutEdgeB = newPointIndex++;
          //    }
          //    else {
          //      cutEdgeA = newPointIndex++;
          //    }
          //  }
          //  else {
          //    if (cutEdgeA.hasValue) {
          //      cutEdgeB = cutPoint.existingPoint;
          //    }
          //    else {
          //      cutEdgeB = cutPoint.existingPoint;
          //    }
          //  }
          //}
          

        }
        finally {
          newPositions.Clear();
          Pool<List<Vector3>>.Recycle(newPositions);
          newPolygons.Clear();
          Pool<List<Polygon>>.Recycle(newPolygons);
          cutPoints.Clear();
          Pool<List<CutPoint>>.Recycle(cutPoints);
        }

      }

      public struct PolyCutPoint {
        public int existingPoint { get { throw new System.NotImplementedException(); } }
      }

      public static void ApplyCut(PolyCutPoint c0, PolyCutPoint c1) {

        // Hmmm no I've got to be able to just apply the cut points in sequence here...
        // Take advantage of the calculations already present in the cut points
        // to optimally cut the polygon.

        int cut0Idx = ApplyCutPoint(c0);
        int cut1Idx = ApplyCutPoint(c1);

        //LowOp.SplitPolygon(poly, c0.existingPoint, c1.existingPoint);

      }

      /// <summary>
      /// Applies the PolyCutPoint to its configured polygon and return the index of the
      /// vertex of the cut point. (This may or may not add a new position to the
      /// underlying mesh.)
      /// 
      /// 
      /// </summary>
      public static int ApplyCutPoint(PolyCutPoint cutPoint) {
        int vertIndex;
        //if (cutPoint.isNewPoint) {
        //  if (cutPoint.isEdgePoint) {
        //    if (!LowOp.SplitEdgeAddVertex(cutPoint.edge, cutPoint.amountAlongEdge,
        //                                  LowOp.EdgeDistanceMode.Normalized,
        //                                  out vertIndex)) {
        //      Debug.LogError("Error applying cut. (Splitting polygon edge.)");
        //    }
        //  }
        //  else {
        //    // c0 is a point _inside_ the face.
        //    if (!LowOp.PokePolygon(poly, cutPoint.newPosition, out vertIndex)) {
        //      Debug.LogError("Error applying cut. (Poking polygon.)");
        //    }
        //  }
        //}
        //else {
        //  // c0 is an existing point.
        //  vertIndex = cutPoint.existingPoint;
        //}
        throw new System.NotImplementedException();
        //return vertIndex;
      }

    }

    /// <summary>
    /// Low-level PolyMesh operations.
    /// </summary>
    public static class LowOp {

      public enum EdgeDistanceMode {
        Normalized,
        Absolute
      }

      public static bool SplitEdgeAddVertex(Edge e, float amountAlongEdge,
                                            EdgeDistanceMode edgeDistanceMode,
                                            out int addedVertId) {
        throw new System.NotImplementedException();
      }

      public static bool SplitPolygon(Polygon poly, int vertIdx0, int vertIdx1,
                                      out Polygon addedPoly0, out Polygon addedPoly1) {
        throw new System.NotImplementedException();
      }

      public static bool PokePolygon(Polygon poly, Vector3 position,
                                     out int addedVertId) {
        throw new System.NotImplementedException();
      }

    }

    #endregion

    #region Unity Mesh Conversion

    public void FillUnityMesh(Mesh mesh) {
      mesh.Clear();

      var verts   = Pool<List<Vector3>>.Spawn();
      verts.Clear();
      var faces   = Pool<List<int>>.Spawn();
      faces.Clear();
      var normals = Pool<List<Vector3>>.Spawn();
      normals.Clear();
      try {
        foreach (var poly in polygons) {
          var normal = poly.GetNormal(this);
          foreach (var tri in poly.tris) {
            foreach (var idx in tri) {
              faces.Add(verts.Count);
              verts.Add(P(idx));
              normals.Add(normal);
            }
          }
        }

        mesh.SetVertices(verts);
        mesh.SetTriangles(faces, 0, true);
        mesh.SetNormals(normals);
      }
      finally {
        verts.Clear();
        Pool<List<Vector3>>.Recycle(verts);
        faces.Clear();
        Pool<List<int>>.Recycle(faces);
        normals.Clear();
        Pool<List<Vector3>>.Recycle(normals);
      }
    }

    #endregion

  }

}