using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RibbonStroke {

  #region PUBLIC FIELDS

  /// <summary> The radius (half-thickness) of the ribbon mesh to be drawn with this stroke. </summary>
  public float _radius = 1F;

  /// <summary> The color of the ribbon. </summary>
  public Color _color = Color.black;

  /// <summary> The number of points per circular cross-section of the ribbon. </summary>
  public int _resolution = 8;

  /// <summary> The smoothing delay value for this stroke (See Leap.Util.SmoothedVector3). </summary>
  public float _smoothingDelay = 1F;

  /// <summary> The rotational delay value for this stroke (See Leap.Util.SmoothedQuaternion). </summary>
  public float _rotationSmoothingDelay = 1F;

  /// <summary> The points that define the stroke. </summary>
  public List<Vector3> _strokePoints = new List<Vector3>();

  /// <summary> The rotations of each point that define the stroke. </summary>
  public List<Quaternion> _strokePointRotations = new List<Quaternion>();

  /// <summary> The time that passed between a given stroke point and the point that was created before it. </summary>
  public List<float> _strokePointDeltaTimes = new List<float>();

  #endregion

  #region PUBLIC METHODS

  /// <summary> Adds the argument stroke input point and time delta to this TubeStroke. </summary>
  public void RecordStrokePoint(Vector3 strokePoint, Quaternion strokePointRotation, float deltaTime) {
    _strokePoints.Add(strokePoint);
    _strokePointRotations.Add(strokePointRotation);
    _strokePointDeltaTimes.Add(deltaTime);
  }

  /// <summary> Writes this TubeStroke object to a JSON string. </summary>
  public string WriteToJSON() {
    return JsonUtility.ToJson(this);
  }

  /// <summary> Loads a TubeStroke object from a valid JSON string. </summary>
  public static TubeStroke CreateFromJSON(string jsonString) {
    return JsonUtility.FromJson<TubeStroke>(jsonString);
  }

  #endregion

}
