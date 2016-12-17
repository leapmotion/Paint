using UnityEngine;

namespace StrokeProcessing {

  [System.Serializable]
  public struct StrokePoint {

    public Vector3 position;
    public Vector3 normal;
    public Quaternion rotation;
    public Quaternion handOrientation;
    public float deltaTime;
    public float thickness;
    public Color color;

    public StrokePoint(Vector3 position, Vector3 normal, Quaternion rotation, Quaternion handOrientation, float deltaTime, float thickness, Color color) {
      this.position = position;
      this.normal = normal;
      this.rotation = rotation;
      this.handOrientation = handOrientation;
      this.deltaTime = deltaTime;
      this.thickness = thickness;
      this.color = color;
    }

  }

}