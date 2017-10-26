using UnityEngine;

namespace Leap.Unity.Meshing.Examples {

  public class DodecahedronExample : MonoBehaviour {

    public MeshFilter _meshFilter;

    private void Start() {
      _meshFilter.mesh = new Mesh();
      _meshFilter.mesh.name = "Dodecahedron";

      var dodecahedronMesh = Dodecahedron.CreatePolyMesh();
      dodecahedronMesh.FillUnityMesh(_meshFilter.mesh);
    }

  }

}