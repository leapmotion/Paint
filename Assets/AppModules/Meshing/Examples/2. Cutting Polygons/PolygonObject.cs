using UnityEngine;

namespace Leap.Unity.Meshing.Examples {

  public class PolygonObject : MonoBehaviour {

    public MeshFilter _meshFilter;

    [Range(3, 12)]
    public int numVerts = 5;

    public PolyMesh polyMesh;

    private void Reset() {
      if (_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
    }

    private void Start() {
      _meshFilter.mesh = new Mesh();
      _meshFilter.mesh.name = "Polygon";
    }

    public void UpdateUnityMesh() {
      polyMesh.FillUnityMesh(_meshFilter.mesh);
    }

  }

}