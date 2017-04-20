using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct IntersectPoints {
  public Vector3 first;
  public Vector3 second;

  public IntersectPoints(Vector3 first, Vector3 second) {
    this.first = first;
    this.second = second;
  }
}

public struct Rect3D {
  public Vector3 localBottomLeft { get; private set; }
  public Vector3 bottomLeft { get { return localBottomLeft + center; } }
  public Vector3 localBottomRight { get; private set; }
  public Vector3 bottomRight { get { return localBottomRight + center; } }
  public Vector3 localTopLeft { get; private set; }
  public Vector3 topLeft { get { return localTopLeft + center; } }
  public Vector3 localTopRight { get; private set; }
  public Vector3 topRight { get { return localTopRight + center; } }

  public Vector3 center { get; private set; }

  public float width { get; private set; }
  public float height { get; private set; }

  public Rect3D(Vector3 center, Vector3 right, Vector3 up, float width, float height) {
    Vector3 halfUp = up * (height * .5f);
    Vector3 halfSide = right * (width * .5f);

    this.localBottomLeft = -halfUp - halfSide;
    this.localBottomRight = -halfUp + halfSide;
    this.localTopLeft = halfUp - halfSide;
    this.localTopRight = halfUp + halfSide;

    this.center = center;

    this.width = width;
    this.height = height;
  }

  public Vector3 this[int index] {
    get {
      switch (index) {
        case 0:
          return this.localBottomLeft;
        case 1:
          return this.localBottomRight;
        case 2:
          return this.localTopLeft;
        case 3:
          return this.localTopRight;
        case 4:
          return this.bottomLeft;
        case 5:
          return this.bottomRight;
        case 6:
          return this.topLeft;
        case 7:
          return this.topRight;
        default:
          throw new System.IndexOutOfRangeException("Invalid Rect3D index");
      }
    }
  }

  public Vector3 Extents() {
    return new Vector3(width, height, 0f);
  }
}

public static class Geometry {

  //Returns 2 points since on line 1 there will be a closest point to line 2, and on line 2 there will be a closest point to line 1.
  public static IntersectPoints ClosestPointsOnTwoLines(Vector3 point1, Vector3 point1Direction, Vector3 point2, Vector3 point2Direction) {
    IntersectPoints intersections = new IntersectPoints();

    //I dont think we need to normalize
    //point1Direction.Normalize();
    //point2Direction.Normalize();

    float a = Vector3.Dot(point1Direction, point1Direction);
    float b = Vector3.Dot(point1Direction, point2Direction);
    float e = Vector3.Dot(point2Direction, point2Direction);

    float d = a * e - b * b;

    //This is a check if parallel, howeverm since we are not normalizing the directions, it seems even if they are parallel they will not == 0
    //so they will get past this point, but its seems to be alright since it seems to still give a correct point (although a point very fary away).
    //Also, if they are parallel and we dont normalize, the deciding point seems randomly choses on the lines, which while is still correct,
    //our ClosestPointsOnTwoLineSegments gets undesireable results when on corners. (for example when using it in our ClosestPointOnTriangleToLine).
    if (d != 0f) {
      Vector3 r = point1 - point2;
      float c = Vector3.Dot(point1Direction, r);
      float f = Vector3.Dot(point2Direction, r);

      float s = (b * f - c * e) / d;
      float t = (a * f - c * b) / d;

      intersections.first = point1 + point1Direction * s;
      intersections.second = point2 + point2Direction * t;
    }
    else {
      //Lines are parallel, select any points next to eachother
      intersections.first = point1;
      intersections.second = point2 + Vector3.Project(point1 - point2, point2Direction);
    }

    return intersections;
  }

  public static IntersectPoints ClosestPointsOnTwoLineSegments(Vector3 segment1Point1, Vector3 segment1Point2, Vector3 segment2Point1, Vector3 segment2Point2) {
    Vector3 line1Direction = segment1Point2 - segment1Point1;
    Vector3 line2Direction = segment2Point2 - segment2Point1;

    IntersectPoints closests = ClosestPointsOnTwoLines(segment1Point1, line1Direction, segment2Point1, line2Direction);
    IntersectPoints clampedClosests = closests;
    clampedClosests.first = ClampToSegment(clampedClosests.first, segment1Point1, segment1Point2);
    clampedClosests.second = ClampToSegment(clampedClosests.second, segment2Point1, segment2Point2);

    //Since this is a line segment, we need to decide which line we want to clamp both closest points to. So we choose the one that is farthest from its supposed closest point.
    if ((closests.first - clampedClosests.first).sqrMagnitude > (closests.second - clampedClosests.second).sqrMagnitude) {
      clampedClosests.second = SegmentTargetAlignToPoint(clampedClosests.first, segment2Point1, segment2Point2);
    }
    else {
      clampedClosests.first = SegmentTargetAlignToPoint(clampedClosests.second, segment1Point1, segment1Point2);
    }

    return clampedClosests;
  }

  //Assumes the point is already on the line somewhere
  public static Vector3 ClampToSegment(Vector3 point, Vector3 linePoint1, Vector3 linePoint2) {
    Vector3 lineDirection = linePoint2 - linePoint1;

    if (!ExtVector3.IsInDirection(point - linePoint1, lineDirection)) {
      point = linePoint1;
    }
    else if (ExtVector3.IsInDirection(point - linePoint2, lineDirection)) {
      point = linePoint2;
    }

    return point;
  }

  public static Vector3 SegmentTargetAlignToPoint(Vector3 point, Vector3 segmentPoint1, Vector3 segmentPoint2) {
    //We align the second point with the first.
    Vector3 aligned = segmentPoint1 + Vector3.Project(point - segmentPoint1, segmentPoint2 - segmentPoint1);
    aligned = ClampToSegment(aligned, segmentPoint1, segmentPoint2);

    return aligned;
  }

  public static IntersectPoints ClosestPointOnRectangleToLine(Vector3 segment0, Vector3 segment1, Rect3D rectangle, bool treatAsLineSegment = false) {
    return ClosestPointOnTriangleToLine(segment0, segment1, rectangle.bottomLeft, rectangle.topLeft, rectangle.topRight, rectangle.bottomRight, treatAsLineSegment, true);
  }

  //When isRectangle is true, the vertices must be on the same plane.
  static IntersectPoints ClosestPointOnTriangleToLine(Vector3 segment0, Vector3 segment1, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Vector3 vertex4, bool treatAsLineSegment = false, bool isRectangle = false) {
    Vector3 ab = vertex2 - vertex1;
    Vector3 ac = vertex3 - vertex1;
    Vector3 normal = Vector3.Cross(ab, ac);

    float s0PlaneDistance = Vector3.Dot(segment0 - vertex1, normal);
    float s1PlaneDistance = Vector3.Dot(segment1 - vertex1, normal);

    IntersectPoints closestPoints = new IntersectPoints();

    //If we want to treat it as a line segment then we will need to check its distance, but if the line is plane are parallel (s0PlaneDistance != s1PlaneDistance) then we will treat as line segment anyways since we need a point reference
    if (s0PlaneDistance != s1PlaneDistance && (!treatAsLineSegment || (s0PlaneDistance * s1PlaneDistance) < 0f)) {
      closestPoints.first = segment0 + (segment1 - segment0) * (-s0PlaneDistance / (s1PlaneDistance - s0PlaneDistance));
      closestPoints.second = closestPoints.first;
    }
    else {
      //We get the closest segment and calculate its closest distance to the plane
      closestPoints.first = (Mathf.Abs(s0PlaneDistance) < Mathf.Abs(s1PlaneDistance)) ? segment0 : segment1;
      closestPoints.second = closestPoints.first + (normal * LinePlaneDistance(closestPoints.first, normal, vertex1, normal));
    }

    //Make sure plane intersection is within triangle bounds
    float a = Vector3.Dot(Vector3.Cross(normal, vertex2 - vertex1), closestPoints.second - vertex1);
    float b = Vector3.Dot(Vector3.Cross(normal, vertex3 - vertex2), closestPoints.second - vertex2);
    float c = float.MaxValue;
    float d = float.MaxValue;
    if (!isRectangle) {
      c = Vector3.Dot(Vector3.Cross(normal, vertex1 - vertex3), closestPoints.second - vertex3);
    }
    else {
      c = Vector3.Dot(Vector3.Cross(normal, vertex4 - vertex3), closestPoints.second - vertex3);
      d = Vector3.Dot(Vector3.Cross(normal, vertex1 - vertex4), closestPoints.second - vertex4);
    }

    if (a < 0f || b < 0f || c < 0f || d < 0f) {
      //We are not within the triangle, we are on an edge so find the closest
      if (a < b && a < c && a < d) {
        return ClosestPointsOnTwoLineSegments(segment0, segment1, vertex1, vertex2);
      }
      else if (b < a && b < c && b < d) {
        return ClosestPointsOnTwoLineSegments(segment0, segment1, vertex2, vertex3);
      }
      else if ((c < a && c < b && c < d) || !isRectangle) {
        if (!isRectangle) {
          return ClosestPointsOnTwoLineSegments(segment0, segment1, vertex3, vertex1);
        }
        else {
          return ClosestPointsOnTwoLineSegments(segment0, segment1, vertex3, vertex4);
        }
      }
      else if (isRectangle) {
        return ClosestPointsOnTwoLineSegments(segment0, segment1, vertex4, vertex1);
      }
    }

    return closestPoints;
  }

  public static float LinePlaneDistance(Vector3 linePoint, Vector3 lineVec, Vector3 planePoint, Vector3 planeNormal) {
    //calculate the distance between the linePoint and the line-plane intersection point
    float dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
    float dotDenominator = Vector3.Dot(lineVec, planeNormal);

    //line and plane are not parallel
    if (dotDenominator != 0f) {
      return dotNumerator / dotDenominator;
    }

    return 0;
  }

}

public static class ExtVector3 {
  public static readonly Vector3[] GeneralDirections = new Vector3[] { Vector3.right, Vector3.up, Vector3.forward, Vector3.left, Vector3.down, Vector3.back };

  public static float Maximum(this Vector3 vector) {
    return ExtMathf.Max(vector.x, vector.y, vector.z);
  }

  public static float Minimum(this Vector3 vector) {
    return ExtMathf.Min(vector.x, vector.y, vector.z);
  }

  public static bool IsParallel(Vector3 direction, Vector3 otherDirection, float precision = .000001f) {
    return Vector3.Cross(direction, otherDirection).sqrMagnitude < precision;
  }

  public static Vector3 ClosestDirectionTo(Vector3 direction1, Vector3 direction2, Vector3 targetDirection) {
    return (Vector3.Dot(direction1, targetDirection) > Vector3.Dot(direction2, targetDirection)) ? direction1 : direction2;
  }

  //from and to must be normalized
  public static float Angle(Vector3 from, Vector3 to) {
    return Mathf.Acos(Mathf.Clamp(Vector3.Dot(from, to), -1f, 1f)) * Mathf.Rad2Deg;
  }

  public static Vector3 Direction(Vector3 startPoint, Vector3 targetPoint) {
    return (targetPoint - startPoint).normalized;
  }

  public static bool IsInDirection(Vector3 direction, Vector3 otherDirection, float precision, bool normalizeParameters = true) {
    if (normalizeParameters) {
      direction.Normalize();
      otherDirection.Normalize();
    }
    return Vector3.Dot(direction, otherDirection) > 0f + precision;
  }
  public static bool IsInDirection(Vector3 direction, Vector3 otherDirection) {
    return Vector3.Dot(direction, otherDirection) > 0f;
  }

  public static float MagnitudeInDirection(Vector3 vector, Vector3 direction, bool normalizeParameters = true) {
    if (normalizeParameters) direction.Normalize();
    return Vector3.Dot(vector, direction);
  }

  public static Vector3 Abs(this Vector3 vector) {
    return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
  }

  public static Vector3 ClosestGeneralDirection(Vector3 vector) { return ClosestGeneralDirection(vector, GeneralDirections); }
  public static Vector3 ClosestGeneralDirection(Vector3 vector, IList<Vector3> directions) {
    float maxDot = float.MinValue;
    int closestDirectionIndex = 0;

    for (int i = 0; i < directions.Count; i++) {
      float dot = Vector3.Dot(vector, directions[i]);
      if (dot > maxDot) {
        closestDirectionIndex = i;
        maxDot = dot;
      }
    }

    return directions[closestDirectionIndex];
  }
}

public static class ExtMathf {
  public static float Min(float value1, float value2, float value3) {
    float min = (value1 < value2) ? value1 : value2;
    return (min < value3) ? min : value3;
  }

  public static float Max(float value1, float value2, float value3) {
    float max = (value1 > value2) ? value1 : value2;
    return (max > value3) ? max : value3;
  }

  public static bool Approximately(float value1, float value2) { return Approximately(value1, value2, Mathf.Epsilon); }
  public static bool Approximately(float value1, float value2, float precision) {
    return Mathf.Abs(value1 - value2) < precision;
  }

  public static float Squared(this float value) {
    return value * value;
  }
  public static float Squared(this int value) {
    return value * value;
  }
}