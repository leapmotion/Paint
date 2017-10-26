using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing.Examples {

  public class CutManager : MonoBehaviour {

    public PolygonObject polygonToCut;
    public PolygonObject cuttingPolygon;

    public Transform effectivePolygon2Transform;

    private void Start() {
      polygonToCut.polyMesh = new PolyMesh();
      cuttingPolygon.polyMesh = new PolyMesh();
    }

    private List<Vector3> tempList = new List<Vector3>();
    private void Update() {
      Polygon.FillPolyMesh(polygonToCut.numVerts,   polygonToCut.polyMesh);
      Polygon.FillPolyMesh(cuttingPolygon.numVerts, cuttingPolygon.polyMesh);

      // Temporarily, we need to manually transform the positions in the cutting triangle.
      tempList.Clear();
      foreach (var pos in cuttingPolygon.polyMesh.positions) {
        tempList.Add(effectivePolygon2Transform.TransformPoint(pos));
      }
      cuttingPolygon.polyMesh.FillPositionsOnly(tempList);

      // Cut!
      //cuttingPolygon.Cut(polygonToCut);

      // Update Unity mesh.
      polygonToCut.UpdateUnityMesh();
      cuttingPolygon.UpdateUnityMesh();
    }

  }

}