using UnityEngine;

namespace Leap.Unity.Splines {

  /// <summary>
  /// Interpolate between points using (centripetal) Catmull-Rom splines.
  /// </summary>
  public static class CatmullRom {

    /// <summary>
    /// Performs centripetal Catmull-Rom interpolation between the second and third
    /// spline points and writes evenly-distributed points along the spline into interpolatedPoints.
    /// </summary>
    public static void InterpolatePoints(Vector3[] fourPositions, float[] timeValues, ref Vector3[] outPoints) {
      InterpolatePoints(fourPositions, timeValues, ref outPoints, numPoints: outPoints.Length);
    }

    /// <summary>
    /// Performs centripetal Catmull-Rom interpolation between the second and third
    /// spline points and writes evenly-distributed points along the spline into
    /// interpolatedPoints. Use numPoints to specify the number of points to write in
    /// the output buffer.
    /// 
    /// The first point will be the first input position; the last point will be the last
    /// input position.
    /// </summary>
    public static void InterpolatePoints(Vector3[] fourPositions, float[] timeValues,
                                         ref Vector3[] outPoints, int numPoints) {
      for (int i = 0; i < numPoints; i++) {
        outPoints[i] = Interpolate(fourPositions, timeValues, i * (1F / (numPoints - 1)));
      }
    }

    /// <summary> Evaluates the Catmull-Rom spline between p1 and p2 given points P and times T at time t. </summary>
    // http://stackoverflow.com/questions/9489736/catmull-rom-curve-with-no-cusps-and-no-self-intersections
    public static Vector3 Interpolate(Vector3[] P, float[] T, float t) {
      Vector3 L01 = P[0] * (T[1] - t) / (T[1] - T[0]) + P[1] * (t - T[0]) / (T[1] - T[0]);
      Vector3 L12 = P[1] * (T[2] - t) / (T[2] - T[1]) + P[2] * (t - T[1]) / (T[2] - T[1]);
      Vector3 L23 = P[2] * (T[3] - t) / (T[3] - T[2]) + P[3] * (t - T[2]) / (T[3] - T[2]);
      Vector3 L012 = L01 * (T[2] - t) / (T[2] - T[0]) + L12  * (t - T[0]) / (T[2] - T[0]);
      Vector3 L123 = L12 * (T[3] - t) / (T[3] - T[1]) + L23  * (t - T[1]) / (T[3] - T[1]);
      Vector3 C12 = L012 * (T[2] - t) / (T[2] - T[1]) + L123 * (t - T[1]) / (T[2] - T[1]);
      return C12;
    }

  }

}