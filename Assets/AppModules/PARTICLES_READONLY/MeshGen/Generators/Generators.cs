using UnityEngine;

using Vecs = System.Collections.Generic.List<UnityEngine.Vector3>;
using Ints = System.Collections.Generic.List<int>;
using Cols = System.Collections.Generic.List<UnityEngine.Color>;

namespace Leap.Unity.MeshGen {

  public static partial class Generators {

    #region Generator Resources

    private static void borrowGeneratorResources(out Vecs verts,
                                                 out Ints indices,
                                                 out Vecs normals) {
      verts   = Pool<Vecs>.Spawn();
      indices = Pool<Ints>.Spawn();
      normals = Pool<Vecs>.Spawn();
    }

    private static void returnGeneratorResources(Vecs verts,
                                                 Ints indices,
                                                 Vecs normals) {
      verts.Clear();
      Pool<Vecs>.Recycle(verts);

      indices.Clear();
      Pool<Ints>.Recycle(indices);

      normals.Clear();
      Pool<Vecs>.Recycle(normals);
    }

    #endregion

    #region Apply Resources to Mesh

    private static void apply(Mesh mesh, Vecs verts,
                                         Ints indices,
                                         Vecs normals = null,
                                         Cols colors = null) {
      mesh.Clear();

      mesh.SetVertices(verts);
      mesh.SetTriangles(indices, 0, true);

      if (normals != null) {
        mesh.SetNormals(normals);
      }
      else {
        mesh.RecalculateNormals();
      }
      
      if (colors != null) {
        mesh.SetColors(colors);
      }
    }

    #endregion

    #region Generation Functions

    public static void GenerateTorus(Mesh mesh,
                                     float majorRadius, int numMajorSegments,
                                     float minorRadius, int numMinorSegments) {
      Vecs verts; Ints indices; Vecs normals;
      borrowGeneratorResources(out verts, out indices, out normals);

      TorusSupport.AddIndices(indices, verts.Count, numMajorSegments, numMinorSegments);
      TorusSupport.AddVerts(verts, normals, majorRadius, numMajorSegments, minorRadius, numMinorSegments);

      apply(mesh, verts, indices, normals);
      returnGeneratorResources(verts, indices, normals);
    }

    public static void GenerateRoundedRectPrism(Mesh mesh,
                                                Vector3 extents,
                                                float cornerRadius, int cornerDivisions,
                                                bool withBack = true) {
      Vecs verts; Ints indices; Vecs normals;
      borrowGeneratorResources(out verts, out indices, out normals);

      RoundedRectSupport.AddFrontIndices(indices, verts.Count, cornerDivisions);
      RoundedRectSupport.AddFrontVerts(verts, normals, extents, cornerRadius, cornerDivisions);
      //RoundedRectPrism.AddFrontUVs(); // NYI

      RoundedRectSupport.AddSideIndices(indices, verts.Count, cornerDivisions);
      RoundedRectSupport.AddSideVerts(verts, normals, extents, cornerRadius, cornerDivisions);
      //RoundedRectPrism.AddSideUVs(); // NYI

      if (withBack) {
        Vector3 extentsForBack = new Vector3(extents.x, extents.y, 0F);
        RoundedRectSupport.AddFrontIndices(indices, verts.Count, cornerDivisions, flipFacing: true);
        RoundedRectSupport.AddFrontVerts(verts, normals, extentsForBack, cornerRadius, cornerDivisions, flipNormal: true);
        //RoundedRectPrism.AddBackUVs(); // NYI
      }

      apply(mesh, verts, indices, normals);
      returnGeneratorResources(verts, indices, normals);
    }

    #endregion

  }

}