using UnityEngine;
using System.Collections.Generic;

namespace MeshGeneration {

  public class RibbonCircle : IShape {

    #region PRIVATE FIELDS

    private float _radius;
    private float _ribbonThickness;
    private int _radialSubdivisons;
    private float _tangentAngle;

    #endregion

    #region CONSTRUCTOR

    public RibbonCircle(float radius, float thickness, int radialSubdivisions, float tangentAngle = 0F) {
      _radius = radius;
      _ribbonThickness = thickness;
      _radialSubdivisons = radialSubdivisions;
      _tangentAngle = tangentAngle;
    }

    #endregion

    #region PROPERTIES

    public float Radius {
      get { return _radius; }
      set { _radius = value; }
    }

    public float RibbonThickness {
      get { return _ribbonThickness; }
      set { _ribbonThickness = value; }
    }

    public int RadialSubdivisions {
      get { return _radialSubdivisons; }
      set { _radialSubdivisons = value; }
    }

    public float TangentAngle {
      get { return _tangentAngle; }
      set { _tangentAngle = value; }
    }

    #endregion

    #region SHAPE IMPLEMENTATION

    public void CreateMeshData(MeshPoints points, List<int> connections) {
      if (_radius == 0F || _radialSubdivisons < 2) {
        return;
      }

      // MeshPoints
      for (int i = 0; i < RadialSubdivisions; i++) {
        Vector3 radiusPoint = Quaternion.AngleAxis(i * (360F / RadialSubdivisions), Vector3.up) * Vector3.right * Radius;
        Vector3 radiusDirection = radiusPoint.normalized;

        MeshPoint p0 = new MeshPoint(radiusPoint - ((Quaternion.AngleAxis(_tangentAngle, Vector3.Cross(radiusDirection, Vector3.up)) * radiusDirection) * RibbonThickness / 2F));
        p0.Uv = new Vector3(i / (float)RadialSubdivisions, 0F);
        p0.Color = Color.white;
        points.Add(p0);

        MeshPoint p1 = new MeshPoint(radiusPoint + ((Quaternion.AngleAxis(_tangentAngle, Vector3.Cross(radiusDirection, Vector3.up)) * radiusDirection) * RibbonThickness / 2F));
        p1.Uv = new Vector3(i / (float)RadialSubdivisions, 1F);
        p1.Color = Color.white;
        points.Add(p1);
      }

      // Connections
      for (int i = 0; i < RadialSubdivisions - 1; i++) {
        int vertexOffset = i * 2;

        connections.Add(vertexOffset + 0);
        connections.Add(vertexOffset + 1);
        connections.Add(vertexOffset + 2);

        connections.Add(vertexOffset + 1);
        connections.Add(vertexOffset + 3);
        connections.Add(vertexOffset + 2);
      }

      // Complete the circle
      connections.Add((RadialSubdivisions * 2) - 2);
      connections.Add((RadialSubdivisions * 2) - 1);
      connections.Add(0);

      connections.Add((RadialSubdivisions * 2) - 1);
      connections.Add(1);
      connections.Add(0);
    }

    public MeshTopology Topology {
      get { return MeshTopology.Triangles; }
    }

    #endregion

  }

}
