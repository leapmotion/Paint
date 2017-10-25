using Leap.Unity.Query;
using UnityEngine;

namespace Leap.Unity.Meshing {

  #region Math Structs

  public struct Line {
    public Vector3 a, b;
    public Vector3 this[int idx] {
      get { if (idx == 0) return a; return b; }
    }

    public static Line FromEdge(PolyMesh A, Edge e) {
      return new Line() { a = A.GetPosition(e.a), b = A.GetPosition(e.b) };
    }
  }

  public struct Plane {
    public Vector3 point, normal;

    public static Plane FromPoly(PolyMesh mesh, Polygon poly) {
      return new Plane() {
        point  = mesh.GetPosition(poly[0]),
        normal = poly.GetNormal(mesh)
      };
    }
  }

  #endregion

  public static class PolyMath {

    #region Intersection

    /// <summary>
    /// Returns the point of intersection between the given line and plane, or None if
    /// the line is parallel to the plane.
    /// </summary>
    public static Maybe<Vector3> Intersect(Line line, Plane plane) {
      var t = 0f;
      return Intersect(line, plane, out t);
    }

    /// <summary>
    /// Returns the point of intersection between the given line and plane, or None if
    /// the line is parallel to the plane.
    /// 
    /// Optionally, this function provides an out parameter that indicates how far
    /// along the input line the intersection point lies; 0 indicates A, 1 indicates B,
    /// values less than zero indicate the point is behind A, and values greater than
    /// one indicate the point is ahead of B.
    /// </summary>
    public static Maybe<Vector3> Intersect(Line line, Plane plane,
                                           out float tOfIntersection) {
      var lineDir = line.b - line.a;
      var planeNormalDotLineDir = Vector3.Dot(plane.normal, lineDir);

      if (planeNormalDotLineDir == 0f) {
        tOfIntersection = 0f;
        return Maybe.None;
      }

      tOfIntersection = (Vector3.Dot(plane.normal, (plane.point - line.a))
                         / planeNormalDotLineDir);

      return line.a + tOfIntersection * lineDir;
    }

    #endregion

    #region Containment

    /// <summary>
    /// Returns if the point is on the line segment defined by argument Edge of the
    /// argument PolyMesh.
    public static bool IsInside(this Vector3 point, PolyMesh A, Edge edge) {
      var a = A.GetPosition(edge.a);
      var b = A.GetPosition(edge.b);
      return Vector3.Cross((point - a), (b - a)) == Vector3.zero;
    }

    /// <summary>
    /// Returns whether the point, which is assumed to be in the plane of the polygon, is
    /// inside that polygon. (Polygon winding order does not matter.)
    /// 
    /// Points right on an edge or a vertex of a polygon are rejected.
    /// </summary>
    public static bool IsInside(this Vector3 point, PolyMesh A, Polygon aPoly) {
      
      Maybe<Vector3> lastCrossProduct = Maybe.None;

      foreach (var edge in aPoly.edges) {
        var curCrossProduct = Vector3.Cross(A.GetPosition(edge.b) - A.GetPosition(edge.a),
                                            point - A.GetPosition(edge.a));

        if (lastCrossProduct.hasValue) {
          if (Vector3.Dot(lastCrossProduct.valueOrDefault, curCrossProduct) <= 0f) {
            // Flipping cross product direction means the point is outside the polygon.
            // Alternatively, if the dot product of these two cross products ever touches
            // zero, this implies the point is _right on the edge_ (or a vertex) of the
            // polygon, which we here interpret as a rejection case.
            return false;
          }
        }
        lastCrossProduct = curCrossProduct;
      }

      return true;
    }

    #endregion

    #region Closest Point

    public static Vector3 ClampedTo(this Vector3 pos, PolyMesh mesh, Polygon poly) {
      var closestSqrDist = float.PositiveInfinity;
      var clamped = pos;
      foreach (var edge in poly.edges) {
        var testClamped = pos.ClampedTo(mesh, edge);
        if ((pos - testClamped).sqrMagnitude < closestSqrDist) {
          clamped = testClamped;
        }
      }
      return clamped;
    }

    public static Vector3 ClampedTo(this Vector3 pos, PolyMesh mesh, Edge edge) {
      var a = mesh.GetPosition(edge.a);
      var b = mesh.GetPosition(edge.b);
      var mag = (b - a).magnitude;
      var lineDir = (b - a) / mag;
      return a + lineDir * Mathf.Clamp(Vector3.Dot((pos - a), lineDir), 0f, mag);
    }

    #endregion

  }

}