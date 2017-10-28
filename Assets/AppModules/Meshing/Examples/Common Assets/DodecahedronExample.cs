using UnityEngine;

namespace Leap.Unity.Meshing.Examples {

  public class DodecahedronExample : MonoBehaviour {

    public MeshFilter _meshFilter;

    [HideInInspector]
    public DodecahedronCutManager cutManager;

    public PolyMesh polyMesh;

    private void Start() {
      _meshFilter.mesh = new Mesh();
      _meshFilter.mesh.name = "Dodecahedron";

      if (cutManager == null) {
        InitMesh();
        UpdateMesh();
      }
    }

    public void InitMesh() {
      if (polyMesh == null) {
        polyMesh = new PolyMesh(this.transform);
      }
      Dodecahedron.FillPolyMesh(polyMesh);
    }

    public void UpdateMesh() {
      polyMesh.FillUnityMesh(_meshFilter.mesh);
    }

  }

}