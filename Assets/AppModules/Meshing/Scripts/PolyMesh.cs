using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing {

  public class PolyMesh {

    public Vector3[] positions;
    public Polygon[] polygons;

    public PolyMesh(Vector3[] positions, Polygon[] polygons) {
      this.positions = positions;
      this.polygons  = polygons;
    }

    public void FillMesh(Mesh mesh) {
      mesh.Clear();

      var vertList = Pool<List<Vector3>>.Spawn();
      var triList  = Pool<List<int>>.Spawn();
      try {
        vertList.AddRange(positions);
        
        foreach (var poly in polygons) {
          foreach (var tri in poly.tris) {
            foreach (var idx in tri) {
              triList.Add(idx);
            }
          }
        }

        mesh.SetVertices(vertList);
        mesh.SetTriangles(triList, 0, true);
        mesh.RecalculateNormals();
      }
      finally {
        vertList.Clear();
        Pool<List<Vector3>>.Recycle(vertList);
      }
    }

  }

}