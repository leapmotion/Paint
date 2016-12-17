using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Paint2 {

  public class Stroke {

    private List<StrokePoint> strokePoints;
    public IEnumerator<StrokePoint> GetPointsEnumerator() {
      return strokePoints.GetEnumerator();
    }
    public int Count { get { return strokePoints.Count; } }
    public StrokePoint this[int index] {
      get { return strokePoints[index]; }
      set { strokePoints[index] = value; }
    }

    public float timeCreated;

    public Action<Stroke, StrokeModificationHint> OnStrokeModified = (x, y) => { };

    public Stroke() {
      strokePoints = new List<StrokePoint>();
      timeCreated = Time.time;
    }

    public void Add(StrokePoint strokePoint) {
      strokePoints.Add(strokePoint);
      OnStrokeModified(this, StrokeModificationHint.AddedPoint());
    }

    /// <summary>
    /// Returns the rotation at the given index by interpolating halfway between the strokePoints[index].rotation
    /// and the strokePoints[index + 1].rotation. (Also handles the edge cases at either end of the stroke.)
    /// </summary>
    public Quaternion OrientationAt(int index) {
      if (index == 0 || index == Count - 2 || index == Count - 1) return strokePoints[index].rotation;
      else return Quaternion.Slerp(strokePoints[index].rotation, strokePoints[index + 1].rotation, 0.5F);
    }

  }

  public enum StrokeModificationType {
    Overhaul       = 1, // Whole stroke must be rendered again.
    AddedPoint     = 2,
    AddedPoints    = 3,
    ModifiedPoints = 4
  }

  public class StrokeModificationHint {

    public StrokeModificationType modType;

    public List<int>    pointsModified;
    public int          numPointsAdded;

    private StrokeModificationHint(StrokeModificationType type) {
      this.modType = type;
    }

    public static StrokeModificationHint Overhaul() {
      var modHint = new StrokeModificationHint(StrokeModificationType.Overhaul);
      return modHint;
    }

    public static StrokeModificationHint AddedPoints(int numPointsAdded) {
      var modHint = new StrokeModificationHint(StrokeModificationType.AddedPoints);
      modHint.numPointsAdded = numPointsAdded;
      return modHint;
    }

    public static StrokeModificationHint AddedPoint() {
      var modHint = new StrokeModificationHint(StrokeModificationType.AddedPoint);
      return modHint;
    }

    public static StrokeModificationHint ModifiedPoints(List<int> pointsModified) {
      var modHint = new StrokeModificationHint(StrokeModificationType.ModifiedPoints);
      modHint.pointsModified = pointsModified;
      return modHint;
    }

  }

}