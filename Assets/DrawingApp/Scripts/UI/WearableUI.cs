using UnityEngine;
using System.Collections;
using Leap.Unity;
using UnityEngine.Events;

public class WearableUI : MonoBehaviour {

  #region PUBLIC FIELDS

  [Tooltip("The Transform to follow when the wearable interface element is not independent. (e.g. A transform attached to the palm of a hand.)")]
  public Transform _anchorPoint;

  [Tooltip("A Detector that returns true when the wearable interface should be displayed, and false otherwise.")]
  public PalmDirectionDetector _displayDetector;

  #endregion

  #region PRIVATE ATTRIBUTES

  private bool _canDisplay = true;
  private bool _currentlyDisplaying = true;

  [SerializeField]
  private bool _isIndependent = false;

  #endregion

  #region PROPERTIES

  public bool ShouldDisplay {
    get { return _canDisplay && (IsIndependent || (_displayDetector && _displayDetector.HandModel.IsTracked && _displayDetector.IsActive)); }
  }

  public bool IsIndependent {
    get { return _isIndependent; }
  }

  #endregion

  #region UNITY EVENTS

  public UnityEvent OnStartDisplay;
  public UnityEvent OnStopDisplay;

  #endregion

  #region UNITY CALLBACKS

  protected void OnValidate() {
    if (_anchorPoint != null) {
      this.transform.position = _anchorPoint.position;
      this.transform.rotation = _anchorPoint.rotation;
    }
  }

  protected void Start() {
    UpdateDisplayState();
  }

  protected virtual void Update() {
    UpdateDisplayState();
    UpdateTransform();
  }

  #endregion

  #region PRIVATE METHODS

  private void UpdateDisplayState() {
    if (ShouldDisplay && !_currentlyDisplaying) {
      StartDisplay();
      _currentlyDisplaying = true;
    }
    else if (!ShouldDisplay && _currentlyDisplaying) {
      StopDisplay();
      _currentlyDisplaying = false;
    }
  }

  private void StartDisplay() {
    OnStartDisplay.Invoke();
    _currentlyDisplaying = true;
  }

  private void StopDisplay() {
    OnStopDisplay.Invoke();
    _currentlyDisplaying = false;
  }

  private void UpdateTransform() {
    if (!IsIndependent) {
      this.transform.position = _anchorPoint.position;
      this.transform.rotation = _anchorPoint.rotation;
    }

    // position
    // This calculation looks complicated; it just allows you to set _attachmentOffset in the
    // camera's local space as if the camera were always directly facing the attachment point, even if it's not.
    //Vector3 desiredWorldPosition = _attachmentPoint.position
    //  + (Quaternion.FromToRotation(_cameraToFace.forward, (_attachmentPoint.position - _cameraToFace.position).normalized)
    //      * (_cameraToFace.TransformPoint(Vector3.zero) - _cameraToFace.position));
    //this.transform.position = Vector3.Lerp(this.transform.position, desiredWorldPosition, _snapCoefficient);

    // rotation
    //Quaternion desiredWorldRotation = Quaternion.LookRotation((this.transform.position - _cameraToFace.position).normalized, Vector3.up);
    //this.transform.rotation = Quaternion.Slerp(this.transform.rotation, desiredWorldRotation, _snapCoefficient);

    // independently facing elements
    //for (int i = 0; i < independentFacingElements.Length; i++) {
    //  Transform element = independentFacingElements[i];
    //  desiredWorldRotation = Quaternion.LookRotation((element.position - _cameraToFace.position).normalized, Vector3.up);
    //  element.rotation = Quaternion.Slerp(element.rotation, desiredWorldRotation, 0.5F);
    //}
  }

  #endregion

  #region PUBLIC METHODS

  /// <summary>
  /// Allows the wearable to display when the attached Detector returns true.
  /// </summary>
  public void EnableDisplay() {
    _canDisplay = true;
  }

  /// <summary>
  /// Hides the wearable interface, even if the attached Detector returns true.
  /// </summary>
  public void DisableDisplay() {
    _canDisplay = false;
  }

  /// <summary>
  /// Changes the Transform that the WearableUI follows.
  /// </summary>
  public void SetAnchor(Transform newAnchor) {
    _anchorPoint = newAnchor;
  }

  public void MakeIndependent() {
    _isIndependent = true;
  }

  public void RemoveIndependence() {
    _isIndependent = false;
  }

  #endregion

}
