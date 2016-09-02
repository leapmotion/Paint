using UnityEngine;
using System.Collections.Generic;
using Leap.Unity;

public class WearableUIManager : MonoBehaviour {

  public Transform _centerEyeAnchor;

  [Header("Hand State Tracking")]
  public IHandModel _leftHand;
  public PalmDirectionDetector _leftPalmFacingDetector;
  public PinchDetector _leftPinchDetector;
  public IHandModel _rightHand;
  public PalmDirectionDetector _rightPalmFacingDetector;
  public PinchDetector _rightPinchDetector;

  [Header("Pinch Grabbable Wearables")]
  public float _pinchGrabDistance = 0.05F;

  // Wearable/Anchor registration
  public List<WearableUI> _wearables = new List<WearableUI>();
  public WearableAnchor[] _anchors;

  [Header("Effects")]
  public Material _fadeableAnchorRingMaterial;
  public Material _opaqueAnchorRingMaterial;

  // Hand state tracking
  private bool _isLeftHandTracked;
  private bool _isRightHandTracked;
  private bool _isLeftPalmFacingCamera;
  private bool _isRightPalmFacingCamera;
  private Chirality _lastHandFacingCamera;
  
  // Wearable state tracking
  private WearableUI _leftGrabbedWearable = null;
  private WearableUI _rightGrabbedWearable = null;

  protected void Start() {
    _leftPinchDetector.OnActivate.AddListener(OnLeftPinchDetected);
    _leftPinchDetector.OnDeactivate.AddListener(OnLeftPinchEnded);
    _rightPinchDetector.OnActivate.AddListener(OnRightPinchDetected);
    _rightPinchDetector.OnDeactivate.AddListener(OnRightPinchEnded);
  }

  protected void Update() {
    if (_leftHand.IsTracked && !_isLeftHandTracked) {
      OnLeftHandBeganTracking();
      _isLeftHandTracked = true;
    }
    else if (!_leftHand.IsTracked && _isLeftHandTracked) {
      OnLeftHandStoppedTracking();
      _isLeftHandTracked = false;
    }

    if (_rightHand.IsTracked && !_isRightHandTracked) {
      OnRightHandBeganTracking();
      _isRightHandTracked = true;
    }
    else if (!_rightHand.IsTracked && _isRightHandTracked) {
      OnRightHandStoppedTracking();
      _isRightHandTracked = false;
    }

    if (_leftPalmFacingDetector.IsActive && !_isLeftPalmFacingCamera) {
      OnLeftHandBeganFacingCamera();
      _isLeftPalmFacingCamera = true;
    }
    else if (!_leftPalmFacingDetector.IsActive && _isLeftPalmFacingCamera) {
      OnLeftHandStoppedFacingCamera();
      _isLeftPalmFacingCamera = false;
    }

    if (_rightPalmFacingDetector.IsActive && !_isRightPalmFacingCamera) {
      OnRightHandBeganFacingCamera();
      _isRightPalmFacingCamera = true;
    }
    else if (!_rightPalmFacingDetector.IsActive && _isRightPalmFacingCamera) {
      OnRightHandStoppedFacingCamera();
      _isRightPalmFacingCamera = false;
    }
  }

  private void OnLeftHandBeganTracking() {
    for (int i = 0; i < _wearables.Count; i++) {
      _wearables[i].NotifyHandTracked(true);
    }
  }
  private void OnLeftHandStoppedTracking() {
    for (int i = 0; i < _wearables.Count; i++) {
      _wearables[i].NotifyHandTracked(false);
    }
  }
  private void OnLeftHandBeganFacingCamera() {
    for (int i = 0; i < _wearables.Count; i++) {
      _wearables[i].NotifyPalmFacingCamera(true);
    }
  }
  private void OnLeftHandStoppedFacingCamera() {
    for (int i = 0; i < _wearables.Count; i++) {
      _wearables[i].NotifyPalmFacingCamera(false);
    }
  }

  private void OnRightHandBeganTracking() {
    //for (int i = 0; i < _rWearables.Count; i++) {
    //  _rWearables[i].NotifyHandTracked(true);
    //}
  }
  private void OnRightHandStoppedTracking() {
    //for (int i = 0; i < _rWearables.Count; i++) {
    //  _rWearables[i].NotifyHandTracked(false);
    //}
  }
  private void OnRightHandBeganFacingCamera() {
    //for (int i = 0; i < _rWearables.Count; i++) {
    //  _rWearables[i].NotifyPalmFacingCamera(true);
    //}
  }
  private void OnRightHandStoppedFacingCamera() {
    //for (int i = 0; i < _rWearables.Count; i++) {
    //  _rWearables[i].NotifyPalmFacingCamera(false);
    //}
  }

  private void OnLeftPinchDetected() {
    TryGrab(EvaluatePossiblePinch(_leftPinchDetector), Chirality.Left);
  }

  private void OnLeftPinchEnded() {
    if (_leftGrabbedWearable != null) {
      _leftGrabbedWearable.ReleaseFromGrab();
    }
  }

  private void OnRightPinchDetected() {
    TryGrab(EvaluatePossiblePinch(_rightPinchDetector), Chirality.Right);
  }

  private void OnRightPinchEnded() {
    if (_rightGrabbedWearable != null) {
      _rightGrabbedWearable.ReleaseFromGrab();
    }
  }

  /// <summary> Returns the closest WearableUI to the PinchDetector, or null of they are all further than _pinchGrabDistance.</summary>
  private WearableUI EvaluatePossiblePinch(PinchDetector pinchToTest) {
    WearableUI closestWearableUI = null;
    float closestDistance = 1000000F;
    float pinchWearableDistance = 0F;
    for (int i = 0; i < _wearables.Count; i++) {
      pinchWearableDistance = Vector3.Distance(_wearables[i].transform.position, pinchToTest.transform.position);
      if (pinchWearableDistance < _pinchGrabDistance && pinchWearableDistance < closestDistance) {
        closestDistance = pinchWearableDistance;
        closestWearableUI = _wearables[i];
      }
    }
    return closestWearableUI;
  }

  private void TryGrab(WearableUI toGrab, Chirality whichHand) {
    if (toGrab == null) return;
    if (toGrab.BeGrabbedBy((whichHand == Chirality.Left ? _leftPinchDetector.transform : _rightPinchDetector.transform))) {
      if (whichHand == Chirality.Left) {
        _leftGrabbedWearable = toGrab;
      }
      else {
        _rightGrabbedWearable = toGrab;
      }
    }
  }

  // TODO: Delete me if this isn't needed anywhere.
  //public Vector3 GetPinchGrabbedVelocity(WearableUI grabbedWearable) {
  //  Leap.Hand hand = null;
  //  if (_leftGrabbedWearable == grabbedWearable) {
  //    hand = _leftHand.GetLeapHand();
  //  }
  //  else if (_rightGrabbedWearable == grabbedWearable) {
  //    hand = _rightHand.GetLeapHand();
  //  }

  //  if (hand != null) {
  //    return ((hand.Fingers[(int)Leap.Finger.FingerType.TYPE_INDEX].TipVelocity
  //      + hand.Fingers[(int)Leap.Finger.FingerType.TYPE_THUMB].TipVelocity) / 2F).ToVector3();
  //  }
  //  else {
  //    Debug.LogError("[WearableUIManager] Grabbed WearableUI is not tracked as grabbed by this WearableUIManager.");
  //    return Vector3.zero;
  //  }
  //}

  public void RegisterWearable(WearableUI wearable) {
    for (int i = 0; i < _wearables.Count; i++) {
      if (_wearables[i] == wearable) {
        return;
      }
    }
    _wearables.Add(wearable);
  }

  public Vector3 GetLookDirection() {
    return _centerEyeAnchor.transform.forward;
  }

  public Transform GetCenterEyeAnchor() {
    return _centerEyeAnchor;
  }

}
