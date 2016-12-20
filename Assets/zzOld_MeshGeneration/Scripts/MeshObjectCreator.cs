using UnityEngine;

namespace MeshGeneration {

  public static class MeshObjectCreator {

    public static GameObject CreateObject(Mesh mesh, Material mat, Transform root = null) {
      GameObject meshObj = new GameObject("MeshObj");
      meshObj.transform.parent = root;
      meshObj.transform.localPosition = Vector3.zero;
      meshObj.transform.localRotation = Quaternion.identity;
      meshObj.transform.localScale = Vector3.one;

      meshObj.AddComponent<MeshFilter>().mesh = mesh;
      meshObj.AddComponent<MeshRenderer>().material = mat;

      return meshObj;
    }

    public static void CreateObjectsRealtime(ShapeCombiner combiner, Material mat, Transform root = null) {
      combiner.OnNewMesh += mesh => {
        CreateObject(mesh, mat, root);
      };
    }

  }
}