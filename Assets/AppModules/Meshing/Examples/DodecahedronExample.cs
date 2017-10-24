using UnityEngine;
using System.Collections;

namespace Leap.Unity.Meshing.Examples {

  public class DodecahedronExample : MonoBehaviour {

    public MeshFilter _meshFilter;

    private void Start() {
      _meshFilter.mesh = new Mesh();
      _meshFilter.mesh.name = "Dodecahedron";

      var polymesh = new PolyMesh(Dodecahedron.Positions,
                                  Dodecahedron.Polygons);
      polymesh.FillMesh(_meshFilter.mesh);
    }

  }

}