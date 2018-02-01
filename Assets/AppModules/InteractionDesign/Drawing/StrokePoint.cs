using UnityEngine;

namespace Leap.Unity.Drawing {

  [System.Serializable]
  public struct StrokePoint {
    public Pose  pose;
    public Color color;
    public float radius;

    public Vector3 position { get { return pose.position; } }
    public Quaternion rotation { get { return pose.rotation; } }
  }

}
