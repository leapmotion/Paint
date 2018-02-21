using UnityEngine;
using Leap.Unity;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Leap.Unity.LeapPaint_v3 {

  public class WearableManager : MonoBehaviour {

    public Action OnGrabBegin = () => { };
    public Action OnGrabEnd = () => { };

    public Transform _centerEyeAnchor;

    [Header("Hand State Tracking")]
    public HandModelBase _leftHand;
    public PalmDirectionDetector _leftPalmFacingDetector;
    public HandModelBase _rightHand;
    public PalmDirectionDetector _rightPalmFacingDetector;

    [Header("Pinch Grabbable Wearables")]
    public float _pinchGrabDistance = 0.05F;

    // Wearable/Anchor registration
    public WearableUI[] _wearableUIs;
    public WearableAnchor[] _wearableAnchors;
    public List<IWearable> _wearables = new List<IWearable>();

    // Hand state tracking
    private bool _isLeftHandTracked;
    private bool _isRightHandTracked;
    private bool _isLeftPalmFacingCamera;
    private bool _isRightPalmFacingCamera;
    private Chirality _lastHandFacingCamera;

    protected void Start() {
      for (int i = 0; i < _wearableUIs.Length; i++) {
        _wearables.Add(_wearableUIs[i]);
      }
      for (int i = 0; i < _wearableAnchors.Length; i++) {
        _wearables.Add(_wearableAnchors[i]);
        _wearableAnchors[i].ManualInitialize();
      }
    }

    protected void FixedUpdate() {
      for (int i = 0; i < _wearableAnchors.Length; i++) {
        _wearableAnchors[i].ManualFixedUpdate();
      }
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
        // Disable right-hand palm data to prevent the menu from appearing
        // on the right hand. -Nick 02/21/2018
        //OnRightHandBeganFacingCamera();
        //_isRightPalmFacingCamera = true;
      }
      else if (!_rightPalmFacingDetector.IsActive && _isRightPalmFacingCamera) {
        // Disable right-hand palm data to prevent the menu from appearing
        // on the right hand. -Nick 02/21/2018
        //OnRightHandStoppedFacingCamera();
        //_isRightPalmFacingCamera = false;
      }
    }

    private void OnLeftHandBeganTracking() {
      for (int i = 0; i < _wearables.Count; i++) {
        _wearables[i].NotifyHandTracked(true, Chirality.Left);
      }
    }
    private void OnLeftHandStoppedTracking() {
      for (int i = 0; i < _wearables.Count; i++) {
        _wearables[i].NotifyHandTracked(false, Chirality.Left);
      }
    }
    private void OnLeftHandBeganFacingCamera() {
      for (int i = 0; i < _wearables.Count; i++) {
        _wearables[i].NotifyPalmFacingCamera(true, Chirality.Left);
      }
    }
    private void OnLeftHandStoppedFacingCamera() {
      for (int i = 0; i < _wearables.Count; i++) {
        _wearables[i].NotifyPalmFacingCamera(false, Chirality.Left);
      }
    }

    private void OnRightHandBeganTracking() {
      for (int i = 0; i < _wearables.Count; i++) {
        _wearables[i].NotifyHandTracked(true, Chirality.Right);
      }
    }
    private void OnRightHandStoppedTracking() {
      for (int i = 0; i < _wearables.Count; i++) {
        _wearables[i].NotifyHandTracked(false, Chirality.Right);
      }
    }
    private void OnRightHandBeganFacingCamera() {
      for (int i = 0; i < _wearables.Count; i++) {
        _wearables[i].NotifyPalmFacingCamera(true, Chirality.Right);
      }
    }
    private void OnRightHandStoppedFacingCamera() {
      for (int i = 0; i < _wearables.Count; i++) {
        _wearables[i].NotifyPalmFacingCamera(false, Chirality.Right);
      }
    }

    /// <summary> Returns the closest WearableUI to the PinchDetector, or null of they are all further than _pinchGrabDistance.</summary>
    private IWearable EvaluatePossiblePinch(PinchDetector pinchToTest) {
      IWearable closestWearable = null;
      float closestDistance = 1000000F;
      float pinchWearableDistance = 0F;
      for (int i = 0; i < _wearables.Count; i++) {
        if (_wearables[i].CanBeGrabbed()) {
          pinchWearableDistance = Vector3.Distance(_wearables[i].GetPosition(), pinchToTest.transform.position);
          if (pinchWearableDistance < _pinchGrabDistance && pinchWearableDistance < closestDistance) {
            closestDistance = pinchWearableDistance;
            closestWearable = _wearables[i];
          }
        }
      }
      return closestWearable;
    }

    public Transform GetCenterEyeAnchor() {
      return _centerEyeAnchor;
    }

    public Vector3 ValidateTargetWorkstationPosition(Vector3 targetPosition, WearableUI workstation) {
      float targetPosHeight = targetPosition.y;
      for (int i = 0; i < _wearableUIs.Length; i++) {
        if (_wearableUIs[i] == workstation || !_wearableUIs[i].IsWorkstation) continue;
        else {
          Vector3 estPosition = _wearableUIs[i].transform.position; // established workstation position
          Vector3 estPositionToTarget = targetPosition - estPosition;
          float totalDangerRadius = workstation.GetWorkstationDangerZoneRadius() + _wearableUIs[i].GetWorkstationDangerZoneRadius();
          if (estPositionToTarget.magnitude >= totalDangerRadius) continue;
          else {
            estPositionToTarget = new Vector3(estPositionToTarget.x, 0F, estPositionToTarget.z);
            if (estPositionToTarget.magnitude < 0.00001F) {
              Debug.LogWarning("Workstation position validation: Too close on XZ -- doing camera logic.");
              Vector3 cameraXZAlignedEstWorkstationPos = new Vector3(estPosition.x, Camera.main.transform.position.y, estPosition.z);
              Vector3 estOffsetDirection = Quaternion.LookRotation(cameraXZAlignedEstWorkstationPos - Camera.main.transform.position) * Vector3.right;
              targetPosition = estPosition + estOffsetDirection * totalDangerRadius;
            }
            else {
              targetPosition = estPosition + estPositionToTarget.normalized * totalDangerRadius;
            }
            targetPosition = new Vector3(targetPosition.x, targetPosHeight, targetPosition.z);
          }
        }
      }
      return targetPosition;
    }

  }


}
