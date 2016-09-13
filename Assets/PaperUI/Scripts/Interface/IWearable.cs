using UnityEngine;
using Leap.Unity;

public interface IWearable {

  // Hand State
  void NotifyHandTracked(bool isHandTracked, Chirality whichHand);
  void NotifyPalmFacingCamera(bool isPalmFacingCamera, Chirality whichHand);
  void NotifyPinchChanged(bool isPinching, Chirality whichHand);

  // Grabbing
  Vector3 GetPosition();
  bool CanBeGrabbed();
  /// <summary> May return false is the grab is unsuccessful (or, e.g., if the IWearable cannot be grabbed.) </summary>
  bool BeGrabbedBy(Transform grabber);
  void ReleaseFromGrab(Transform grabber);

}
