using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {
  
  public interface IHandledObject {

    void MoveByHandle(IHandle attachedHandle,
                      Pose toPose,
                      Vector3 aroundPivot,
                      out Pose newHandleTargetPose);

  }

}
