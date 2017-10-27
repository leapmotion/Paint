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

      polygonToCut.polyMesh.useTransform   = polygonToCut.transform;
      cuttingPolygon.polyMesh.useTransform = cuttingPolygon.transform;
    }

    private List<Vector3> tempList = new List<Vector3>();
    private void Update() {
      Polygon.FillPolyMesh(polygonToCut.numVerts,   polygonToCut.polyMesh);
      Polygon.FillPolyMesh(cuttingPolygon.numVerts, cuttingPolygon.polyMesh);

      // Cut!
      //cuttingPolygon.Cut(polygonToCut);
      PolyMesh.CutOps.TryCut(polygonToCut.polyMesh,
                             cuttingPolygon.polyMesh,
                             cuttingPolygon.polyMesh.polygons[0]);

      // Update Unity mesh.
      polygonToCut.UpdateUnityMesh();
      cuttingPolygon.UpdateUnityMesh();
    }

  }

}