using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public struct KinematicState {

    public Pose pose;
    public Movement movement;


    public void Integrate(float deltaTime) {
      pose.Integrate(movement, deltaTime);
    }

    public void Integrate(Vector3 linearAcceleration,
                          float deltaTime) {
      movement.Integrate(linearAcceleration, deltaTime);
      pose.Integrate(movement, deltaTime);
    }

    public void Integrate(Vector3 linearAcceleration,
                          Vector3 angularAcceleration,
                          float deltaTime) {
      movement.Integrate(linearAcceleration, angularAcceleration, deltaTime);
      pose.Integrate(movement, deltaTime);
    }

  }

  public static class PoseExtensions {

    public static void Integrate(this Pose thisPose, Movement movement, float deltaTime) {
      thisPose.position = movement.velocity * deltaTime + thisPose.position;

      if (movement.angularVelocity.sqrMagnitude > 0.00001f) {
        thisPose.rotation = Quaternion.AngleAxis(movement.angularVelocity.magnitude * deltaTime,
                                                 movement.angularVelocity.normalized)
                   * thisPose.rotation;
      }
    }

  }

}
