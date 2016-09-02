using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace MeshGeneration {

  [ExecuteInEditMode]
  public abstract class ShapeBehaviour : MonoBehaviour, IShape {

    [SerializeField]
    private bool _autoUpdate;

    public void GenerateMesh() {
      var filter = GetComponent<MeshFilter>();
      if (filter != null) {
#if UNITY_EDITOR
        Mesh mesh = ShapeCombiner.BuildOneMesh(this, shouldOptimize: false, shouldUpload: false);

        var p = new UnityEditor.UnwrapParam();
        p.angleError = 0.05f;
        p.areaError = 0.05f;
        p.hardAngle = 90;
        p.packMargin = 0.0f;
        UnityEditor.Unwrapping.GenerateSecondaryUVSet(mesh);

        //mesh.uv = mesh.uv2;

        filter.sharedMesh = mesh;
#endif
      }
    }

    void Update() {
      if (!Application.isPlaying && _autoUpdate) {
        GenerateMesh();
      }
    }

    public abstract MeshTopology Topology { get; }
    public abstract void CreateMeshData(MeshPoints points, List<int> connections);
  }
}
