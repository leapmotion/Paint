namespace Leap.Unity.Gestures {

  public interface IPoseGesture {

    Pose currentPose { get; }

    bool wasActivated { get; }
    bool isActive { get; }
    bool wasDeactivated { get; }

  }

}