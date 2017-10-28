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

    #region Transform Support

    /// <summary>
    /// If this value is non-null, GetPosition() will return transformed positions.
    /// To get local positions always, use GetLocalPosition().
    /// </summary>
    public Transform useTransform = null;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a new, empty PolyMesh. The PolyMesh will use the provided Transform to
    /// know where it is in world-space; this is optional, but important if you intend
    /// to perform operations on this PolyMesh with respect to other PolyMeshes that
    /// correspond to Unity transforms in a scene.
    /// </summary>
    public PolyMesh() {
      _positions = new List<Vector3>();
      _polygons = new List<Polygon>();
      _edgeFaces = new Dictionary<Edge, List<Polygon>>();
      _faceEdges = new Dictionary<Polygon, List<Edge>>();
    }

    /// <summary>
    /// Creates a new, empty PolyMesh. The PolyMesh will use the provided Transform to
    /// know where it is in world-space; this is optional, but important if you intend
    /// to perform operations on this PolyMesh with respect to other PolyMeshes that
    /// correspond to Unity transforms in a scene.
    /// </summary>
    public PolyMesh(Transform useTransform)
    : this() {
      if (useTransform != null) {
        this.useTransform = useTransform;
      }
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
      Fill(positions, polygons, PositionMode.World);
    }

    #endregion

    #region Basic Operations

    /// <summary>
    /// Clears the PolyMesh.
    /// </summary>
    public void Clear(bool clearTransform = true) {
      _positions.Clear();
      _polygons.Clear();
      _edgeFaces.Clear();
      _faceEdges.Clear();

      if (clearTransform) {
        useTransform = null;
      }
    }

    public enum PositionMode { Local, World }

    /// <summary>
    /// Fills this PolyMesh with data from the other PolyMesh (via deep copy). The result
    /// is an identical but independent mesh.
    /// 
    /// This mesh is cleared first if it's not empty.
    /// </summary>
    public void Fill(PolyMesh otherPolyMesh) {
      if (_positions.Count != 0) { Clear(clearTransform: true); }

      this.useTransform = otherPolyMesh.useTransform;

      AddPositions(otherPolyMesh.positions);
      AddPolygons(otherPolyMesh.polygons, copyPolygonVertLists: true);
    }

    /// <summary>
    /// Fills the PolyMesh with the provided positions and a single polygon.
    /// 
    /// The mesh is cleared first if it's not empty.
    /// </summary>
    public void Fill(ReadonlyList<Vector3> positions, Polygon polygon) {
      if (_positions.Count != 0) { Clear(clearTransform: false); }

      AddPositions(positions);
      AddPolygon(polygon);
    }

    /// <summary>
    /// Fills the PolyMesh with the provided positions and polygons.
    /// 
    /// The mesh is cleared first if it's not empty.
    /// </summary>
    public void Fill(ReadonlyList<Vector3> positions, ReadonlyList<Polygon> polygons) {
      if (_positions.Count != 0) { Clear(clearTransform: false); }

      AddPositions(positions);
      AddPolygons(polygons);
    }

    /// <summary>
    /// Fills the PolyMesh with the provided positions and polygons.
    /// 
    /// The mesh is cleared first if it's not empty.
    /// </summary>
    public void Fill(IEnumerable<Vector3> positions, IEnumerable<Polygon> polygons,
                     PositionMode mode) {
      if (_positions.Count != 0) { Clear(clearTransform: false); }

      int addedIdx;
      foreach (var position in positions) {
        AddPosition(position, out addedIdx, mode);
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
      if (useTransform != null) {
        return useTransform.TransformPoint(GetLocalPosition(vertIdx));
      }
      return GetLocalPosition(vertIdx);
    }

    /// <summary>
    /// Returns the position from the positions array of the argument vertex index.
    /// </summary>
    public Vector3 GetLocalPosition(int vertIdx) {
      return positions[vertIdx];
    }

    /// <summary>
    /// Adds a new position to this PolyMesh. Optionally provide the index of the added
    /// position.
    /// </summary>
    public void AddPosition(Vector3 position) {
      int addedIdx;
      AddPosition(position, out addedIdx);
    }

    /// <summary>
    /// Adds a new position to this PolyMesh. Optionally provide the index of the added
    /// position.
    /// 
    /// Note: If the useTransform property of this PolyMesh is non-null, it will be used
    /// to inverse-transform the given position into mesh-local-space before adding the
    /// point.
    /// </summary>
    public void AddPosition(Vector3 position, out int addedIndex,
                            PositionMode mode = PositionMode.World) {
      addedIndex = _positions.Count;
      if (useTransform != null && mode != PositionMode.Local) {
        _positions.Add(useTransform.InverseTransformPoint(position));
      }
      else {
        _positions.Add(position);
      }
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
    /// 
    /// If copyPolygonVertLists is set to true, the polygons added to this PolyMesh will
    /// create new List instances for their vert data; this is used when performing a
    /// deep copy from one PolyMesh to another.
    /// </summary>
    public void AddPolygons(ReadonlyList<Polygon> polygons,
                            bool copyPolygonVertLists = false) {
      foreach (var poly in polygons) {
        AddPolygon(poly, copyPolygonVertLists);
      }
    }

    /// <summary>
    /// Adds a polygon to this PolyMesh.
    /// 
    /// If copyPolygonVertList is set to true, the verts from the polygon will be copied
    /// into a new List. Use this when performing a deep copy from one PolyMesh to
    /// another, otherwise you may have two Polygons that share pointers to the same
    /// vertex index list.
    /// </summary>
    public void AddPolygon(Polygon polygon, bool copyPolygonVertList = false) {
      if (polygon.mesh != this) {
        // We assume that a polygon passed to a new mesh is supposed to be a part of that
        // new mesh; this is common if e.g. combining two meshes into one mesh,
        // or adding just-initialized Polygons (without their mesh set).
        polygon.mesh = this;
      }

      if (copyPolygonVertList) {
        var origList = polygon.verts;
        polygon.verts = new List<int>();
        polygon.verts.AddRange(origList);
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
      }

      List<Edge> adjEdges;
      if (faceAdjEdges.TryGetValue(poly, out adjEdges)) {
        throw new System.InvalidOperationException(
          "Already have edge data for this polygon somehow.");
      }
      else {
        var edgeList = Pool<List<Edge>>.Spawn();
        edgeList.Clear();
        foreach (var edge in poly.edges) {
          edgeList.Add(edge);
        }
        faceAdjEdges[poly] = edgeList;
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

      #region Combine

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

      #endregion

      #region Cut

      /// <summary>
      /// Cuts A using B, modifying A. B is neither cut nor modified. The cut operation
      /// produces new edges on A along the intersection of A and B.
      /// 
      /// Return true if the operation applied any valid cut. Note that if A and B only
      /// share edges or vertex positions, no cut is necessary, and the method will
      /// return false.
      /// 
      /// Edge and vertex sharing is resolved against resolution specified by
      /// PolyMath.POSITION_TOLERANCE. (Tolerances that are too low can produce
      /// extremely thin triangles and degenerate behaviour due to floating-point error.)
      /// 
      /// If a non-null List is provided as outCutEdges, it will be appended with edges
      /// on A that form the cut.
      /// </summary>
      public static bool Cut(PolyMesh A, PolyMesh B,
                             List<Edge> outCutEdges = null) {
        bool anySuccessful = false;
        foreach (var cuttingPoly in B.polygons) {
          anySuccessful |= CutOps.TryCutWithPoly(A, B, cuttingPoly, outCutEdges);
        }

        return anySuccessful;
      }

      /// <summary>
      /// Cuts A and B with one another, producing new edges on A and B along their
      /// mutual intersections.
      /// 
      /// Return true if the operation applied any valid cut. Note that if A and B only
      /// share edges or vertex positions, no cut is necessary, and the method will
      /// return false.
      /// 
      /// If outCutEdgesA or outCutEdgesB are provided and non-null, they will
      /// be appended with the edges that form the cut on their respective mesh.
      /// </summary>
      public static bool DualCut(PolyMesh A, PolyMesh B,
                                 List<Edge> outCutEdgesA = null,
                                 List<Edge> outCutEdgesB = null) {

        // Copy A's current polygons so we can use them to cut into B, instead of using
        // the polygons of A after its cut, which would take longer as there would be
        // more of them.
        var aPolysCopy = Pool<List<Polygon>>.Spawn();
        aPolysCopy.Clear();
        var cutEdgesA = Pool<List<Edge>>.Spawn();
        var cutEdgesB = Pool<List<Edge>>.Spawn();
        try {
          foreach (var polygon in A.polygons) {
            // We don't need to deep-copy the polygons; when they're removed from A
            // and replaced, A will receive new polygons rather than re-using its
            // removed polygons.

            aPolysCopy.Add(polygon);
          }

          // Cut A using B.
          bool anySuccessful = false;
          foreach (var cuttingPoly in B.polygons) {
            anySuccessful |= CutOps.TryCutWithPoly(A, B, cuttingPoly, cutEdgesA);
          }

          // TODO: MAJOR OPTIMIZATION:
          // Way deeper down, CutOps should have a DualCutOp equivalent:
          // Any valid CutOp on a polygon in A has a dual CutOp on a polygon in B;
          // when any cut on an A polygon is applied, we can ALSO apply a cut on B.

          if (!anySuccessful) {
            // Early out -- there won't be any cuts the other direction if there weren't
            // any to begin with.
            return false;
          }
          else {
            foreach (var cuttingPoly in aPolysCopy) {
              CutOps.TryCutWithPoly(B, A, cuttingPoly, cutEdgesB);
            }

            if (outCutEdgesA != null) {
              foreach (var edge in cutEdgesA) {
                outCutEdgesA.Add(edge);
              }
            }
            if (outCutEdgesB != null) {
              foreach (var edge in cutEdgesB) {
                outCutEdgesB.Add(edge);
              }
            }

            return true;
          }
        }
        finally {
          aPolysCopy.Clear();
          Pool<List<Polygon>>.Recycle(aPolysCopy);
          cutEdgesA.Clear();
          Pool<List<Edge>>.Recycle(cutEdgesA);
          cutEdgesB.Clear();
          Pool<List<Edge>>.Recycle(cutEdgesB);
        }
      }

      #endregion

      #region Subtract (not yet implemented)

      // DELETEME
      private static void RenderEdge(Edge e) {
        RuntimeGizmos.RuntimeGizmoDrawer drawer;
        if (RuntimeGizmos.RuntimeGizmoManager.TryGetGizmoDrawer(out drawer)) {
          drawer.color = LeapColor.mint;

          var a = e.GetPositionAlongEdge(0f);
          var b = e.GetPositionAlongEdge(1f);

          drawer.DrawWireCapsule(a, b, 0.02f);
          drawer.DrawWireCapsule(a, b, 0.03f);
          drawer.DrawWireCapsule(a, b, 0.04f);
        }
      }

      public static void Subtract(PolyMesh A, PolyMesh B, PolyMesh intoPolyMesh) {

        // For now, let's just get all the edges as a result of the cut of the two meshes
        // and render them.

        var aEdges = Pool<List<Edge>>.Spawn();
        aEdges.Clear();
        var bEdges = Pool<List<Edge>>.Spawn();
        bEdges.Clear();
        var tempAMesh = Pool<PolyMesh>.Spawn();
        var tempBMesh = Pool<PolyMesh>.Spawn();
        try {

          tempAMesh.Fill(A);
          tempBMesh.Fill(B);

          if (!DualCut(A, B, aEdges, bEdges)) {
            Debug.LogError("No cut occurred.");
          }

          foreach (var aEdge in aEdges) {
            RenderEdge(aEdge);
          }

        }
        finally {
          aEdges.Clear();
          Pool<List<Edge>>.Recycle(aEdges);
          bEdges.Clear();
          Pool<List<Edge>>.Recycle(bEdges);
          tempAMesh.Clear();
          tempBMesh.Clear();
          Pool<PolyMesh>.Recycle(tempAMesh);
          Pool<PolyMesh>.Recycle(tempBMesh);
        }

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

      #endregion

    }

    /// <summary>
    /// PolyMesh Cut operation support.
    /// </summary>
    protected static class CutOps {

      public enum PolyCutPointType {
        Invalid,
        ExistingPoint,
        NewPointEdge,
        NewPointFace
      }

      public struct PolyCutPoint {
        public Polygon polygon;

        private PolyCutPointType _type;
        public PolyCutPointType type { get { return _type; } }

        private Maybe<int>     _maybeExistingPoint;
        private Maybe<Vector3> _maybeNewPoint;
        private Maybe<Edge>    _maybeNewPointEdge;

        public bool Equals(PolyCutPoint other) {
          return this.polygon == other.polygon
              && this.type == other.type
              && _maybeExistingPoint == other._maybeExistingPoint
              && _maybeNewPoint == other._maybeNewPoint
              && _maybeNewPointEdge == other._maybeNewPointEdge;
        }
        public override bool Equals(object obj) {
          if (obj is PolyCutPoint) {
            return Equals((PolyCutPoint)obj);
          }
          return base.Equals(obj);
        }
        public override int GetHashCode() {
          return new Hash() { polygon, type, _maybeExistingPoint, _maybeNewPoint, _maybeNewPointEdge };
        }
        public static bool operator ==(PolyCutPoint one, PolyCutPoint other) {
          return one.Equals(other);
        }
        public static bool operator !=(PolyCutPoint one, PolyCutPoint other) {
          return !(one.Equals(other));
        }

        public PolyCutPoint(Vector3 desiredPointOnPoly, Polygon onPoly) {
          polygon = onPoly;

          _type = PolyCutPointType.Invalid;
          _maybeExistingPoint = Maybe.None;
          _maybeNewPoint = Maybe.None;
          _maybeNewPointEdge = Maybe.None;

          _maybeNewPoint = desiredPointOnPoly;

          for (int i = 0; i < polygon.verts.Count; i++) {
            var curVertPos = polygon.GetMeshPosition(polygon[i]);
            if (Vector3.Distance(curVertPos, desiredPointOnPoly) < PolyMath.POSITION_TOLERANCE) {
              _maybeExistingPoint = polygon[i];
              _maybeNewPointEdge = Maybe.None;
              _maybeNewPoint = Maybe.None;
              break;
            }
            else {
              var edge = new Edge() {
                mesh = polygon.mesh,
                a = polygon[i],
                b = polygon[i + 1]
              };
              if (desiredPointOnPoly.IsInside(edge)) {
                _maybeNewPointEdge = edge;
              }
            }
          }

          if (_maybeExistingPoint.hasValue) {
            _type = PolyCutPointType.ExistingPoint;
          }
          else if (_maybeNewPointEdge.hasValue) {
            _type = PolyCutPointType.NewPointEdge;
          }
          else {
            _type = PolyCutPointType.NewPointFace;
          }

          if (this.isMalformed) {
            throw new System.InvalidOperationException("Cut point malformed.");
          }
        }

        // TODO: deleteme..?
        public bool isMalformed {
          get {
            switch (type) {
              case PolyCutPointType.Invalid:
                return true;
              case PolyCutPointType.ExistingPoint:
                return _maybeNewPoint.hasValue
                    || _maybeNewPointEdge.hasValue;
              case PolyCutPointType.NewPointEdge:
                return _maybeExistingPoint.hasValue
                   || !_maybeNewPointEdge.hasValue
                   || !_maybeNewPoint.hasValue;
              case PolyCutPointType.NewPointFace:
                return _maybeExistingPoint.hasValue
                    || _maybeNewPointEdge.hasValue
                    || !_maybeNewPoint.hasValue;
              default:
                return false;
            }
          }
        }

        // Existing Point data.
        public bool isExistingPoint { get { return _type == PolyCutPointType.ExistingPoint; } }
        public int existingPoint { get { return _maybeExistingPoint.valueOrDefault; } }

        // New edge point data.
        public bool isNewEdgePoint { get { return _type == PolyCutPointType.NewPointEdge; } }
        public Edge edge { get { return _maybeNewPointEdge.valueOrDefault; } }

        // New face point data.
        public bool isNewFacePoint { get { return _type == PolyCutPointType.NewPointFace; } }
        public Vector3 newPoint { get { return _maybeNewPoint.valueOrDefault; } }

        public Vector3 GetPosition() {
          if (isExistingPoint) {
            return polygon.mesh.GetPosition(existingPoint);
          }
          else {
            return newPoint;
          }
        }
      }

      /// <summary>
      /// Cuts into A, using PolyMesh B's polygon b. This operation modifies A, producing
      /// extra positions if necessary, and increasing the number of faces (polygons) if
      /// the cut succeeds.
      /// 
      /// The cut is "successful" if the cut attempt produces at least one new edge of
      /// non-zero length between two position indices, which may be added to the
      /// positions list for the purposes of the cut.
      /// 
      /// If a non-null list is provided as outCutEdges, it will be appended with
      /// edges that form the cut, whether or not a new cut was actually required!
      /// </summary>
      public static bool TryCutWithPoly(PolyMesh meshToCut,
                                        PolyMesh cutWithMesh, Polygon cutWithMeshPoly,
                                        List<Edge> outCutEdges = null) {

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
          // In effect, this function slowly reduces cuts to their most trivial form,
          // No cut being possible, or all cuts already exist, for all polygon cut
          // attempts.
          Maybe<PolyCutOp> cutOp = Maybe.None;
          int loopCount = 0;
          int fromPolyIdx = 0;
          while (fromPolyIdx < meshToCut.polygons.Count && loopCount < PolyMath.MAX_LOOPS) {

            for (; fromPolyIdx < meshToCut.polygons.Count; ) {

              if (TryCreateCutOp(meshToCut.polygons[fromPolyIdx],
                                 fromPolyIdx,
                                 cutWithMeshPoly,
                                 out cutOp)) {
                break;
              }
              else {
                fromPolyIdx += 1;
              }
            }

            // Apply a cut operation if we have one, then consume it.
            if (cutOp.hasValue) {
              if (!cutOp.valueOrDefault.representsNewCut) {
                // Increment fromIdx still, because there was no actual polygon change.
                fromPolyIdx += 1;
              }

              Edge cutEdge;
              ApplyCut(cutOp.valueOrDefault, out cutEdge);
              edges.Add(cutEdge);

              cutOp = Maybe.None;
            }

            loopCount++;
          }
          if (loopCount == PolyMath.MAX_LOOPS) {
            throw new System.InvalidOperationException(
              "Hit maximum loops trying to iterate through meshToCut's polygons.");
          }

          if (edges.Count > 0) {
            outCutEdges.AddRange(edges);

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
      /// A data object containing two cut points, representing a cut operation on a
      /// single polygon.
      /// </summary>
      public struct PolyCutOp {
        public PolyCutPoint c0;
        public PolyCutPoint c1;
        public bool representsNewCut;
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
      public static bool TryCreateCutOp(Polygon aPoly, int aPolyIdx,
                                        Polygon bPoly,
                                        out Maybe<PolyCutOp> maybeCutOp) {

        var aPolyPlane = Plane.FromPoly(aPoly);
        var bPolyPlane = Plane.FromPoly(bPoly);

        maybeCutOp = Maybe.None;

        var newPositions = Pool<List<Vector3>>.Spawn();
        newPositions.Clear();
        var newPolygons = Pool<List<Polygon>>.Spawn();
        newPolygons.Clear();
        var cutPoints = Pool<List<PolyCutPoint>>.Spawn();
        cutPoints.Clear();
        try {

          // Construct cut points. After removing trivially similar cutpoints, we expect
          // a maximum of two.
          foreach (var aEdge in aPoly.edges) {
            float intersectionTime = 0f;
            var onPolyBPlane = PolyMath.Intersect(Line.FromEdge(aEdge),
                                                  bPolyPlane, out intersectionTime);

            if (onPolyBPlane.hasValue) {
              if (intersectionTime >= -0.1f && intersectionTime <= 1.1f) {
                var bPlanePoint = onPolyBPlane.valueOrDefault;

                if (bPlanePoint.IsInside(bPoly)) {
                  cutPoints.Add(new PolyCutPoint(
                                  bPlanePoint,
                                  aPoly));
                }
              }
            }
          }
          foreach (var bEdge in bPoly.edges) {
            float intersectionTime = 0f;
            var onPolyAPlane = PolyMath.Intersect(Line.FromEdge(bEdge),
                                                  aPolyPlane, out intersectionTime);

            if (onPolyAPlane.hasValue) {
              if (intersectionTime >= -0.1f && intersectionTime <= 1.1f) {
                var aPlanePoint = onPolyAPlane.valueOrDefault;

                if (aPlanePoint.IsInside(aPoly)) {
                  cutPoints.Add(new PolyCutPoint(
                                  aPlanePoint,
                                  aPoly));
                }
              }
            }
          }

          // Make sure there aren't multiple cut points for a single position index.
          var existingCutPointCountDict = Pool<Dictionary<int, int>>.Spawn();
          existingCutPointCountDict.Clear();
          try {

            foreach (var cutPoint in cutPoints) {
              if (cutPoint.isExistingPoint) {
                int curCount;
                if (existingCutPointCountDict.TryGetValue(cutPoint.existingPoint,
                                                          out curCount)) {
                  existingCutPointCountDict[cutPoint.existingPoint] = curCount + 1;
                }
                else {
                  existingCutPointCountDict[cutPoint.existingPoint] = 1;
                }
              }
            }

            cutPoints.RemoveAll(c => {
              if (!c.isExistingPoint) return false;
              int existingPoint = c.existingPoint;
              if (existingCutPointCountDict[existingPoint] > 1) {
                existingCutPointCountDict[existingPoint] -= 1;
                return true;
              }
              return false;
            });
          }
          finally {
            existingCutPointCountDict.Clear();
            Pool<Dictionary<int, int>>.Recycle(existingCutPointCountDict);
          }
          
          // If cut points exist both on an edge and a vertex on that edge, pick one.
          var removeCutPointIndices = Pool<List<int>>.Spawn();
          removeCutPointIndices.Clear();
          try {
            foreach (var edgeCpPair in cutPoints.Query().Where(cp => cp.isNewEdgePoint)
                                                        .Select(cp => new Pair<Edge, PolyCutPoint>() {
                                                          a = cp.edge,
                                                          b = cp
                                                        })) {
              var edge         = edgeCpPair.a;
              var edgeCutPoint = edgeCpPair.b;
              foreach (var vertex in cutPoints.Query().Where(cp => cp.isExistingPoint)
                                                      .Select(cp => cp.existingPoint)) {
                if (edge.ContainsVertex(vertex)) {
                  var vertPos = cutPoints[0].polygon.GetMeshPosition(vertex);
                  var edgePos = edgeCutPoint.newPoint;
                  if (Vector3.Distance(vertPos, edgePos) < PolyMath.POSITION_TOLERANCE) {
                    // Within distance, remove edge.
                    var removeIdx = cutPoints.FindIndex(cp => cp == edgeCutPoint);
                    if (!removeCutPointIndices.Contains(removeIdx)) {
                      removeCutPointIndices.Add(removeIdx);
                    }
                  }
                  else {
                    // Beyond distance, remove vertex.
                    var removeIdx = cutPoints.FindIndex(cp => cp.existingPoint == vertex);
                    if (!removeCutPointIndices.Contains(removeIdx)) {
                      removeCutPointIndices.Add(removeIdx);
                    }
                  }
                }
              }
            }
            
            removeCutPointIndices.Sort();
            cutPoints.RemoveAtMany(removeCutPointIndices);
          }
          finally {
            removeCutPointIndices.Clear();
            Pool<List<int>>.Recycle(removeCutPointIndices);
          }


          if (cutPoints.Count > 2) {
            
            if (cutPoints.Count == 3) {

              // Edge-case: Three cut points, where one is an edge between a and b,
              // and the second and third are a and b -- an invalid cut.
              {
                Maybe<int> existingIdx0 = Maybe.None, existingIdx1 = Maybe.None;
                Maybe<Edge> cutPointEdge = Maybe.None;
                for (int i = 0; i < cutPoints.Count; i++) {
                  var cutPoint = cutPoints[i];
                  if (cutPoint.isExistingPoint) {
                    if (!existingIdx0.hasValue) {
                      existingIdx0 = cutPoint.existingPoint;
                    }
                    else {
                      existingIdx1 = cutPoint.existingPoint;
                    }
                  }
                  else if (cutPoint.isNewEdgePoint) {
                    cutPointEdge = cutPoint.edge;
                  }
                }

                if (existingIdx0.hasValue && existingIdx1.hasValue && cutPointEdge.hasValue) {
                  if (new Edge() {
                    mesh = cutPointEdge.valueOrDefault.mesh,
                    a = existingIdx0.valueOrDefault,
                    b = existingIdx1.valueOrDefault
                  } == cutPointEdge) {
                    // Single-edge cut. Remove the edge cut.
                    cutPoints.RemoveAll(p => p.isNewEdgePoint);
                  }
                }
              }

            }

          }

          if (cutPoints.Count > 2) {
            
            // Cluster cut points that are very close to one another; if there are two
            // clusters, we can pick the closest points to the connecting segment between
            // the segment and we're good; if there's more than two cluster, this cut
            // is weird and we're gonna panic again.
            {
              var clusters = Pool<List<List<int>>>.Spawn();
              clusters.Clear();
              var clusterCentroids = Pool<List<Vector3>>.Spawn();
              clusterCentroids.Clear();
              try {
                for (int i = 0; i < cutPoints.Count; i++) {
                  var cutPoint = cutPoints[i];
                  var cutPos = cutPoint.GetPosition();

                  int indexOfNearbyCluster = -1;
                  if (clusters.Count != 0) {

                    for (int c = 0; c < clusters.Count; c++) {
                      var cluster  = clusters[c];
                      var centroid = clusterCentroids[c];

                      if (Vector3.Distance(centroid, cutPos)
                          < PolyMath.CLUSTER_TOLERANCE) {
                        indexOfNearbyCluster = c;
                        break;
                      }
                    }
                  }

                  if (indexOfNearbyCluster == -1) {
                    // Add the cut point to a new cluster.
                    var newCluster = Pool<List<int>>.Spawn();
                    newCluster.Add(i);
                    clusters.Add(newCluster);
                    clusterCentroids.Add(cutPoints[i].GetPosition());
                  }
                  else {
                    var origNumClusterPoints = clusters[indexOfNearbyCluster].Count;
                    clusters[indexOfNearbyCluster].Add(i);

                    // Accumulate position into cluster centroid.
                    clusterCentroids[indexOfNearbyCluster]
                      = (cutPos + clusterCentroids[indexOfNearbyCluster]
                                  * origNumClusterPoints)
                         / (origNumClusterPoints + 1);

                  }
                }

                if (clusters.Count > 2) {
                  Debug.LogWarning("Found more than 2 clusters. Will only use the first "
                                 + "2, but this will likely produce unexpected behavior.");
                }
                if (clusters.Count > 1) {
                  // Pick the point from each cluster that is closest to the other cluster's
                  // centroid.
                  int keep0 = -1, keep1 = -1;
                  float keep0SqrDist = float.PositiveInfinity,
                      keep1SqrDist = float.PositiveInfinity;
                  foreach (var cpIdx in clusters[0]) {
                    var pos = cutPoints[cpIdx].GetPosition();
                    var testSqrDist = (clusterCentroids[1] - pos).sqrMagnitude;
                    if (keep0 == -1 || keep0SqrDist > testSqrDist) {
                      keep0 = cpIdx;
                      keep0SqrDist = testSqrDist;
                    }
                  }
                  foreach (var cpIdx in clusters[1]) {
                    var pos = cutPoints[cpIdx].GetPosition();
                    var testSqrDist = (clusterCentroids[0] - pos).sqrMagnitude;
                    if (keep1 == -1 || keep1SqrDist > testSqrDist) {
                      keep1 = cpIdx;
                      keep1SqrDist = testSqrDist;
                    }
                  }

                  var cp0 = cutPoints[keep0];
                  var cp1 = cutPoints[keep1];
                  cutPoints.Clear();
                  cutPoints.Add(cp0);
                  cutPoints.Add(cp1);
                }
                //else {
                //  throw new System.InvalidOperationException(
                //    "Tried to cluster cut points when more than 2 were found, but there "
                //    + "were more than 2 clusters (" + clusters.Count + " found.)");

                //}
              }
              finally {
                foreach (var cluster in clusters) {
                  cluster.Clear();
                  Pool<List<int>>.Recycle(cluster);
                }
                clusters.Clear();
                Pool<List<List<int>>>.Recycle(clusters);
                clusterCentroids.Clear();
                Pool<List<Vector3>>.Recycle(clusterCentroids);
              }
            }
          }

          if (cutPoints.Count > 2) {
            // If we have more than 2 cut points still, and as many cut points as there
            // are points on the polygon, just ignore 'em! They must be tiny...
            // (I'm not proud of this.)
            var polyIndicesHash = Pool<HashSet<int>>.Spawn();
            polyIndicesHash.Clear();
            try {
              foreach (var cp in cutPoints) {
                if (cp.isExistingPoint) {
                  polyIndicesHash.Add(cp.existingPoint);
                }
              }

              if (polyIndicesHash.Count == cutPoints[0].polygon.Count) {
                cutPoints.Clear();
              }
            }
            finally {
              polyIndicesHash.Clear();
              Pool<HashSet<int>>.Recycle(polyIndicesHash);
            }
          }

          if (cutPoints.Count > 2) {
            throw new System.InvalidOperationException(
              "Logic error somewhere during cut. Cut points length is " + cutPoints.Count);
          }
          
          // There must be two cut points to define a successful cut, but some two-point
          // cut configurations still won't produce an actual cut.
          if (cutPoints.Count == 2) {

            var c0 = cutPoints[0];
            var c1 = cutPoints[1];

            bool isValidCut;
            bool isNewCut = true; // There's only one case where we'll override new cut
                                  // to false (pre-existing adjacted vertices).

            // If either cut is a new _face_ point, the cut is guaranteed to be valid.
            if (c0.isNewFacePoint || c1.isNewFacePoint) {
              isValidCut = true;
            }
            else {
              bool bothExistingPoints = c0.isExistingPoint && c1.isExistingPoint;
              if (bothExistingPoints) {
                // Both points already exist, so the cut is only valid if there is no
                // edge that already exists between the two cut points.
                //var edge = new Edge() {
                //  mesh = c0.polygon.mesh,
                //  a = c0.existingPoint,
                //  b = c1.existingPoint
                //};
                //isValidCut = !c0.polygon.mesh.edgeAdjFaces.ContainsKey(edge);

                //// TODO DELETEME
                //// Hmm. Something's weird, double-check that the two vertices aren't
                //// adjacent on this polygon.
                //var poly = c0.polygon;
                //var indexOfC1Point = poly.verts.IndexOf(c1.existingPoint);
                //var indexOfC0Point = poly.verts.IndexOf(c0.existingPoint);
                //if (Mathf.Abs(indexOfC1Point - indexOfC0Point) <= 1
                //    && isValidCut) {
                //  throw new System.InvalidOperationException(
                //    "Two adjacent vertices do not represent a valid cut. But this " +
                //    "should be detected via the edge-check!");
                //}

                // CHANGE:
                // We need to return ALL of the edges that form a cut, even if it's not
                // a new cut. So it's OK to return a cut operation that only exists along
                // a pre-existing edge.
                isNewCut = false;
                isValidCut = true;
              }
              else {
                bool oneExistingPoint = c0.isExistingPoint || c1.isExistingPoint;

                if (oneExistingPoint) {
                  if (!c0.isExistingPoint) {
                    Utils.Swap(ref c0, ref c1);
                  }

                  if (!(c0.isExistingPoint && c1.isNewEdgePoint)) {
                    throw new System.InvalidOperationException(
                      "Logic error: Somehow the c0 isn't existing point or c1 isn't edge.");
                  }

                  // c0 is an existing point and c1 is an edge cut, so the cut is only
                  // valid if c0's existing point isn't on that edge.
                  isValidCut = c0.existingPoint != c1.edge.a
                            && c0.existingPoint != c1.edge.b;
                }
                else {
                  if (!(c0.isNewEdgePoint && c1.isNewEdgePoint)) {
                    throw new System.InvalidOperationException(
                      "Logic error: Somehow the cut points aren't both edge points.");
                  }

                  // Both cut points must be edge points. In this case a valid cut
                  // can occur as long as the edges are not the same.
                  isValidCut = c0.edge != c1.edge;
                }

              }
            }

            if (isValidCut) {
              maybeCutOp = new PolyCutOp() {
                c0 = cutPoints[0],
                c1 = cutPoints[1],
                representsNewCut = isNewCut
              };
              return true;
            }
            else {
              return false;
            }

          }

        }
        finally {
          newPositions.Clear();
          Pool<List<Vector3>>.Recycle(newPositions);
          newPolygons.Clear();
          Pool<List<Polygon>>.Recycle(newPolygons);
          cutPoints.Clear();
          Pool<List<PolyCutPoint>>.Recycle(cutPoints);
        }

        return false;
      }

      public struct Pair<T, U> {
        public T a;
        public U b;
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
      public static void ApplyCut(PolyCutOp cutOp, out Edge cutEdge) {
        var c0 = cutOp.c0;
        var c1 = cutOp.c1;

        // TODO: DELETEME
        var cutPoints = new List<PolyCutPoint>() { c0, c1 };
        RuntimeGizmos.RuntimeGizmoDrawer drawer;
        if (RuntimeGizmos.RuntimeGizmoManager.TryGetGizmoDrawer(out drawer)) {
          foreach (var cutPoint in cutPoints) {
            Vector3 p;
            if (cutPoint.isExistingPoint) {
              p = cutPoint.polygon.GetMeshPosition(cutPoint.existingPoint);
              drawer.color = Color.red;
            }
            else if (cutPoint.isNewEdgePoint) {
              drawer.color = Color.green;
              p = cutPoint.newPoint;
            }
            else {
              drawer.color = Color.blue;
              p = cutPoint.newPoint;
            }

            drawer.DrawWireSphere(p, 0.005f);
          }
        }

        Maybe<int> c0VertIdx = Maybe.None;
        Maybe<int> c1VertIdx = Maybe.None;

        if (c0.type == PolyCutPointType.Invalid || c1.type == PolyCutPointType.Invalid) {
          throw new System.InvalidOperationException(
            "Cannot cut using invalid PolyCutPoints.");
        }

        if (c0.polygon != c1.polygon) {
          throw new System.InvalidOperationException(
            "Cannot apply cut points from two different polygons.");
        }

        var polygon = c0.polygon;

        if (c0.isExistingPoint && c1.isExistingPoint) {

          // If an edge already exists between these two points, we're already done;
          // no modifications needed, simply return the cut edge.
          if (c0.polygon.mesh.CheckValidEdge(new Edge() {
            mesh = c0.polygon.mesh,
            a = c0.existingPoint,
            b = c1.existingPoint
          })) {

          }
          else {
            ApplyVertexVertexCut(polygon, c0.existingPoint, c1.existingPoint);
          }

          c0VertIdx = c0.existingPoint;
          c1VertIdx = c1.existingPoint;
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
            int addedVertId;

            switch (c1.type) {
              case PolyCutPointType.NewPointEdge:
                // Vertex-Edge cut.
                ApplyVertexEdgeCut(polygon, c0.existingPoint,
                                   c1.edge, c1.newPoint,
                                   out addedVertId);
                break;
              case PolyCutPointType.NewPointFace:
                // Vertex-Face cut.
                ApplyVertexFaceCut(polygon, c0.existingPoint, c1.newPoint,
                                   out addedVertId);
                break;
              default:
                throw new System.InvalidOperationException(
                  "Invalid PolyCutPointType for second cut point.");
            }

            c0VertIdx = c0.existingPoint;
            c1VertIdx = addedVertId;
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
              int addedVertId0, addedVertId1;

              switch (c1.type) {
                case PolyCutPointType.NewPointEdge:
                  if (c0.edge == c1.edge) {
                    throw new System.InvalidOperationException(
                      "Error applying cut: Cannot edge-edge cut the same edge.");
                  }
                  ApplyEdgeEdgeCut(polygon,
                                   c0.edge, c0.newPoint,
                                   c1.edge, c1.newPoint,
                                   out addedVertId0, out addedVertId1);
                  break;
                case PolyCutPointType.NewPointFace:
                  ApplyEdgeFaceCut(polygon,
                                   c0.edge, c0.newPoint,
                                   c1.newPoint,
                                   out addedVertId0, out addedVertId1);
                  break;
                default:
                  throw new System.InvalidOperationException(
                    "Invalid PolyCutPointType for second cut point.");
              }

              c0VertIdx = addedVertId0;
              c1VertIdx = addedVertId1;
            }
            else {
              int addedVertId0, addedVertId1;

              // c0 and c1 must both be face points.
              if (c0.isNewFacePoint & c1.isNewFacePoint) {
                ApplyFaceFaceCut(polygon, c0.newPoint, c1.newPoint,
                                 out addedVertId0, out addedVertId1);
              }
              else {
                throw new System.InvalidOperationException(
                  "Logic error resolving cut points! Couldn't find correct cut type "
                + "resolution.");
              }

              c0VertIdx = addedVertId0;
              c1VertIdx = addedVertId1;
            }
          }
        }

        if (!c0VertIdx.hasValue || !c1VertIdx.hasValue) {
          throw new System.InvalidOperationException(
            "Error applying cut; one of the cut points was not successfully set while "
          + "applying the cut operation.");
        }

        // If we made it this far without throwing an exception, the cut
        // was applied, and there's now a valid edge between the two existing or newly
        // defined cut points.
        cutEdge = new Edge() {
          mesh = c0.polygon.mesh,
          a = c0VertIdx.valueOrDefault,
          b = c1VertIdx.valueOrDefault
        };

      }

      #region Cut Types

      public static void ApplyVertexVertexCut(Polygon polygon, int vert0, int vert1) {
        LowOps.SplitPolygon(polygon, vert0, vert1);
      }

      public static void ApplyVertexEdgeCut(Polygon polygon, int vert,
                                            Edge edge, Vector3 pointOnEdge,
                                            out int addedVertId) {
        Polygon equivalentPolygon;
        Edge    addedEdge0, addedEdge1; // unused.
        LowOps.SplitEdgeAddVertex(edge, pointOnEdge,
                                 out addedVertId,
                                 out addedEdge0, out addedEdge1,
                                 polygon,
                                 out equivalentPolygon);
        LowOps.SplitPolygon(equivalentPolygon, vert, addedVertId);
      }

      public static void ApplyEdgeEdgeCut(Polygon polygon,
                                          Edge edge0, Vector3 pointOnEdge0,
                                          Edge edge1, Vector3 pointOnEdge1,
                                          out int addedVertId0,
                                          out int addedVertId1) {
        
        Polygon equivalentPolygon;
        Edge    addedEdge00, addedEdge01;
        LowOps.SplitEdgeAddVertex(edge0, pointOnEdge0,
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
        
        Edge addedEdge10, addedEdge11;
        LowOps.SplitEdgeAddVertex(edge1, pointOnEdge1,
                                 out addedVertId1,
                                 out addedEdge10,
                                 out addedEdge11,
                                 equivalentPolygon,
                                 out equivalentPolygon);

        LowOps.SplitPolygon(equivalentPolygon, addedVertId0, addedVertId1);

      }

      public static void ApplyVertexFaceCut(Polygon polygon, int vert,
                                            Vector3 facePosition,
                                            out int addedVertId) {
        
        LowOps.PokePolygon(polygon, facePosition, out addedVertId, null, null, vert);

      }

      public static void ApplyEdgeFaceCut(Polygon polygon,
                                          Edge edge, Vector3 pointOnEdge,
                                          Vector3 facePosition,
                                          out int addedVertId0,
                                          out int addedVertId1) {
        Polygon equivalentPolygon;
        Edge    addedEdge0, addedEdge1; // unused.
        LowOps.SplitEdgeAddVertex(edge, pointOnEdge,
                                 out addedVertId0,
                                 out addedEdge0, out addedEdge1,
                                 polygon,
                                 out equivalentPolygon);

        // TODO: DELETEME
        RuntimeGizmos.RuntimeGizmoDrawer drawer;
        if (RuntimeGizmos.RuntimeGizmoManager.TryGetGizmoDrawer(out drawer)) {
          drawer.color = LeapColor.cyan;
          foreach (var vert in equivalentPolygon.verts) {
            drawer.DrawWireSphere(equivalentPolygon.GetMeshPosition(vert), 0.003f);
          }
        }

        LowOps.PokePolygon(equivalentPolygon, facePosition,
                          out addedVertId1,
                          ensureEdgeToVertex: addedVertId0);

      }

      public static void ApplyFaceFaceCut(Polygon polygon,
                                          Vector3 facePosition0,
                                          Vector3 facePosition1,
                                          out int addedVertId0,
                                          out int addedVertId1) {

        // TODO: There should be a specific primitive operation for a double-poke;
        // this would be able to produce slightly better tesselation.

        var addedPolys = Pool<List<Polygon>>.Spawn();
        addedPolys.Clear();
        var addedEdges = Pool<List<Edge>>.Spawn();
        addedPolys.Clear();
        try {
          LowOps.PokePolygon(polygon, facePosition0,
                            out addedVertId0,
                            addedPolys,
                            addedEdges);

          var edgeWithNewPoint = addedEdges.Query().Where(edge => facePosition1.IsInside(edge))
                                                   .FirstOrDefault();
          if (edgeWithNewPoint != default(Edge)) {
            
            // DELETEME
            RuntimeGizmos.RuntimeGizmoDrawer drawer;
            if (RuntimeGizmos.RuntimeGizmoManager.TryGetGizmoDrawer(out drawer)) {
              drawer.color = LeapColor.purple;
              foreach (var poly in addedPolys) {
                foreach (var vert in poly.verts) {
                  drawer.DrawWireCube(poly.GetMeshPosition(vert), Vector3.one * 0.003f);
                  drawer.DrawWireCube(poly.GetMeshPosition(vert), Vector3.one * 0.004f);
                  drawer.DrawWireCube(poly.GetMeshPosition(vert), Vector3.one * 0.005f);
                }
              }

              drawer.color = LeapColor.olive;
              drawer.DrawWireCube(edgeWithNewPoint.GetPositionAlongEdge(0.25f, EdgeDistanceMode.Normalized), Vector3.one * 0.003f);
              drawer.DrawWireCube(edgeWithNewPoint.GetPositionAlongEdge(0.50f, EdgeDistanceMode.Normalized), Vector3.one * 0.004f);
              drawer.DrawWireCube(edgeWithNewPoint.GetPositionAlongEdge(0.75f, EdgeDistanceMode.Normalized), Vector3.one * 0.005f);
            }

            // Second point is on one of the newly-created edges.
            LowOps.SplitEdgeAddVertex(edgeWithNewPoint, facePosition1, out addedVertId1);
          }
          else {
            // Second point is inside a face, not on an edge.

            // DELETEME
            RuntimeGizmos.RuntimeGizmoDrawer drawer;
            if (RuntimeGizmos.RuntimeGizmoManager.TryGetGizmoDrawer(out drawer)) {
              drawer.color = LeapColor.purple;
              foreach (var poly in addedPolys) {
                foreach (var vert in poly.verts) {
                  drawer.DrawWireSphere(poly.GetMeshPosition(vert), 0.003f);
                  drawer.DrawWireSphere(poly.GetMeshPosition(vert), 0.004f);
                  drawer.DrawWireSphere(poly.GetMeshPosition(vert), 0.005f);
                }
              }
            }

            var secondPoly = addedPolys.Query().Where(p => facePosition1.IsInside(p))
                                               .FirstOrDefault();

            if (secondPoly != default(Polygon)) {
              LowOps.PokePolygon(secondPoly, facePosition1, out addedVertId1);
            }
          }

        }
        finally {
          addedPolys.Clear();
          Pool<List<Polygon>>.Recycle(addedPolys);
        }

      }

      #endregion

    }

    /// <summary>
    /// Low-level PolyMesh operations.
    /// </summary>
    public static class LowOps {

      #region SplitEdgeAddVertex

      /// <summary>
      /// Splits an edge, adding a new position to the edge's polygon to do so. This
      /// version of the function assumes that you've already calculated the target
      /// vertex position -- this MUST be on the edge! (It will be projected onto the
      /// edge in case it's slightly off the edge.)
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

        newEdgePosition = newEdgePosition.ClampedTo(edge);

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

            if (!polygon.IsPlanar()) {
              throw new System.InvalidOperationException(
                "SplitEdgeAddVertex operation resulted in a non-planar polygon.");
            }

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
      public static void SplitEdgeAddVertex(Edge edge, Vector3 newEdgePosition,
                                            out int addedVertId) {
        Edge    addedEdge0, addedEdge1;
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

          if (addedPoly0.verts.Count < 3) {
            throw new System.InvalidOperationException(
              "SplitPolygon produced a polygon with fewer than 3 vertices.");
          }

          if (!addedPoly0.IsPlanar()) {
            throw new System.InvalidOperationException(
              "SplitPolygon operation resulted in a non-planar polygon 0.");
          }

          addedPoly1 = new Polygon() {
            mesh = mesh,
            verts = vertsBufferB.Query().ToList()
          };
          
          if (addedPoly1.verts.Count < 3) {
            throw new System.InvalidOperationException(
              "SplitPolygon produced a polygon with fewer than 3 vertices.");
          }

          if (!addedPoly1.IsPlanar()) {
            throw new System.InvalidOperationException(
              "SplitPolygon operation resulted in a non-planar polygon 1.");
          }
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
      public static void PokePolygon(Polygon pokedPolygon, Vector3 position,
                                     out int addedVertId,
                                     List<Polygon> outAddedPolygonsList = null,
                                     List<Edge> outAddedEdgesList = null,
                                     int? ensureEdgeToVertex = null) {

        var mesh = pokedPolygon.mesh;
        
        if (mesh == null) {
          throw new System.InvalidOperationException("Mesh is null?");
        }
        mesh.AddPosition(position, out addedVertId);

        var addedPolygons = Pool<List<Polygon>>.Spawn();
        addedPolygons.Clear();
        var addedEdgesSet = Pool<HashSet<Edge>>.Spawn();
        addedEdgesSet.Clear();
        try {
          int fromIdx = 0;
          int startingOffset = ensureEdgeToVertex.HasValue ?
                                 pokedPolygon.verts.IndexOf(ensureEdgeToVertex.Value)
                               : 0;
          while (fromIdx < pokedPolygon.verts.Count) {
            var fragmentPoly = new Polygon() {
              mesh = mesh,
              verts = new List<int>() { addedVertId }
            };

            // (Polygons have cyclic indexers.)
            for (int i = fromIdx + startingOffset;
                     i <= startingOffset + pokedPolygon.verts.Count;
                     i++) {
              var vertIdx = pokedPolygon[i];

              if (fragmentPoly.Count < 2) {
                fragmentPoly.verts.Add(vertIdx);
              }
              else if (fragmentPoly.Count == 2) {
                fragmentPoly.verts.Add(vertIdx);
                fromIdx++;
              }
              else {
                fragmentPoly.verts.Add(vertIdx);
                if (!fragmentPoly.IsConvex()) {
                  fragmentPoly.verts.RemoveAt(fragmentPoly.verts.Count - 1);
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

            if (!fragmentPoly.IsPlanar()) {
              throw new System.InvalidOperationException(
                "PokePolygon fragment was non-planar.");
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

          mesh.RemovePolygon(pokedPolygon);
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
          var normal = poly.GetNormal();
          foreach (var tri in poly.tris) {
            foreach (var idx in tri) {
              faces.Add(verts.Count);
              verts.Add(GetLocalPosition(idx));
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