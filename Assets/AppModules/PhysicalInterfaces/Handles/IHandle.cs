namespace Leap.Unity.PhysicalInterfaces {

  public interface IHandle {

    bool isHeld { get; }

    Pose targetPose { get; }

    ReadonlyList<IHandledObject> attachedObjects { get; }

  }

}