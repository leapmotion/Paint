using UnityEngine;
using System.Collections;

namespace Leap.Unity.Meshing.Examples {

  public class DodecahedronExample : MonoBehaviour {

    public MeshFilter _meshFilter;

    private void Start() {
      _meshFilter.mesh = new Mesh();
      _meshFilter.mesh.name = "Dodecahedron";

      var dodecahedronMesh = Dodecahedron.Create();
      dodecahedronMesh.FillUnityMesh(_meshFilter.mesh);
    }

  }

}