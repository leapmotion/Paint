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

    /// <summary> Updated when AddPolygon or RemovePolygon is called. </summary>
    private Dictionary<Edge, List<Polygon>> _edgeFaces;
    public Dictionary<Edge, List<Polygon>> edgeAdjFaces {
      get { return _edgeFaces; }
    }

    /// <summary> Updated when AddPolygon or RemovePolygon is called. </summary>
    private Dictionary<Polygon, List<Edge>> _faceEdges;
    public Dictionary<Polygon, List<Edge>> faceAdjEdges {
      get { return _faceEdges; }
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
      _faceEdges = new Dictionary<Polygon, List<Edge>>();
    }

    /// <summary>
    /// Creates a new PolyMesh using copies of the provided positions and the polygon.
    /// </summary>
    public PolyMesh(ReadonlyList<Vector3> positions, Polygon polygon)
      : this() {
      Fill(positions, polygons);
    }

    /// <summary>
    /// Creates a new PolyMesh using copies of the provided positions and polygons
    /// lists.
    /// </summary>
    public PolyMesh(ReadonlyList<Vector3> positions, ReadonlyList<Polygon> polygons)
      : this() {
      Fill(positions, polygons);
    }

    /// <summary>
    /// Creates a new PolyMesh by copying the elements of the positions and polygons
    /// enumerables.
    /// </summary>
    public PolyMesh(IEnumerable<Vector3> positions, IEnumerable<Polygon> polygons) 
      : this() {
      Fill(positions, polygons);
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
      _faceEdges.Clear();
    }

    /// <summary>
    /// Fills the PolyMesh with the provided positions and a single polygon.
    /// 
    /// The mesh is cleared first if it's not empty.
    /// </summary>
    public void Fill(ReadonlyList<Vector3> positions, Polygon polygon) {
      if (_positions.Count != 0) { Clear(); }

      AddPositions(positions);
      AddPolygon(polygon);
    }

    /// <summary>
    /// Fills the PolyMesh with the provided positions and polygons.
    /// 
    /// The mesh is cleared first if it's not empty.
    /// </summary>
    public void Fill(ReadonlyList<Vector3> positions, ReadonlyList<Polygon> polygons) {
      if (_positions.Count != 0) { Clear(); }

      AddPositions(positions);
      AddPolygons(polygons);
    }

    /// <summary>
    /// Fills the PolyMesh with the provided positions and polygons.
    /// 
    /// The mesh is cleared first if it's not empty.
    /// </summary>
    public void Fill(IEnumerable<Vector3> positions, IEnumerable<Polygon> polygons) {
      if (_positions.Count != 0) { Clear(); }

      foreach (var position in positions) {
        AddPosition(position);
      }
      foreach (var polygon in polygons) {
        AddPolygon(polygon);
      }
    }

    /// <summary>
    /// Replaces the positions in this PolyMesh with the positions in the argument list.
    /// 
    /// This operation is only valid if the provided list of positions is the same length
    /// as the original list of positions.
    /// </summary>
    public void FillPositionsOnly(List<Vector3> positions) {
      if (positions.Count != _positions.Count) {
        throw new System.InvalidOperationException("Cannot change the number of positions "
          + "using SetPositions.");
      }

      _positions.Clear();
      _positions.AddRange(positions);
    }

    /// <summary>
    /// Returns the position from the positions array of the argument vertex index.
    /// </summary>
    public Vector3 GetPosition(int vertIdx) {
      return P(vertIdx);
    }

    /// <summary>
    /// Returns the position from the positions array of the argument vertex index.
    /// </summary>
    private Vector3 P(int vertIdx) {
      return positions[vertIdx];
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
    public void AddPosition(Vector3 position, out int addedIndex) {
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
        // new mesh; this is common if e.g. combining two meshes into one mesh,
        // or adding just-initialized Polygons (without their mesh set).
        polygon.mesh = this;
      }

      _polygons.Add(polygon);

      updateEdges_PolyAdded(polygon);
    }

    public void RemovePolygons(IEnumerable<Polygon> toRemove) {
      var polyPool = Pool<HashSet<Polygon>>.Spawn();
      foreach (var polygon in toRemove) {
        polyPool.Add(polygon);

        updateEdges_PolyRemoved(polygon);
      }

      _polygons.RemoveAll(p => polyPool.Contains(p));
    }

    public void RemovePolygon(Polygon polygon) {
      _polygons.Remove(polygon);

      updateEdges_PolyRemoved(polygon);
    }

    #endregion

    #region Edge Data

    /// <summary>
    /// Adds edge data based on the added polygon.
    /// 
    /// This should be called right after a polygon is added to the mesh.
    /// (Automatically called by the AddPolygon functions.)
    /// </summary>
    private void updateEdges_PolyAdded(Polygon poly) {
      foreach (var edge in poly.edges) {
        List<Polygon> adjFaces;
        if (edgeAdjFaces.TryGetValue(edge, out adjFaces)) {
          adjFaces.Add(poly);
        }
        else {
          edgeAdjFaces[edge] = new List<Polygon>() { poly };
        }

        List<Edge> adjEdges;
        if (faceAdjEdges.TryGetValue(poly, out adjEdges)) {
          adjEdges.Add(edge);
        }
        else {
          faceAdjEdges[poly] = new List<Edge>() { edge };
        }
      }
    }

    /// <summary>
    /// Removes edge data based on the removed polygon.
    /// 
    /// This should be called right after a polygon is removed from the mesh.
    /// (Automatically called by the RemovePolygon functions.)
    /// </summary>
    private void updateEdges_PolyRemoved(Polygon poly) {
      foreach (var edge in poly.edges) {
        List<Polygon> adjFaces;
        if (edgeAdjFaces.TryGetValue(edge, out adjFaces)) {
          adjFaces.Remove(poly);

          if (adjFaces.Count == 0) {
            edgeAdjFaces.Remove(edge);
          }
        }
        else {
          throw new System.InvalidOperationException(
            "Adjacent polygon data for this edge never existed.");
        }
      }

      List<Edge> adjEdges;
      if (faceAdjEdges.TryGetValue(poly, out adjEdges)) {
        faceAdjEdges.Remove(poly);
      }
      else {
        throw new System.InvalidOperationException(
          "This polygon never had adjacent edge data.");
      }
    }

    public bool CheckValidEdge(Edge edge) {
      return _edgeFaces.ContainsKey(edge);
    }

    #endregion

    #region Operations

    /// <summary>
    /// High-level PolyMesh operations.
    /// </summary>
    public static class Ops {

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

      //public static void CutIntersectionLoops(PolyMesh A, PolyMesh B,
      //                                        out EdgeLoop aIntersection,
      //                                        out EdgeLoop bIntersection) {
      //  // Next steps.. this structure isn't right,
      //  // PolyMesh A and B may well have multiple edge loops that define their
      //  // intersections.
      //  throw new System.NotImplementedException();
      //}

    }

    /// <summary>
    /// Mid-level PolyMesh operations, lower-level than Op.
    /// </summary>
    public static class MidOps {

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

              if (TryCut(A, poly, fromPolyIdx, B, poly, out cutOp)) {
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
            throw new System.NotImplementedException();
            //EdgeSequence.Merge(edges, cutResult);

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

      public enum PolyCutPointType {
        Invalid,
        ExistingPoint,
        NewPointEdge,
        NewPointFace
      }

      public struct PolyCutPoint {
        public PolyCutPointType type;
        public Polygon polygon;

        // Existing Point data.
        public bool isExistingPoint { get { throw new System.NotImplementedException(); } }
        public int existingPoint { get { throw new System.NotImplementedException(); } }

        // New edge point data.
        public bool isNewEdgePoint { get { throw new System.NotImplementedException(); } }
        public Edge edge { get { throw new System.NotImplementedException(); } }
        public float amountAlongEdge { get { throw new System.NotImplementedException(); } }
        public EdgeDistanceMode edgeDistanceMode { get { throw new System.NotImplementedException(); } }

        // New face point data.
        public bool isNewFacePoint { get { throw new System.NotImplementedException(); } }
        public Vector3 newFacePoint { get { throw new System.NotImplementedException(); } }

        public void ConvertEdgeDistanceMode(EdgeDistanceMode newMode) {
          if (this.edgeDistanceMode == newMode) return;

          throw new System.NotImplementedException();
        }
      }

      /// <summary>
      /// Given two correctly configured PolyCutPoint structs, this method will apply
      /// those structs to their assigned polygon. This will modify that polygon's mesh.
      /// 
      /// Returns true if the cut configuration was valid; otherwise false is returned
      /// and the mesh is not modified.
      /// 
      /// If this method returns true, the polygon used to specify the PolyCutPoints will
      /// no longer be valid!
      /// </summary>
      public static bool TryApplyCut(PolyCutPoint c0, PolyCutPoint c1) {

        if (c0.type == PolyCutPointType.Invalid || c1.type == PolyCutPointType.Invalid) {
          Debug.LogError("Cannot cut using invalid PolyCutPoints.");
          return false;
        }

        if (c0.polygon != c1.polygon) {
          Debug.LogError("Cannot apply cut points from two different polygons.");
          return false;
        }

        var polygon = c0.polygon;

        if (c0.isExistingPoint && c1.isExistingPoint) {
          ApplyVertexVertexCut(polygon, c0.existingPoint, c1.existingPoint);
        }
        else {
          bool c0IsExistingPoint = false;
          if (c0.isExistingPoint) {
            c0IsExistingPoint = true;
          }
          else if (c1.isExistingPoint) {
            Utils.Swap(ref c0, ref c1);
            c0IsExistingPoint = true;
          }

          if (c0IsExistingPoint) {
            switch (c1.type) {
              case PolyCutPointType.NewPointEdge:
                // Vertex-Edge cut.
                ApplyVertexEdgeCut(polygon, c0.existingPoint,
                                   c1.edge, c1.amountAlongEdge, c1.edgeDistanceMode);
                break;
              case PolyCutPointType.NewPointFace:
                // Vertex-Face cut.
                ApplyVertexFaceCut(polygon, c0.existingPoint, c1.newFacePoint);
                break;
              default:
                Debug.LogError("Invalid PolyCutPointType for second cut point.");
                return false;
            }
          }
          else {
            bool c0IsEdgePoint = false;
            if (c0.isNewEdgePoint) {
              c0IsEdgePoint = true;
            }
            else if (c1.isNewEdgePoint) {
              Utils.Swap(ref c0, ref c1);
              c0IsEdgePoint = true;
            }

            if (c0IsEdgePoint) {
              switch (c1.type) {
                case PolyCutPointType.NewPointEdge:
                  if (c0.edgeDistanceMode != c1.edgeDistanceMode) {
                    c1.ConvertEdgeDistanceMode(c0.edgeDistanceMode);
                  }
                  if (c0.edge == c1.edge) {
                    Debug.LogError("Error applying cut: Cannot edge-edge cut the same "
                                 + "edge.");
                    return false;
                  }
                  ApplyEdgeEdgeCut(polygon,
                                   c0.edge, c0.amountAlongEdge,
                                   c1.edge, c1.amountAlongEdge,
                                   c0.edgeDistanceMode);
                  break;
                case PolyCutPointType.NewPointFace:
                  ApplyEdgeFaceCut(polygon,
                                   c0.edge, c0.amountAlongEdge, c0.edgeDistanceMode,
                                   c1.newFacePoint);
                  break;
              }
            }
            else {
              // c0 and c1 must both be face points.
              if (c0.isNewFacePoint & c1.isNewFacePoint) {
                ApplyFaceFaceCut(polygon, c0.newFacePoint, c1.newFacePoint);
              }
              else {
                Debug.LogError("Logic error resolving cut points! Couldn't find correct "
                             + "cut type resolution.");
                return false;
              }
            }
          }
        }

        return true;
      }

      public static void ApplyVertexVertexCut(Polygon polygon, int vert0, int vert1) {
        LowOp.SplitPolygon(polygon, vert0, vert1);
      }

      public static void ApplyVertexEdgeCut(Polygon polygon, int vert,
                                            Edge edge, float amountAlongEdge,
                                            EdgeDistanceMode edgeDistanceMode) {
        int     addedVertId;
        Polygon equivalentPolygon;
        Edge    addedEdge0, addedEdge1; // unused.
        LowOp.SplitEdgeAddVertex(edge, amountAlongEdge, edgeDistanceMode,
                                      out addedVertId,
                                      out addedEdge0, out addedEdge1,
                                      polygon,
                                      out equivalentPolygon);
        LowOp.SplitPolygon(equivalentPolygon, vert, addedVertId);
      }

      public static void ApplyEdgeEdgeCut(Polygon polygon,
                                          Edge edge0, float amountAlongEdge0,
                                          Edge edge1, float amountAlongEdge1,
                                          EdgeDistanceMode edgeDistanceMode) {

        int     addedVertId0;
        Polygon equivalentPolygon;
        Edge    addedEdge00, addedEdge01;
        LowOp.SplitEdgeAddVertex(edge0, amountAlongEdge0, edgeDistanceMode,
                                 out addedVertId0,
                                 out addedEdge00, out addedEdge01,
                                 polygon,
                                 out equivalentPolygon);

        // After the first edge split, our other edge should still be a valid edge.
        // Let's check just in case something went terribly wrong!
        if (!equivalentPolygon.mesh.CheckValidEdge(edge1)) {
          throw new System.InvalidOperationException("Error performing edge-edge cut; "
            + "edge1 was invalidated after edge0 split operation.");
        }

        int addedVertId1;
        Edge addedEdge10, addedEdge11;
        LowOp.SplitEdgeAddVertex(edge1, amountAlongEdge1, edgeDistanceMode,
                                 out addedVertId1,
                                 out addedEdge10,
                                 out addedEdge11,
                                 equivalentPolygon,
                                 out equivalentPolygon);

        LowOp.SplitPolygon(equivalentPolygon, addedVertId0, addedVertId1);

      }

      public static void ApplyVertexFaceCut(Polygon polygon, int vert,
                                            Vector3 facePosition) {

        int addedVertId;
        LowOp.PokePolygon(polygon, facePosition, out addedVertId, null, null, vert);

      }

      public static void ApplyEdgeFaceCut(Polygon polygon,
                                          Edge edge, float amountAlongEdge,
                                          EdgeDistanceMode edgeDistanceMode,
                                          Vector3 facePosition) {
        int     addedVertId0;
        Polygon equivalentPolygon;
        Edge    addedEdge0, addedEdge1; // unused.
        LowOp.SplitEdgeAddVertex(edge, amountAlongEdge, edgeDistanceMode,
                                      out addedVertId0,
                                      out addedEdge0, out addedEdge1,
                                      polygon,
                                      out equivalentPolygon);

        int addedVertId1;
        LowOp.PokePolygon(equivalentPolygon, facePosition,
                          out addedVertId1,
                          ensureEdgeToVertex: addedVertId0);

      }

      public static void ApplyFaceFaceCut(Polygon polygon,
                                          Vector3 facePosition0,
                                          Vector3 facePosition1) {

        // TODO: There should be a specific primitive operation for a double-poke.

        var addedPolys = Pool<List<Polygon>>.Spawn();
        addedPolys.Clear();
        var addedEdges = Pool<List<Edge>>.Spawn();
        addedPolys.Clear();
        int addedVertId0;
        try {
          LowOp.PokePolygon(polygon, facePosition0,
                            out addedVertId0,
                            addedPolys,
                            addedEdges);

          var edgeWithNewPoint = addedEdges.Query().Where(e => facePosition1.IsInside(e))
                                                   .FirstOrDefault();
          if (edgeWithNewPoint == default(Edge)) {
            // Second point is inside a face, not on an edge.
            var secondPoly = addedPolys.Query().Where(p => facePosition1.IsInside(p))
                                               .FirstOrDefault();

            int addedVertId1;
            LowOp.PokePolygon(secondPoly, facePosition1, out addedVertId1);
          }
          else {
            // Second point is on one of the newly-created edges.
            LowOp.SplitEdgeAddVertex(edgeWithNewPoint, facePosition1);
          }

        }
        finally {
          addedPolys.Clear();
          Pool<List<Polygon>>.Recycle(addedPolys);
        }

      }

    }

    /// <summary>
    /// Low-level PolyMesh operations.
    /// </summary>
    public static class LowOp {

      #region SplitEdgeAddVertex

      /// <summary>
      /// Splits an edge, adding a new position to the edge's polygon to do so. This
      /// version of the function assumes that you've already calculated the target
      /// vertex position -- this MUST be on the edge!
      /// 
      /// This operation invalidates the argument Edge and any polygons it is attached to!
      /// However, you can provide a Polygon as an additional argument and receive back
      /// the equivalent Polygon after the operation is completed.
      /// </summary>
      public static void SplitEdgeAddVertex(Edge edge,
                                            Vector3 newEdgePosition,
                                            out int addedVertId,
                                            out Edge addedEdge0,
                                            out Edge addedEdge1,
                                            Polygon? receiveEquivalentPolygon,
                                            out Polygon equivalentPolygon) {

        var mesh = edge.mesh;
        mesh.AddPosition(newEdgePosition, out addedVertId);
        addedEdge0 = new Edge() { mesh = mesh, a = edge.a, b = addedVertId };
        addedEdge1 = new Edge() { mesh = mesh, a = addedVertId, b = edge.b };

        equivalentPolygon = default(Polygon);
        bool tryReceiveEquivalentPolygon = receiveEquivalentPolygon.HasValue;

        var edgePolys = Pool<List<Polygon>>.Spawn();
        try {
          var origPolys = mesh.edgeAdjFaces[edge];

          edgePolys.AddRange(origPolys);

          // Each of these polygons needs to be reconstructed to incorporate the new
          // vertex.
          mesh.RemovePolygons(edgePolys);
          bool foundEquivalentPolygon = false;
          bool equivalentPolygonAssigned = false;
          foreach (var polygon in edgePolys) {
            if (!foundEquivalentPolygon && tryReceiveEquivalentPolygon) {
              if (polygon == receiveEquivalentPolygon.Value) {
                foundEquivalentPolygon = true;
              }
            }

            polygon.InsertEdgeVertex(edge, addedVertId);
            mesh.AddPolygon(polygon);

            if (!equivalentPolygonAssigned && foundEquivalentPolygon) {
              equivalentPolygonAssigned = true;
              equivalentPolygon = polygon;
            }
          }

          if (!foundEquivalentPolygon && tryReceiveEquivalentPolygon) {
            throw new System.InvalidOperationException(
              "receiveEquivalentPolygon was specified, but no corresponding polygon was "
            + "found attached to the argument edge being split.");
          }
        }
        finally {
          edgePolys.Clear();
          Pool<List<Polygon>>.Recycle(edgePolys);
        }

      }

      /// <summary>
      /// Splits an edge, adding a new position to the edge's polygon to do so. This
      /// version of the function assumes that you've already calculated the target
      /// vertex position -- this MUST be on the edge!
      /// 
      /// This operation invalidates the argument Edge and any polygons it is attached to!
      /// However, you can provide a Polygon as an additional argument and receive back
      /// the equivalent Polygon after the operation is completed.
      /// </summary>
      public static void SplitEdgeAddVertex(Edge edge, Vector3 newEdgePosition,
                                            out int addedVertId,
                                            out Edge addedEdge0,
                                            out Edge addedEdge1) {
        Polygon unusedEquivalentPolygon;
        SplitEdgeAddVertex(edge, newEdgePosition,
                           out addedVertId, out addedEdge0, out addedEdge1,
                           null, out unusedEquivalentPolygon);
      }

      /// <summary>
      /// Splits an edge, adding a new position to the edge's polygon to do so. This
      /// version of the function assumes that you've already calculated the target
      /// vertex position -- this MUST be on the edge!
      /// 
      /// This operation invalidates the argument Edge and any polygons it is attached to!
      /// However, you can provide a Polygon as an additional argument and receive back
      /// the equivalent Polygon after the operation is completed.
      /// </summary>
      public static void SplitEdgeAddVertex(Edge edge, Vector3 newEdgePosition) {
        int addedVertId;
        Edge addedEdge0, addedEdge1;
        SplitEdgeAddVertex(edge, newEdgePosition,
                           out addedVertId,
                           out addedEdge0, out addedEdge1);
      }

      /// <summary>
      /// Splits an edge, adding a new position to the edge's polygon to do so.
      /// 
      /// This operation invalidates the argument Edge and any polygons it is attached to!
      /// However, you can provide a Polygon as an additional argument and receive back
      /// the equivalent Polygon after the operation is completed.
      /// </summary>
      public static void SplitEdgeAddVertex(Edge edge, float amountAlongEdge,
                                            EdgeDistanceMode edgeDistanceMode,
                                            out int     addedVertId,
                                            out Edge    addedEdge0,
                                            out Edge    addedEdge1,
                                            Polygon?    receiveEquivalentPolygon,
                                            out Polygon equivalentPolygon) {

        var newEdgePosition = edge.GetPositionAlongEdge(amountAlongEdge, edgeDistanceMode);

        SplitEdgeAddVertex(edge, newEdgePosition,
                           out addedVertId, out addedEdge0, out addedEdge1,
                           receiveEquivalentPolygon, out equivalentPolygon);
      }

      /// <summary>
      /// Splits an edge, adding a new position to the edge's polygon to do so.
      /// 
      /// This operation invalidates the argument Edge and any polygons it is attached to!
      /// However, you can provide a Polygon as an additional argument and receive back
      /// the equivalent Polygon after the operation is completed.
      /// </summary>
      public static void SplitEdgeAddVertex(Edge edge, float amountAlongEdge,
                                            EdgeDistanceMode edgeDistanceMode,
                                            out int addedVertId,
                                            out Edge addedEdge0,
                                            out Edge addedEdge1) {
        Polygon unusedEquivalentPolygon;
        SplitEdgeAddVertex(edge, amountAlongEdge, edgeDistanceMode,
                           out addedVertId, out addedEdge0, out addedEdge1,
                           null, out unusedEquivalentPolygon);
      }

      #endregion

      #region SplitPolygon

      /// <summary>
      /// Splits a polygon by removing it from the mesh and adding two new polygons with
      /// an edge defined between vertIdx0 and vertIdx1.
      /// 
      /// Optionally provides the resulting new edge and the two new polygons back as
      /// out parameters.
      /// </summary>
      public static void SplitPolygon(Polygon poly,
                                      int vertIdx0, int vertIdx1,
                                      out Edge    addedEdge,
                                      out Polygon addedPoly0,
                                      out Polygon addedPoly1) {

        var mesh = poly.mesh;

        // In a single cycle through all vertices, tag them as A, B, or both:
        // Verts on the split boundary are added to BOTH polygons, so should have A and B
        // Verts on either side of the boundary merely need to be A or B
        bool useBufferA = true;
        var vertsBufferA = Pool<List<int>>.Spawn();
        vertsBufferA.Clear();
        var vertsBufferB = Pool<List<int>>.Spawn();
        vertsBufferB.Clear();
        try {
          foreach (var vertIndex in poly.verts) {
            if (vertIndex == vertIdx0 || vertIndex == vertIdx1) {
              // Split boundary detected.
              vertsBufferA.Add(vertIndex);
              vertsBufferB.Add(vertIndex);
              useBufferA = !useBufferA;
            }
            else if (useBufferA) {
              vertsBufferA.Add(vertIndex);
            }
            else {
              vertsBufferB.Add(vertIndex);
            }
          }

          addedPoly0 = new Polygon() {
            mesh = mesh,
            verts = vertsBufferA.Query().ToList()
          };

          addedPoly1 = new Polygon() {
            mesh = mesh,
            verts = vertsBufferB.Query().ToList()
          };
        }
        finally {
          vertsBufferA.Clear();
          Pool<List<int>>.Recycle(vertsBufferA);
          vertsBufferB.Clear();
          Pool<List<int>>.Recycle(vertsBufferB);
        }

        addedEdge = new Edge() { mesh = mesh, a = vertIdx0, b = vertIdx1 };

        mesh.RemovePolygon(poly);
        mesh.AddPolygon(addedPoly0);
        mesh.AddPolygon(addedPoly1);
      }

      /// <summary>
      /// Splits a polygon by removing it from the mesh and adding two new polygons with
      /// an edge defined between vertIdx0 and vertIdx1.
      /// 
      /// Optionally provides the resulting new edge and the two new polygons back as
      /// out parameters.
      /// </summary>
      public static void SplitPolygon(Polygon poly, int vertIdx0, int vertIdx1) {
        Edge    addedEdge;
        Polygon addedPoly0, addedPoly1;
        SplitPolygon(poly, vertIdx0, vertIdx1,
                     out addedEdge,
                     out addedPoly0, out addedPoly1);
      }

      #endregion

      #region PokePolygon

      /// <summary>
      /// Shatters a polygon into smaller pieces by adding a new position to the mesh at
      /// the specified position -- which must be IN the argument polygon.
      /// 
      /// This operation does NOT guarantee that a new edge will exist from the argument
      /// position to any specific vertex index in the shattered polygon, because the
      /// poke does not necessarily break the polygon into triangles.
      /// 
      /// Instead, the operation attempts to produce fewer polygons by
      /// combining triangles into larger polygons as long as they remain convex.
      /// (This behavior is not guaranteed to be optimal.)
      /// 
      /// You can provide a non-null index to "ensureEdgeToVertex" to guarantee that a
      /// new edge will be created from that index to the poked vertex.
      /// </summary>
      public static void PokePolygon(Polygon poly, Vector3 position,
                                     out int addedVertId,
                                     List<Polygon> outAddedPolygonsList = null,
                                     List<Edge> outAddedEdgesList = null,
                                     int? ensureEdgeToVertex = null) {

        var mesh = poly.mesh;
        
        mesh.AddPosition(position, out addedVertId);

        var addedPolygons = Pool<List<Polygon>>.Spawn();
        addedPolygons.Clear();
        var addedEdgesSet = Pool<HashSet<Edge>>.Spawn();
        addedEdgesSet.Clear();
        try {
          int fromIdx = 0;
          while (fromIdx < poly.verts.Count) {
            var fragmentPoly = new Polygon() {
              mesh = mesh,
              verts = new List<int>() { addedVertId }
            };

            foreach (var vertIdx in poly.verts.FromIndex(fromIdx)) {
              if (poly.Count <= 2) {
                poly.verts.Add(vertIdx);
                fromIdx++;
              }
              else {
                poly.verts.Add(vertIdx);
                if (!poly.IsConvex()) {
                  poly.verts.RemoveAt(poly.verts.Count - 1);
                  break;
                }
                else {
                  fromIdx++;
                }
              }
            }

            if (fragmentPoly.Count < 3) {
              throw new System.InvalidOperationException(
                "PokePolygon exception; produced a fragment polygon with < 3 verts.");
            }

            addedPolygons.Add(fragmentPoly);
            if (outAddedEdgesList != null) {
              foreach (var edge in fragmentPoly.edges) {
                addedEdgesSet.Add(edge);
              }
            }
          }

          if (outAddedPolygonsList != null) {
            outAddedPolygonsList.Clear();
            outAddedPolygonsList.AddRange(addedPolygons);
          }

          if (outAddedEdgesList != null) {
            outAddedEdgesList.Clear();
            foreach (var edge in addedEdgesSet) {
              outAddedEdgesList.Add(edge);
            }
          }

          mesh.RemovePolygon(poly);
          mesh.AddPolygons(addedPolygons);

        }
        finally {
          addedPolygons.Clear();
          Pool<List<Polygon>>.Recycle(addedPolygons);
          addedEdgesSet.Clear();
          Pool<HashSet<Edge>>.Recycle(addedEdgesSet);
        }

      }

      #endregion

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