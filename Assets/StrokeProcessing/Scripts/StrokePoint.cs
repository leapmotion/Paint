using UnityEngine;

public struct StrokePoint {

  public Vector3 position;
  public Vector3 normal;
  public Quaternion rotation;
  public Quaternion handOrientation;
  public float deltaTime;

  public StrokePoint(Vector3 position, Vector3 normal, Quaternion rotation, Quaternion handOrientation, float deltaTime) {
    this.position = position;
    this.normal = normal;
    this.rotation = rotation;
    this.handOrientation = handOrientation;
    this.deltaTime = deltaTime;
  }

}