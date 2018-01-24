using Leap.Unity.Meshing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Drawing {
  
  public class StrokePolyMeshManager : MonoBehaviour {

    public int maxPolygonsPerPolyMesh = 64;

    public Material outputMaterial;

    public int addedTriangleCount = 0;
    public int addedMeshCount = 0;

    private KeyedPolyMeshObject _curPolyMeshObj;

    private List<KeyedPolyMeshObject> _livePolyMeshObjs = new List<KeyedPolyMeshObject>();
    private List<KeyedPolyMeshObject> _pendingPolyMeshObjs = new List<KeyedPolyMeshObject>();

    // The manager places one or more strokes into each LivePolyMeshObject.

    private Dictionary<StrokeObject, KeyedPolyMeshObject> _strokeMeshes
      = new Dictionary<StrokeObject, KeyedPolyMeshObject>();
    private Dictionary<KeyedPolyMeshObject, List<StrokeObject>> _meshStrokes
      = new Dictionary<KeyedPolyMeshObject, List<StrokeObject>>();

    void LateUpdate() {
      var strokesToAdd = Pool<List<StrokeObject>>.Spawn();
      strokesToAdd.Clear();
      try {
        foreach (var child in this.transform.GetChildren()) {
          var strokeObj = child.GetComponent<StrokeObject>();
          if (strokeObj != null) {
            strokesToAdd.Add(strokeObj);
          }
        }

        if (_curPolyMeshObj == null) {
          _curPolyMeshObj = createNewPolyMeshObj();
        }

        foreach (var stroke in strokesToAdd) {

          stroke.transform.parent = _curPolyMeshObj.transform;

          var strokeMeshPolygons  = Pool<List<Polygon>>.Spawn();
          strokeMeshPolygons.Clear();
          var strokeMeshPositions = Pool<List<Vector3>>.Spawn();
          strokeMeshPositions.Clear();
          var strokeSmoothEdges = Pool<List<Edge>>.Spawn();
          strokeSmoothEdges.Clear();
          try {
            getStrokePolygons(stroke, strokeMeshPositions,
                                      strokeMeshPolygons,
                                      strokeSmoothEdges);

            // If the current one is full, create a new LivePolyMeshObject for this
            // stroke's polygon data.
            if (_curPolyMeshObj.PolygonCount
                + strokeMeshPolygons.Count > maxPolygonsPerPolyMesh) {
              _livePolyMeshObjs.Add(_curPolyMeshObj);
              _pendingPolyMeshObjs.Add(_curPolyMeshObj);

              _curPolyMeshObj = createNewPolyMeshObj();
            }

            addStrokePolygonData(_curPolyMeshObj, stroke,
                                 strokeMeshPositions,
                                 strokeMeshPolygons,
                                 strokeSmoothEdges);
            
            _strokeMeshes[stroke] = _curPolyMeshObj;

            List<StrokeObject> strokes;
            if (_meshStrokes.TryGetValue(_curPolyMeshObj, out strokes)) {
              strokes.Add(stroke);
            }
            else {
              // TODO not sure what to do here
            }

            stroke.OnStrokeModified += onStrokeModified;
          }
          finally {
            strokeMeshPolygons.Clear();
            Pool<List<Polygon>>.Recycle(strokeMeshPolygons);
            strokeMeshPositions.Clear();
            Pool<List<Vector3>>.Recycle(strokeMeshPositions);
            strokeSmoothEdges.Clear();
            Pool<List<Edge>>.Recycle(strokeSmoothEdges);
          }
        }
      }
      finally {
        strokesToAdd.Clear();
        Pool<List<StrokeObject>>.Recycle(strokesToAdd);
      }
    }

    // TODO: Actually refactor this into some static generator method, and figure out how
    // to get it some actual customizability.
    private void getStrokePolygons(StrokeObject stroke,
                                   List<Vector3> outStrokePositions,
                                   List<Polygon> outStrokePolygons,
                                   List<Edge> outStrokeSmoothEdges) {
      if (stroke.Count == 1) {
        // nothing for now.
      }
      else {
        Maybe<Vector3> prevBinormal = Maybe.None;
        for (int i = 0; i + 1 < stroke.Count; i++) {
          var aP = stroke[i + 0];
          var bP = stroke[i + 1];

          var n = aP.normal;                              // normal
          var t = (bP.position - aP.position).normalized; // tangent (not normalized)
          var b = Vector3.Cross(t, n).normalized;         // binormal
          n = Vector3.Cross(b, t).normalized;

          // Modulate binormal thickness based on stroke curvature.
          // This is a look-backward modification.
          var bMult = 1f;
          if (i > 0) {
            var zP = stroke[i - 1];

            var zaDir = (aP.position - zP.position).normalized;
            var abDir = (bP.position - aP.position).normalized;

            bMult = Vector3.Dot(abDir, zaDir).Map(0.5f, 1f, 0f, 1f);

            outStrokePositions[outStrokePositions.Count - 2]
              = aP.position + prevBinormal.valueOrDefault * zP.size * bMult;
            outStrokePositions[outStrokePositions.Count - 1]
              = aP.position - prevBinormal.valueOrDefault * zP.size * bMult;
          }

          // Line up the corners of each quad in the stroke.
          var prevB = b;
          if (prevBinormal.hasValue) {
            prevB = prevBinormal.valueOrDefault;
          }
          prevBinormal = b;

          // Add positions.
          if (i == 0) {
            outStrokePositions.Add(aP.position - prevB * aP.size * bMult);
            outStrokePositions.Add(aP.position + prevB * aP.size * bMult);
          }
          outStrokePositions.Add(bP.position + b * aP.size);
          outStrokePositions.Add(bP.position - b * aP.size);

          // Add polygon.
          var polyVerts = Pool<List<int>>.Spawn();
          if (i == 0) {
            polyVerts.Add((i * 4) + 0);
            polyVerts.Add((i * 4) + 1);
            polyVerts.Add((i * 4) + 2);
            polyVerts.Add((i * 4) + 3);
          }
          else {
            polyVerts.Add(4 + (i - 1) * 2 - 1);
            polyVerts.Add(4 + (i - 1) * 2 - 2);
            polyVerts.Add(4 + (i - 1) * 2 + 0);
            polyVerts.Add(4 + (i - 1) * 2 + 1);
          }
          outStrokePolygons.Add(new Polygon() {
            mesh = null,      // meshless Polygon
            verts = polyVerts
          });

          // Mark the joining edges of the stroke as smooth.
          if (i == 0) {
            outStrokeSmoothEdges.Add(new Edge((i * 4) + 2, (i * 4) + 3));
          }
          else {
            outStrokeSmoothEdges.Add(new Edge((4 + (i - 1) * 2 + 0), (4 + (i - 1) * 2 + 1)));
          }
        }
      }
    }

    private KeyedPolyMeshObject createNewPolyMeshObj() {
      var gameObj = new GameObject("Live PolyMesh Object");
      gameObj.transform.parent = this.transform;

      var polyMeshObj = gameObj.AddComponent<KeyedPolyMeshObject>();
      polyMeshObj.meshRenderer.material = outputMaterial;
      polyMeshObj.generateDoubleSidedTris = true;

      addedMeshCount += 1;
      return polyMeshObj;
    }
    
    /// <summary>
    /// Add data for the stroke object to the LivePolyMeshObject. Later, when the stroke
    /// is modified, we'll use its modified callback to update the stroke data keyed by
    /// this stroke - see onStrokeModified.
    /// </summary>
    private void addStrokePolygonData(KeyedPolyMeshObject polyMeshObj,
                                      StrokeObject strokeObj,
                                      List<Vector3> strokeMeshPositions,
                                      List<Polygon> strokeMeshPolygons,
                                      List<Edge>    strokeSmoothEdges) {
      polyMeshObj.AddDataFor(strokeObj, strokeMeshPositions,
                                        strokeMeshPolygons,
                                        strokeSmoothEdges);

      // TODO: This counting is hacky and not-robust :(
      foreach (var polygon in strokeMeshPolygons) {
        addedTriangleCount += (polygon.Count - 2) * 2; // double-sided means twice as
                                                       // many triangles.
      }
    }

    /// <summary>
    /// Called when a stroke is modified, this method updates the polygons in the
    /// stroke's associated LivePolyMeshObject with the polygons that define the newly
    /// modified stroke.
    /// </summary>
    private void onStrokeModified(StrokeObject strokeObj) {
      using (new ProfilerSample("StrokePolyMeshManager: onStrokeModified")) {
        KeyedPolyMeshObject polyMeshObj;
        if (!_strokeMeshes.TryGetValue(strokeObj, out polyMeshObj)) {
          throw new System.InvalidOperationException(
            "No LivePolyMeshObject found for stroke: " + strokeObj);
        }

        PolyMesh polyMesh;
        var curStrokePosIndices = Pool<List<int>>.Spawn();
        curStrokePosIndices.Clear();
        var curStrokePolyIndices = Pool<List<int>>.Spawn();
        curStrokePolyIndices.Clear();
        KeyedPolyMeshObject.NotifyPolyMeshModifiedAction modificationsFinishedCallback;
        try {
          // Begin our modification of the data in the PolyMesh for this Stroke.
          polyMeshObj.ModifyDataFor(strokeObj,
                                    out polyMesh,
                                    curStrokePosIndices,
                                    curStrokePolyIndices,
                                    out modificationsFinishedCallback);

          var newStrokeMeshPolygons  = Pool<List<Polygon>>.Spawn();
          newStrokeMeshPolygons.Clear();
          var newStrokeMeshPositions = Pool<List<Vector3>>.Spawn();
          newStrokeMeshPositions.Clear();
          var newStrokeSmoothEdges   = Pool<List<Edge>>.Spawn();
          newStrokeSmoothEdges.Clear();
          var addedStrokeMeshPositionIndices = Pool<List<int>>.Spawn();
          addedStrokeMeshPositionIndices.Clear();
          var addedStrokeMeshPolygonIndices = Pool<List<int>>.Spawn();
          addedStrokeMeshPolygonIndices.Clear();
          var finalPositionIndices = Pool<List<int>>.Spawn();
          finalPositionIndices.Clear();
          try {

            using (new ProfilerSample("getStrokePolygons")) {
              // Get polygon data for the entirety of the stroke.
              // TODO: _Removing_ smooth edges is not supported.
              getStrokePolygons(strokeObj, newStrokeMeshPositions,
                                           newStrokeMeshPolygons,
                                           newStrokeSmoothEdges);
            }

            using (new ProfilerSample("Add/modify PolyMesh stroke positions")) {

              // Re-use existing position indices or add new ones to fit the modified
              // stroke.
              for (int i = 0; i < newStrokeMeshPositions.Count; i++) {
                int existingPositionIdx = -1;
                if (i < curStrokePosIndices.Count) {
                  existingPositionIdx = curStrokePosIndices[i];
                }
                if (existingPositionIdx != -1) {
                  // Modify the position at the existing position index.
                  polyMesh.SetPosition(existingPositionIdx, newStrokeMeshPositions[i]);

                  // Remember what this position index wound up being; the polygons of
                  // the stroke need to re-index into the final position index list.
                  finalPositionIndices.Add(existingPositionIdx);
                }
                else {
                  // Add the position and remember the added position index.
                  int addedPositionIdx;
                  polyMesh.AddPosition(newStrokeMeshPositions[i], out addedPositionIdx);
                  addedStrokeMeshPositionIndices.Add(addedPositionIdx);

                  // Remember what this position index wound up being.
                  finalPositionIndices.Add(addedPositionIdx);
                }
              }
            }

            using (new ProfilerSample("Add/modify PolyMesh stroke polygons")) {
              // Re-use existing polygon indices or add new ones to fit the modified
              // stroke.
              for (int i = 0; i < newStrokeMeshPolygons.Count; i++) {

                // First, re-index the polygon to refer to the final position indices of
                // the mesh. We constructed this list during the Add Positions step above.
                var newStrokeMeshPolygon = newStrokeMeshPolygons[i];
                for (int v = 0; v < newStrokeMeshPolygon.verts.Count; v++) {
                  newStrokeMeshPolygon.verts[v]
                    = finalPositionIndices[newStrokeMeshPolygon.verts[v]];
                }

                int existingPolygonIdx = -1;
                if (i < curStrokePolyIndices.Count) {
                  existingPolygonIdx = curStrokePolyIndices[i];
                }
                if (existingPolygonIdx != -1) {
                  // Modify the polygon at the existing position index.
                  Polygon replacedPolygon;
                  polyMesh.SetPolygon(existingPolygonIdx, newStrokeMeshPolygon,
                                      out replacedPolygon);
                  // (Add the vertex index list of the replaced polygon back to its pool.)
                  replacedPolygon.RecycleVerts();
                }
                else {
                  // Add the polygon and remember the added polygon index.
                  int addedPolygonIdx;
                  polyMesh.AddPolygon(newStrokeMeshPolygon, out addedPolygonIdx);

                  addedTriangleCount += (newStrokeMeshPolygon.Count - 2) * 2;
                  // (double-sided tris, also TODO: this is SUPER HACKY D: )

                  addedStrokeMeshPolygonIndices.Add(addedPolygonIdx);
                }
              }
            }

            using (new ProfilerSample("Mark new PolyMesh edges smooth")) {
              foreach (var newStrokeSmoothEdge in newStrokeSmoothEdges) {
                // Re-index the edge to refer to final positions in the PolyMesh.
                var finalSmoothEdge = new Edge(finalPositionIndices[newStrokeSmoothEdge.a],
                                               finalPositionIndices[newStrokeSmoothEdge.b]);
                polyMesh.MarkEdgeSmooth(finalSmoothEdge);
              }
            }

            using (new ProfilerSample("Call modifications-finished callback")) {
              // Notify that we've finished modifying the keyed object (the stroke object),
              // and include any new positions or polygons we may have added to do so.
              modificationsFinishedCallback(strokeObj,
                addedStrokeMeshPositionIndices,
                addedStrokeMeshPolygonIndices);
            }
          }
          finally {
            newStrokeMeshPolygons.Clear();
            Pool<List<Polygon>>.Recycle(newStrokeMeshPolygons);
            newStrokeMeshPolygons.Clear();
            Pool<List<Vector3>>.Recycle(newStrokeMeshPositions);
            newStrokeSmoothEdges.Clear();
            Pool<List<Edge>>.Recycle(newStrokeSmoothEdges);
            addedStrokeMeshPositionIndices.Clear();
            Pool<List<int>>.Recycle(addedStrokeMeshPositionIndices);
            addedStrokeMeshPolygonIndices.Clear();
            Pool<List<int>>.Recycle(addedStrokeMeshPolygonIndices);
            finalPositionIndices.Clear();
            Pool<List<int>>.Recycle(finalPositionIndices);
          }
        }
        finally {
          curStrokePolyIndices.Clear();
          Pool<List<int>>.Recycle(curStrokePolyIndices);
          curStrokePosIndices.Clear();
          Pool<List<int>>.Recycle(curStrokePosIndices);
        }
      }
    }

  }

}
