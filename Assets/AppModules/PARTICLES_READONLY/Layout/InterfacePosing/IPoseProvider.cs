using UnityEngine;

namespace Leap.Unity.Layout {

  public interface IPoseProvider {

    Vector3 GetTargetPosition();

    Quaternion GetTargetRotation();

  }

  public static class IPoseProviderExtensions {
    
    public static Pose GetTargetPose(this IPoseProvider poseProvider) {
      return new Pose(poseProvider.GetTargetPosition(),
                      poseProvider.GetTargetRotation());
    }

  }

}