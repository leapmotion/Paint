using Leap.Unity.Query;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing {

  #region Math Structs

  public struct Line {
    public Vector3 a, b;
    public Vector3 this[int idx] {
      get { if (idx == 0) return a; return b; }
    }

    public static Line FromEdge(Edge edge) {
      return new Line() { a = edge.GetPositionA(),
                          b = edge.GetPositionB() };
    }
  }

  public struct Plane {
    public Vector3 point, normal;

    public static Plane FromPoly(Polygon poly) {
      return new Plane() {
        point  = poly.mesh.GetPosition(poly[0]),
        normal = poly.GetNormal()
      };
    }
  }

  #endregion

  public static class PolyMath {

    public const float POSITION_TOLERANCE = 5e-03f;
    public const float CLUSTER_TOLERANCE = 5e-02f;

    public const int MAX_LOOPS = 10000;

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

    public enum EdgeIntersectionType {
      None,
      FullyColinear,
      SemiColinear,
      FragmentColinear,
      Crossed
    }

    public struct EdgeIntersection {
      public EdgeIntersectionType type;
      public Edge edgeA, edgeB;

      public Maybe<Vector3> point0;
      public Maybe<Vector3> point1;
    }

    /// <summary>
    /// Returns the point of intersection if the two edges intersect, or None if the
    /// edges do not intersect one another. (Touching vertices counts as a point of
    /// intersection.)
    /// </summary>
    public static Maybe<Vector3> Intersect(Edge edge0, Edge edge1,
                                           out bool edgesWereColinear) {
      edgesWereColinear = false;

      var edge0AOnEdge1 = edge0.GetPositionA().ClampedTo(edge1);
      var edge0BOnEdge1 = edge0.GetPositionB().ClampedTo(edge1);

      if (edge0AOnEdge1 == edge0BOnEdge1
          && edge0AOnEdge1.IsInside(edge0)) {
        return edge0AOnEdge1;
      }
      else if (edge0AOnEdge1.IsInside(edge0)
               && edge0BOnEdge1.IsInside(edge0)) {
        edgesWereColinear = true;
      }

      return Maybe.None;
    }

    /// <summary>
    /// Produces the correct cut points for an with another edge this is colinear with it
    /// -- they intersect at infinitely many points. This will produce zero, one, or two
    /// cut points.
    /// </summary>
    public static void ResolveColinearity(Edge colinearEdgeToCut, Edge colinearCuttingEdge,
                                          List<Vector3> outCutsOnEdge) {
      var cuttingPointA = colinearCuttingEdge.GetPositionA();
      var normalizedCuttingAOnETC = colinearEdgeToCut
                                      .GetNormalizedAmountAlongEdge(cuttingPointA);

      var cuttingPointB = colinearCuttingEdge.GetPositionB();
      var normalizedCuttingBOnETC = colinearEdgeToCut
                                      .GetNormalizedAmountAlongEdge(cuttingPointB);

      if (normalizedCuttingAOnETC > 0f && normalizedCuttingAOnETC < 1f) {
        outCutsOnEdge.Add(cuttingPointA);
      }

      if (normalizedCuttingBOnETC > 0f && normalizedCuttingBOnETC < 1f) {
        outCutsOnEdge.Add(cuttingPointB);
      }
    }

    #endregion

    #region Containment

    /// <summary>
    /// Returns if the point is on the line segment defined by argument Edge of the
    /// argument PolyMesh.
    /// </summary>
    public static bool IsInside(this Vector3 point, Edge edge) {
      return Vector3.Distance(point.ClampedTo(edge), point) < POSITION_TOLERANCE;

      //var a = edge.mesh.GetPosition(edge.a);
      //var b = edge.mesh.GetPosition(edge.b);
      //var ap = (point - a);
      //var ab = (b - a);

      //// ap must be along the same line as ab. Here this is evaluated as a rough
      //// cross product component tolerance.
      //var apXab = Vector3.Cross(ap, ab);
      //if (apXab.x > POSITION_TOLERANCE
      // || apXab.y > POSITION_TOLERANCE
      // || apXab.z > POSITION_TOLERANCE) return false;

      //// ap must be in the same direction as ab (or have no direction).
      //var apDab = Vector3.Dot(ap, ab);
      //if (apDab < 0f) return false;

      //// ap's square magnitude should be equal to or less than ab's (0 length is fine).
      //return ap.sqrMagnitude <= ab.sqrMagnitude;
    }

    /// <summary>
    /// Returns whether the point, which is assumed to be in the plane of the polygon, is
    /// inside that polygon. (Polygon winding order does not matter.)
    /// 
    /// Points right on an edge or a vertex of a polygon are accepted.
    /// </summary>
    public static bool IsInside(this Vector3 point, Polygon aPoly) {
      
      Maybe<Vector3> lastCrossProduct = Maybe.None;

      Vector3 a, b;
      foreach (var edge in aPoly.edges) {
        a = edge.mesh.GetPosition(edge.a);
        b = edge.mesh.GetPosition(edge.b);
        var curCrossProduct = Vector3.Cross(b - a, point - a);

        if (lastCrossProduct.hasValue) {
          if (Vector3.Dot(lastCrossProduct.valueOrDefault, curCrossProduct) < 0f) {
            return false;
          }
        }
        if (curCrossProduct != Vector3.zero) {
          lastCrossProduct = curCrossProduct;
        }
      }

      return true;
    }

    #endregion

    #region Closest Point

    /// <summary>
    /// Clamps this position to this polygon, but assumes that the input is already on
    /// the plane of this polygon!
    /// </summary>
    public static Vector3 ClampedTo(this Vector3 pos, Polygon poly) {
      if (pos.IsInside(poly)) return pos;

      var closestSqrDist = float.PositiveInfinity;
      var clamped = pos;
      foreach (var edge in poly.edges) {
        var testClamped = pos.ClampedTo(edge);
        if ((pos - testClamped).sqrMagnitude < closestSqrDist) {
          clamped = testClamped;
        }
      }
      return clamped;
    }

    public static Vector3 ClampedTo(this Vector3 pos, Edge edge) {
      var a = edge.GetPositionA();
      var b = edge.GetPositionB();
      var ab = b - a;
      var mag = ab.magnitude;
      var lineDir = ab / mag;
      var progress = Mathf.Clamp(Vector3.Dot((pos - a), lineDir), 0f, mag);
      if (progress < POSITION_TOLERANCE) progress = 0f;
      else if (progress > (mag - POSITION_TOLERANCE)) progress = mag;
      return a + lineDir * progress;
    }

    #endregion

  }

}