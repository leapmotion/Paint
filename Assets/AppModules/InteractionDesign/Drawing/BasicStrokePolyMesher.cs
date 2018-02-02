using Leap.Unity.Meshing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Drawing {

  public class BasicStrokePolyMesher : MonoBehaviour,
                                       IPolyMesher<StrokeObject> {

    public void FillPolyMeshData(StrokeObject strokeObj,
                                 List<Vector3> outStrokePositions,
                                 List<Polygon> outStrokePolygons,
                                 List<Edge> outStrokeSmoothEdges) {
      if (strokeObj.Count == 1) {
        return;
      }
      else {
        StrokePoint? maybeLastStrokePoint = null;
        int polyOffset = 0;
        foreach (var strokePoint in strokeObj.Query()) {
          var a = strokePoint;
          var p0 = a.pose.position + a.pose.rotation * Vector3.right * a.radius;
          var p1 = a.pose.position - a.pose.rotation * Vector3.right * a.radius;
          outStrokePositions.Add(p0, p1, p0, p1);

          if (maybeLastStrokePoint.HasValue) {
            var newPoly = Polygon.SpawnQuad(0, 1, -3, -4,
                                            indexOffset: polyOffset);
            outStrokePolygons.Add(newPoly);
            outStrokeSmoothEdges.Add(new Edge(polyOffset + 0, polyOffset + 1));

            // Double-sided:
            var oppositePoly = Polygon.SpawnQuad(2, 3, -1, -2,
                                                 indexOffset: polyOffset);
            outStrokePolygons.Add(oppositePoly);
            outStrokeSmoothEdges.Add(new Edge(polyOffset + 2, polyOffset + 3));
          }

          polyOffset += 4;
          maybeLastStrokePoint = strokePoint;

        }
      }
    }

    // TODO: DELETEME
    #region deleteme Original StrokePolyMeshManager getPolygons
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

          var n = aP.rotation * Vector3.up;               // normal
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
              = aP.position + prevBinormal.valueOrDefault * zP.radius * bMult;
            outStrokePositions[outStrokePositions.Count - 1]
              = aP.position - prevBinormal.valueOrDefault * zP.radius * bMult;
          }

          // Line up the corners of each quad in the stroke.
          var prevB = b;
          if (prevBinormal.hasValue) {
            prevB = prevBinormal.valueOrDefault;
          }
          prevBinormal = b;

          // Add positions.
          if (i == 0) {
            outStrokePositions.Add(aP.position - prevB * aP.radius * bMult);
            outStrokePositions.Add(aP.position + prevB * aP.radius * bMult);
          }
          outStrokePositions.Add(bP.position + b * aP.radius);
          outStrokePositions.Add(bP.position - b * aP.radius);

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
    #endregion

  }

}
