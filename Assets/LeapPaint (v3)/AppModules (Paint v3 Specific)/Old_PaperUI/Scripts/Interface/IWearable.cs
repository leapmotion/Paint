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

}
