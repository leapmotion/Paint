using UnityEngine;
using System.Collections.Generic;

namespace Procedural.DynamicMesh {

  public static class QuadIsland {
    public static void AddQuad(RawMesh mesh, int vertOffset, int v0, int v1, int v2, int v3) {
      mesh.indexes.Add(vertOffset + v0);
      mesh.indexes.Add(vertOffset + v1);
      mesh.indexes.Add(vertOffset + v3);

      mesh.indexes.Add(vertOffset + v1);
      mesh.indexes.Add(vertOffset + v2);
      mesh.indexes.Add(vertOffset + v3);
    }

    public static void AddQuads(RawMesh mesh, int quadNumber) {
      var list = mesh.indexes;
      int v = mesh.verts.Count;
      for (int i = quadNumber; i-- != 0;) {
        list.Add(v);
        list.Add(v + 1);
        list.Add(v + 3);

        list.Add(v + 1);
        list.Add(v + 2);
        list.Add(v + 3);

        v += 4;
      }
    }

    public static void AddVerts(RawMesh mesh, Vector3 center, Vector3 primaryExtent, Vector3 secondaryExtent) {
      mesh.verts.Add(center + primaryExtent + secondaryExtent);
      mesh.verts.Add(center - primaryExtent + secondaryExtent);
      mesh.verts.Add(center - primaryExtent - secondaryExtent);
      mesh.verts.Add(center + primaryExtent - secondaryExtent);
    }

    private static Vector2[] quadUvs = new Vector2[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up, Vector2.zero, Vector2.right, Vector2.one };
    public static void AddUvs(RawMesh mesh, int offset) {
      mesh.uv0.Add(quadUvs[offset++]);
      mesh.uv0.Add(quadUvs[offset++]);
      mesh.uv0.Add(quadUvs[offset++]);
      mesh.uv0.Add(quadUvs[offset++]);
    }
  }
}
