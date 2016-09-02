using UnityEngine;
using System.Collections;
using Leap.Unity;

public class WearableUI : AnchoredBehaviour {

  [Header("Wearable UI")]
  public MeshRenderer _appearanceExplosionRenderer;

  private float _returnToAnchorDistance = 0.2F;

  private bool _isHandTracked = false;
  private bool _isPalmFacingCamera = false;

  private TweenHandle _appearTween;
  private bool _appearScheduled = false;
  private bool _vanishScheduled = false;

  private WearableUIManager _manager;
  private WearableAnchor _wearableAnchor;

  protected void Start() {
    _appearTween = ConstructAppearTween();

    _manager = GameObject.FindObjectOfType<WearableUIManager>();
    _manager.RegisterWearable(this);
    WearableAnchor anchor = _anchorTransform.GetComponent<WearableAnchor>();
    if (anchor != null) {
      _wearableAnchor = anchor;
    }

    _appearTween.Progress = 1F;
    Vanish();
  }

  protected void FixedUpdate() {
    if (_appearScheduled) {
      Appear();
      _appearScheduled = false;
      if (_vanishScheduled) _vanishScheduled = false;
    }
    else if (_vanishScheduled) {
      Vanish();
      _vanishScheduled = false;
    }

    FixedGrabUpdate();
    FixedWorkstationTransitionUpdate();
  }

  public void NotifyHandTracked(bool isHandTracked) {
    _isHandTracked = isHandTracked;
    RefreshVisibility();
  }

  public void NotifyPalmFacingCamera(bool isPalmFacingCamera) {
    _isPalmFacingCamera = isPalmFacingCamera;
    RefreshVisibility();
  }

  public void RefreshVisibility() {
    if (_isAttached) {
      if (_isHandTracked && _isPalmFacingCamera) {
        ScheduleAppear();
      }
      else if (_anchorTransform != _grabbingTransform) {
        ScheduleVanish();
      }
    }
  }

  public void RefreshAnchorState() {
    if (_grabbingTransform == null) {
      if (_anchorTransform != _wearableAnchor.transform) {
        RefreshVisibility();
      }
      _anchorTransform = _wearableAnchor.transform;
    }
    else {
      if (_anchorTransform != _grabbingTransform) {
        RefreshVisibility();
      }
      _anchorTransform = _grabbingTransform;
    }
  }

  #region Appear / Vanish Tween

  private void ScheduleAppear() {
    _appearScheduled = true;
  }
  private void ScheduleVanish() {
    _vanishScheduled = true;
  }

  private void Appear() {
    if (!_appearTween.IsRunning || _appearTween.Direction == TweenDirection.BACKWARD) {
      _appearTween.Play(TweenDirection.FORWARD);
    }
  }

  private void Vanish() {
    if (!_appearTween.IsRunning || _appearTween.Direction == TweenDirection.FORWARD) {
      _appearTween.Play(TweenDirection.BACKWARD);
    }
  }

  private TweenHandle ConstructAppearTween() {
    return Tween.Target(this.transform)
      .LocalScale(this.transform.localScale / 1000F, this.transform.localScale)
      .Target(_appearanceExplosionRenderer.material)
      .Color(Color.white, new Color(1F, 1F, 1F, 0F))
      .OverTime(0.2F)
      .Smooth(TweenType.SMOOTH)
      .OnLeaveStart(OnExplosionShouldAppear)
      .OnReachEnd(OnExplosionShouldDisappear)
      .OnLeaveEnd(OnExplosionShouldAppear)
      .OnReachStart(OnExplosionShouldDisappear)
      .Keep();
  }

  private void OnExplosionShouldAppear() {
    _appearanceExplosionRenderer.enabled = true;
  }

  private void OnExplosionShouldDisappear() {
    _appearanceExplosionRenderer.enabled = false;
  }

  #endregion

  #region Grab State

  private Transform _grabbingTransform = null;

  private bool _hasGrabVelocity = false;
  private int _framesGrabbed = 0;
  private Vector3 _curPosition;
  private Vector3 _lastPosition;

  protected bool IsGrabbed {
    get { return _grabbingTransform != null; }
  }

  protected void FixedGrabUpdate() {
    if (_grabbingTransform != null) {
      _framesGrabbed += 1;
      _lastPosition = _curPosition;
      _curPosition = this.transform.position;
      if (_framesGrabbed == 2) {
        _hasGrabVelocity = true;
      }
    }
    else {
      if (_framesGrabbed > 0) {
        _framesGrabbed = 0;
        _hasGrabVelocity = false;
        _curPosition = Vector3.zero;
        _lastPosition = Vector3.zero;
      }
    }
  }

  /// <summary> Returns true if the grab is successful, or false if the Wearable refused to be grabbed. </summary>
  public bool BeGrabbedBy(Transform grabber) {
    _grabbingTransform = grabber;
    _isAttached = true;
    RefreshAnchorState();
    DoOnGrabbed();
    return true;
  }

  protected virtual void DoOnGrabbed() { }

  public void ReleaseFromGrab() {
    bool doOnReleased = false;
    if (_grabbingTransform != null) {
      doOnReleased = true;
      _grabbingTransform = null;
    }
    if (doOnReleased) {
      DoOnReleasedFromGrab();
    }
  }

  private void DoOnReleasedFromGrab() {
    bool shouldActivateWorkstation = EvaluateShouldActivateWorkstation();

    if (shouldActivateWorkstation) {
      ActivateWorkstationTransition();
    }
    else {
      RefreshAnchorState();
    }
  }

  private bool EvaluateShouldActivateWorkstation() {
    bool shouldActivateWorkstation = false;
    if (_isHandTracked) {
      if (Vector3.Distance(this.transform.position, _wearableAnchor.transform.position) < _returnToAnchorDistance) {
        if (IsVelocityAwayFromAnchor()) {
          shouldActivateWorkstation = true;
        }
        else {
          shouldActivateWorkstation = false;
        }
      }
      else {
        if (IsVelocityTowardAnchor()) {
          shouldActivateWorkstation = false;
        }
        else {
          shouldActivateWorkstation = true;
        }
      }
    }
    else {
      shouldActivateWorkstation = true;
    }
    return shouldActivateWorkstation;
  }

  private bool IsVelocityAwayFromAnchor() {
    return VelocityTowardsAnchorNormalizedDotProduct() < -0.2F;
  }

  private bool IsVelocityTowardAnchor() {
    return VelocityTowardsAnchorNormalizedDotProduct() > 0.85F;
  }

  private float VelocityTowardsAnchorNormalizedDotProduct() {
    Vector3 grabVelocity = GetGrabVelocity();
    Vector3 wearableToAnchor = _wearableAnchor.transform.position - this.transform.position;
    return Vector3.Dot(grabVelocity.normalized, wearableToAnchor.normalized);
  }

  private Vector3 GetGrabVelocity() {
    if (!_hasGrabVelocity) {
      Debug.Log("Don't have grab velocity, returning zero.");
      return Vector3.zero;
    }
    Vector3 grabVelocity = (_curPosition - _lastPosition) / Time.fixedDeltaTime;
    return grabVelocity;
  }

  #endregion

  #region Workstation Transition

  private Rigidbody _simulatedThrownBody = null;
  private Transform _workstationTargetLocation = null;

  private TweenHandle _workstationPlacementTween;

  private float _optimalWorkstationDistance = 0.6F;
  private float _optimalWorkstationVerticalOffset = -0.3F;

  private bool _isRigidbodyUpdateScheduled = false;
  private Vector3 _scheduledRigidbodyPosition;
  private Quaternion _scheduledRigidbodyRotation;
  private Vector3 _scheduledRigidbodyVelocity;

  private Transform _lerpFrom;
  private Transform _lerpTo;
  private TweenHandle ConstructWorkstationPlacementTween(Transform from, Transform to) {
    _lerpFrom = from;
    _lerpTo = to;
    return Tween.Target(this.transform)
      .Rotation(from, to)
      .Value(0F, 1F, OnGetWorkstationEmergeTransformLerpAmount)
      .OverTime(0.5F)
      .Smooth(TweenType.SMOOTH)
      .OnLeaveStart(DoOnMovementToWorkstationBegan)
      .OnReachEnd(DoOnMovementToWorkstationFinished)
      .Keep();
  }

  private void OnGetWorkstationEmergeTransformLerpAmount(float lerpAmount) {
    this.transform.position = Vector3.Lerp(_lerpFrom.position, _lerpTo.position, lerpAmount);
  }

  private void ActivateWorkstationTransition() {
    // Create rigidbody throw simulation object
    if (_simulatedThrownBody == null) {
      GameObject bodyObj = new GameObject("WearableUI Rigidbody Simulation Object");
      _simulatedThrownBody = bodyObj.AddComponent<Rigidbody>();
      _simulatedThrownBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
      _simulatedThrownBody.interpolation = RigidbodyInterpolation.Interpolate;
      bodyObj.AddComponent<RigidbodyGizmo>();
    }
    Vector3 throwVelocity = GetGrabVelocity();
    ScheduleRigidbodyUpdate(this.transform.position, this.transform.rotation, throwVelocity);

    // Construct target workstation location object
    if (_workstationTargetLocation != null) {
      Destroy(_workstationTargetLocation.gameObject);
    }
    _workstationTargetLocation = new GameObject("WearableUI Workstation Target Location Object").GetComponent<Transform>();
    SetGoodWorkstationTransform(_workstationTargetLocation, _simulatedThrownBody.transform.position, throwVelocity);

    // Construct tween and play it.
    ConstructWorkstationPlacementTween(_simulatedThrownBody.transform, _workstationTargetLocation).Play();
  }

  private void ScheduleRigidbodyUpdate(Vector3 position, Quaternion rotation, Vector3 velocity) {
    _isRigidbodyUpdateScheduled = true;
    _scheduledRigidbodyPosition = position;
    _scheduledRigidbodyRotation = rotation;
    _scheduledRigidbodyVelocity = velocity;
  }

  private void FixedWorkstationTransitionUpdate() {
    if (_isRigidbodyUpdateScheduled) {
      _simulatedThrownBody.transform.position = _scheduledRigidbodyPosition;
      _simulatedThrownBody.transform.rotation = _scheduledRigidbodyRotation;
      _simulatedThrownBody.velocity = _scheduledRigidbodyVelocity;
      _isRigidbodyUpdateScheduled = false;
    }
  }

  private void SetGoodWorkstationTransform(Transform toSet, Vector3 initPosition, Vector3 initVelocity) {
    Transform centerEyeAnchor = _manager.GetCenterEyeAnchor();

    // Find step direction
    Vector3 projectDirection;
    Vector3 groundAlignedInitVelocity = new Vector3(initVelocity.x, 0F, initVelocity.z);
    if (initVelocity.magnitude < 0.5F || groundAlignedInitVelocity.magnitude < 0.01F) {
      Vector3 lookDirection = _manager.GetLookDirection();
      projectDirection = new Vector3(lookDirection.x, 0F, lookDirection.z);
      if (projectDirection.magnitude < 0.01F) {
        if (lookDirection.y > 0F) {
          projectDirection = -centerEyeAnchor.up;
        }
        else {
          projectDirection = centerEyeAnchor.up;
        }
      }
    }
    else {
      projectDirection = groundAlignedInitVelocity;
    }
    projectDirection = projectDirection.normalized;

    // Find good workstation position
    Vector3 workstationPosition;
    Vector3 workstationDirection = (initPosition + (projectDirection * 20F) - centerEyeAnchor.position);
    Vector3 groundAlignedWorkstationDirection = new Vector3(workstationDirection.x, 0F, workstationDirection.z).normalized;
    workstationPosition = _optimalWorkstationDistance * groundAlignedWorkstationDirection;

    // Find a good workstation orientation
    Vector3 optimalLookVector = GetOptimalOrientationLookVector(centerEyeAnchor);

    // Set the workstation target transform.
    toSet.position = new Vector3(workstationPosition.x, centerEyeAnchor.position.y + _optimalWorkstationVerticalOffset, workstationPosition.z);
    toSet.rotation = Quaternion.LookRotation(optimalLookVector);
  }

  protected virtual Vector3 GetOptimalOrientationLookVector(Transform centerEyeAnchor) {
    Vector3 towardsCamera = centerEyeAnchor.position - this.transform.position;
    return new Vector3(towardsCamera.x, 0F, towardsCamera.z).normalized;
  }

  protected virtual void DoOnMovementToWorkstationBegan() {
    _isAttached = false;
  }

  protected virtual void DoOnMovementToWorkstationFinished() { }

  #endregion

}

public static class ColorExtensions {
  public static Color AlphaTo(this Color color, float alpha) {
    return new Color(color.r, color.g, color.b, alpha);
  }
}
