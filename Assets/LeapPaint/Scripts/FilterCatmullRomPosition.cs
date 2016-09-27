using UnityEngine;
using System.Collections.Generic;
using System;

public class FilterCatmullRomPosition {

  private List<Vector3> _interpolatedPoints = new List<Vector3>();

  /// <summary>
  /// Catmull-Rom splines interpolate smoothly between vectors p1 and p2 (by t, 0 to 1) given a sequence
  /// of vectors p0, p1, p2, and p3.
  /// </summary>
  private Vector3 InterpCatmullRom(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
    Vector3 a = 0.5F * (2F * p1);
    Vector3 b = 0.5F * (p2 - p0);
    Vector3 c = 0.5F * (2F * p0 - 5F * p1 + 4F * p2 - p3);
    Vector3 d = 0.5F * (-p0 + 3F * p1 - 3F * p2 + p3);

    Vector3 pos = a + (b * t) + (c * t * t) + (d * t * t * t);

    return pos;
  }

}