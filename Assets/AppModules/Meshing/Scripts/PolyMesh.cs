using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing {

  public class PolyMesh {

    public List<Vector3> positions;
    public List<Polygon> polygons;

    #region Convenience Functions

    /// <summary>
    /// Clears the PolyMesh.
    /// </summary>
    public void Clear() {
      positions.Clear();
      polygons.Clear();
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
    public Vector3 PositionAt(int vertIdx) {
      return P(vertIdx);
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
          foreach (var tri in poly.tris) {
            var normal = Vector3.Cross(P(tri[1]) - P(tri[0]),
                                       P(tri[2]) - P(tri[0])).normalized;
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

    #region Operations

    public static class Op {

      public static void Combine(PolyMesh A, PolyMesh B, PolyMesh intoPolyMesh) {
        intoPolyMesh.Clear();

        foreach (var pos in A.positions) {
          intoPolyMesh.positions.Add(pos);
        }
        foreach (var poly in A.polygons) {
          intoPolyMesh.polygons.Add(poly);
        }

        foreach (var pos in B.positions) {
          intoPolyMesh.positions.Add(pos);
        }
        foreach (var poly in B.polygons) {
          intoPolyMesh.polygons.Add(poly.IncrementIndices(A.positions.Count));
        }
      }

      public static PolyMesh Combine(PolyMesh A, PolyMesh B) {
        var result = new PolyMesh();

        Combine(A, B, result);

        return result;
      }
      
      public static void Subtract(PolyMesh A, PolyMesh B, PolyMesh intoPolyMesh) {
        EdgeLoop aIntersection, bIntersection;
        CutIntersectionLoops(A, B, out aIntersection, out bIntersection);
        
        var aTemp = Pool<PolyMesh>.Spawn();
        var bTemp = Pool<PolyMesh>.Spawn();
        try {
          aIntersection.insidePolys.Fill(aTemp);
          bIntersection.outsidePolys.Fill(bTemp);

          Combine(aTemp, bTemp, intoPolyMesh);
        }
        finally {
          Pool<PolyMesh>.Recycle(aTemp);
          Pool<PolyMesh>.Recycle(bTemp);
        }
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

    #endregion

  }

}