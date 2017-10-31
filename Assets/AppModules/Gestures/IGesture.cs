namespace Leap.Unity.Gestures {

  public interface IGesture {

    bool wasActivated { get; }
    bool isActive { get; }
    bool wasDeactivated { get; }

  }

}
