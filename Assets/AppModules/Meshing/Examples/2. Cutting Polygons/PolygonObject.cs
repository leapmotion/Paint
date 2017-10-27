using Leap.Unity.Query;
using Leap.Unity.RuntimeGizmos;
using UnityEngine;

namespace Leap.Unity.Meshing.Examples {

  public class PolygonObject : MonoBehaviour, IRuntimeGizmoComponent {

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

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      var mesh = polyMesh;

      if (mesh == null) return;

      Color[] polyColors = new Color[] {
        LeapColor.amber,
        LeapColor.beige,
        LeapColor.cerulean,
        LeapColor.red,
        LeapColor.turquoise,
        LeapColor.yellow
      };

      float[] indexRMults = new float[] {
        0.70f,
        0.80f,
        0.90f,
        1.10f,
        1.20f,
        1.30f,
        1.40f,
        1.50f,
        1.60f,
      };

      // Verts.
      int polyIdx = 0;
      int vertIdx = 0;
      foreach (var poly in mesh.polygons) {
        drawer.color = polyColors[polyIdx % polyColors.Length];
        polyIdx++;
        polyIdx %= polyColors.Length;
        foreach (var vertPos in poly.verts.Query().Select(vIdx => poly.GetMeshPosition(vIdx))) {
          drawer.color = Color.Lerp(drawer.color, Color.black, ((float)vertIdx / poly.verts.Count) * 0.5f);
          //float rMult = indexRMults[vertIdx % indexRMults.Length];
          float rMult = 1f;
          drawer.DrawWireSphere(vertPos, 0.10f * rMult);
          vertIdx++;
          vertIdx %= indexRMults.Length;
        }
      }

    }
  }

}