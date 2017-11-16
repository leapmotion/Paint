namespace Leap.Unity.PhysicalInterfaces {

  public interface IHandle {

    Pose targetPose { get; }

    ReadonlyList<IHandledObject> attachedObjects { get; }

    bool isHeld { get; }

  }

}