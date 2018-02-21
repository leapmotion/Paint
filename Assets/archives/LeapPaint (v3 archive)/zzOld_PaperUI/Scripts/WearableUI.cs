using UnityEngine;
using System;
using System.Collections;
using Leap.Unity;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity.Animation;
using Leap.Unity.Interaction;

namespace Leap.Unity.LeapPaint_v3 {
  
  public class WearableUI : AnchoredBehaviour, IWearable, IRuntimeGizmoComponent {

    public Action OnActivateMarble = () => { };
    public Action OnWorkstationActivated = () => { };

    [Header("Wearable UI")]
    public MeshRenderer _appearanceExplosionRenderer;
    public InteractionBehaviour _intObj;
    public Collider _marbleCollider;
    public MeshRenderer _marbleRenderer;
    public SoundEffect _activateEffect;
    public SoundEffect _grabEffect;
    public SoundEffect _throwEffect;
    public float _maxVolumeVelocity = 1;
    public SoundEffect _returnEffect;

    private WearableManager _manager;
    private WearableAnchor _wearableAnchor;
    public WearableAnchor WearableAnchor {
      get { return _wearableAnchor; }
    }
    private Chirality _anchorChirality = Chirality.Left;
    public Chirality AnchorChirality {
      get { return _anchorChirality; }
    }
    private Chirality _displayingChirality = Chirality.Left;
    protected Chirality DisplayingChirality {
      get { return _displayingChirality; }
      set { _displayingChirality = value; }
    }

    private bool _isDisplayingOnAnyHand = false;

    protected override void OnEnable() {
      base.OnEnable();

      if (Application.isPlaying) {
        initGraspAndRelease();
      }
    }

    protected virtual void OnDisable() {
      if (Application.isPlaying) {
        teardownGraspAndRelease();
      }
    }

    protected virtual void Start() {
      InitAppearVanish();

      _manager = GameObject.FindObjectOfType<WearableManager>();
      WearableAnchor anchor = _anchorTransform.GetComponent<WearableAnchor>();
      if (anchor != null) {
        _wearableAnchor = anchor;
        _wearableAnchor.OnAnchorChiralityChanged += DoOnAnchorChiralityChanged;
      }

      if (Application.isPlaying) {
        InitMarbleTouch();
      }
    }

    protected virtual void FixedUpdate() {
      FixedAppearVanishUpdate();
      FixedGrabUpdate();
      FixedWorkstationTransitionUpdate();
    }

    protected override void Update() {
      base.Update();

      if (Application.isPlaying) {
        MarbleTouchUpdate();
      }
    }

    protected virtual void OnApplicationQuit() {
      FinalizeMarbleTouch();
    }

    private void RefreshVisibility() {
      if (_isAttached) {
        if (!_isDisplayingOnAnyHand) {
          if (_isLeftHandTracked && _isLeftPalmFacingCamera && !_isLeftHandPinching) {
            ScheduleAppear(Chirality.Left);
          }
          else if (_isRightHandTracked && _isRightPalmFacingCamera && !_isRightHandPinching) {
            ScheduleAppear(Chirality.Right);
          }
          // Not sure what this was for. TODO: If there's a visiblity bug, investigate
          //else if (_anchorTransform != _grabbingTransform) {
          //  ScheduleVanish();
          //}
        }
        else if (isGrasped) {
          // If currently grabbed, don't change visibility.
        }
        else {
          if (_wearableAnchor._chirality == Chirality.Left && (!_isLeftHandTracked || !_isLeftPalmFacingCamera || _isLeftHandPinching)) {
            ScheduleVanish();
          }
          else if (_wearableAnchor._chirality == Chirality.Right && (!_isRightHandTracked || !_isRightPalmFacingCamera || _isRightHandPinching)) {
            ScheduleVanish();
          }
        }
      }
    }

    private void MoveBackToAnchor() {
      RefreshVisibility();

      // this should be redundant, it no longer changes
      _anchorTransform = _wearableAnchor.transform;

      AttachToAnchor();

      DoOnReturnedToAnchor();
    }

    protected virtual void DoOnAnchorChiralityChanged(Chirality whichHand) {
      if (whichHand != _anchorChirality) {
        _anchorChirality = whichHand;
      }
    }

    protected virtual void DoOnReturnedToAnchor() {
      _returnEffect.PlayOnTransform(transform);
    }

    public virtual float GetAnchoredDangerZoneRadius() {
      return 0.1F;
    }

    #region Wearable Implementation

    private bool _isLeftHandTracked = false;
    private bool _isLeftPalmFacingCamera = false;
    private bool _isLeftHandPinching = false;
    private bool _isRightHandTracked = false;
    private bool _isRightPalmFacingCamera = false;
    private bool _isRightHandPinching = false;

    public void NotifyHandTracked(bool isHandTracked, Chirality whichHand) {
      if (whichHand == Chirality.Left) {
        _isLeftHandTracked = isHandTracked;
      }
      else {
        _isRightHandTracked = isHandTracked;
      }
      RefreshVisibility();
    }

    public void NotifyPalmFacingCamera(bool isPalmFacingCamera, Chirality whichHand) {
      if (whichHand == Chirality.Left) {
        _isLeftPalmFacingCamera = isPalmFacingCamera;
      }
      else {
        _isRightPalmFacingCamera = isPalmFacingCamera;
      }
      RefreshVisibility();
    }

    public void NotifyPinchChanged(bool isPinching, Chirality whichHand) {
      if (whichHand == Chirality.Left) {
        _isLeftHandPinching = isPinching;
      }
      else {
        _isRightHandPinching = isPinching;
      }
      RefreshVisibility();
    }

    public Vector3 GetPosition() {
      return this.transform.position;
    }

    // See Grab State region for grab implementation.

    #endregion

    #region Appear / Vanish

    private Tween _appearTween;
    private bool _appearScheduled = false;
    private bool _vanishScheduled = false;

    private void InitAppearVanish() {
      if (Application.isPlaying) {
        _appearTween = ConstructAppearTween();
        _appearTween.progress = 0.001F;
        Vanish();
      }
    }

    private void ScheduleAppear(Chirality whichHand) {
      _appearScheduled = true;
    }
    private void ScheduleVanish() {
      _vanishScheduled = true;
    }

    private void FixedAppearVanishUpdate() {
      if (_appearScheduled) {
        Appear();
        _appearScheduled = false;
        if (_vanishScheduled) _vanishScheduled = false;
      }
      else if (_vanishScheduled) {
        Vanish();
        _vanishScheduled = false;
      }
    }

    private void Appear() {
      _appearTween.Play(Direction.Forward);
    }

    private void Vanish() {
      _appearTween.Play(Direction.Backward);
    }

    private Tween ConstructAppearTween() {
      return Tween.Persistent().Target(this.transform)
        .LocalScale(Vector3.one / 1000F, Vector3.one)
        .Target(_appearanceExplosionRenderer.material)
        .Color(Color.white, new Color(1F, 1F, 1F, 0F))
        .OverTime(0.2F)
        .Smooth(SmoothType.Smooth)
        .OnLeaveStart(DoOnStartAppearing)
        .OnReachEnd(OnExplosionShouldDisappear)
        .OnLeaveEnd(OnExplosionShouldAppear)
        .OnReachStart(DoOnFinishedVanishing);
    }

    private void DoOnStartAppearing() {
      _isDisplayingOnAnyHand = true;
      OnExplosionShouldAppear();
    }

    private void OnExplosionShouldAppear() {
      _appearanceExplosionRenderer.enabled = true;
    }

    private void DoOnFinishedVanishing() {
      _isDisplayingOnAnyHand = false;
      RefreshVisibility();
    }

    private void OnExplosionShouldDisappear() {
      _appearanceExplosionRenderer.enabled = false;
    }

    public bool IsDisplaying {
      get { return _appearTween.isValid && _appearTween.progress > 0.01F; }
    }

    #endregion

    #region Marble Touching

    private const float MARBLE_COOLDOWN = 0.05F;
    [SerializeField]
    private float _marbleCooldownTimer = 0F;
    [SerializeField]
    private bool _marbleReady = true;

    [SerializeField]
    private bool _fingerTouchingMarble = false;
    [SerializeField]
    private bool _fingerTouchingDepthCollider = false;
    private CapsuleCollider _marbleDepthCollider;

    private Pulsator _marblePulsator;

    private void InitMarbleTouch() {
      PassTriggerEvents triggerEvents = _marbleCollider.GetComponent<PassTriggerEvents>();
      if (triggerEvents == null) {
        triggerEvents = _marbleCollider.gameObject.AddComponent<PassTriggerEvents>();
      }
      triggerEvents.PassedOnTriggerEnter.AddListener(NotifyFingerEnterMarble);
      triggerEvents.PassedOnTriggerExit.AddListener(NotifyFingerExitMarble);

      if (_marbleDepthCollider == null) {
        GameObject depthColliderObj = new GameObject();
        depthColliderObj.transform.parent = _marbleCollider.transform.parent;
        depthColliderObj.transform.localRotation = Quaternion.Euler(new Vector3(90F, 0F, 0F));
        depthColliderObj.transform.localScale = Vector3.one;
        depthColliderObj.name = "Marble Depth Touch Collider";

        _marbleDepthCollider = depthColliderObj.AddComponent<CapsuleCollider>();
        _marbleDepthCollider.isTrigger = true;
        _marbleDepthCollider.radius = 0.96F;
        _marbleDepthCollider.height = 2.88F;
        _marbleDepthCollider.transform.localPosition = new Vector3(0F, 0F, -1.144F);
        triggerEvents = depthColliderObj.AddComponent<PassTriggerEvents>();
        triggerEvents.PassedOnTriggerEnter.AddListener(NotifyFingerEnterDepthCollider);
        triggerEvents.PassedOnTriggerExit.AddListener(NotifyFingerExitDepthCollider);
      }

      if (_marblePulsator == null) {
        _marblePulsator = gameObject.AddComponent<Pulsator>();
        _marblePulsator._restValue = 0F;
        _marblePulsator._pulseValue = 0.4F;
        _marblePulsator._holdValue = 0.15F;
        _marblePulsator._speed = 2F;
        _marblePulsator.OnValueChanged += DoOnMarblePulsateValue;
      }
    }

    private void MarbleTouchUpdate() {
      if (!_marbleReady && _marbleCooldownTimer > 0F) {
        _marbleCooldownTimer -= Time.deltaTime;
        if (_marbleCooldownTimer <= 0F) {
          _marbleCooldownTimer = 0F;
          _marbleReady = true;
          _marblePulsator.Release();
        }
      }
    }

    private void DoOnMarblePulsateValue(float normalizedValue) {
      if (Application.isPlaying) {
        _marbleRenderer.material.SetColor("_EmissionColor", Color.Lerp(Color.black, Color.white, normalizedValue));
      }
    }

    public void NotifyFingerEnterMarble(Collider fingerCollider) {
      if (this.name.Equals("Color")) {
        Debug.Log("RECEIVE NotifyFingerEnterMarble");
      }

      _fingerTouchingMarble = true;

      if (_marbleReady) {
        _marblePulsator.WarmUp();
      }
    }

    public void NotifyFingerExitMarble(Collider fingerCollider) {
      _fingerTouchingMarble = false;

      if (_marbleReady) {
        DoOnMarbleActivated();
        _marbleReady = false;
      }
      RefreshMarbleCountdown();
    }

    public void NotifyFingerEnterDepthCollider(Collider fingerCollider) {
      if (this.name.Equals("Color")) {
        Debug.Log("RECEIVE NotifyFingerEnterDepthCollider");
      }

      _fingerTouchingDepthCollider = true;
    }

    public void NotifyFingerExitDepthCollider(Collider fingerCollider) {
      _fingerTouchingDepthCollider = false;
      RefreshMarbleCountdown();
    }

    private void RefreshMarbleCountdown() {
      if (!_fingerTouchingDepthCollider && !_fingerTouchingMarble && !_marbleReady) {
        _marbleCooldownTimer = MARBLE_COOLDOWN;
      }
    }

    private void FinalizeMarbleTouch() {
      DestroyImmediate(_marbleDepthCollider.gameObject);
    }

    protected virtual void DoOnMarbleActivated() {
      OnActivateMarble();
      _activateEffect.PlayOnTransform(transform);
      _marblePulsator.Activate();
    }

    #endregion

    #region Grab and Release

    private const float VELOCITY_DEPENDENT_RETURN_TO_ANCHOR_DISTANCE = 0.2F;
    private const float OVERRIDE_VELOCITY_RETURN_TO_ANCHOR_DISTANCE = 0.05F;

    private bool _hasGrabVelocity = false;
    private int _framesGrabbed = 0;
    private Vector3 _curPosition;
    private Vector3 _lastPosition;

    protected bool isGrasped {
      get { return _intObj.isGrasped; }
    }

    // TODO: upgrade note: used by pinch grabbing, probably should delete.
    public bool CanBeGrabbed() {
      return _isDisplayingOnAnyHand;
    }

    private void initGraspAndRelease() {
      _intObj.OnGraspBegin -= DoOnGrabbed;
      _intObj.OnGraspEnd -= DoOnReleasedFromGrab;
      _intObj.OnGraspBegin += DoOnGrabbed;
      _intObj.OnGraspEnd += DoOnReleasedFromGrab;
    }

    private void teardownGraspAndRelease() {
      _intObj.OnGraspBegin -= DoOnGrabbed;
      _intObj.OnGraspEnd -= DoOnReleasedFromGrab;
    }

    protected void FixedGrabUpdate() {
      if (isGrasped) {
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

    protected virtual void DoOnGrabbed() {
      ReleaseFromAnchor();
      
      _isWorkstation = false;

      _grabEffect.PlayOnTransform(transform);
    }

    private void DoOnReleasedFromGrab() {
      bool shouldActivateWorkstation = EvaluateShouldActivateWorkstation();

      if (shouldActivateWorkstation) {
        ActivateWorkstationTransition();
      }
      else {
        _marbleReady = false;
        _marbleCooldownTimer = MARBLE_COOLDOWN * 2f;
        MoveBackToAnchor();
      }
    }

    private bool EvaluateShouldActivateWorkstation() {
      bool shouldActivateWorkstation = false;

      // Old logic: When the menu could switch hands,
      // a release from the hand that contains the menu can NEVER re-dock, so always
      // return true.
      // Commented out because the menu can't switch and also relies on old pinch logic.
      {
        //if ((_wearableAnchor._chirality == Chirality.Left
        //     && (WearableUI)_manager.LastGrabbedByLeftHand() == this)
        //    || (_wearableAnchor._chirality == Chirality.Right
        //        && (WearableUI)_manager.LastGrabbedByRightHand() == this)) {
        //  return true;
        //}
      }
      // New logic: Nothing to replace it; shouldn't have to ask whether to launch the
      // workstation if a release occurs from a left hand.

      if ((_wearableAnchor._chirality == Chirality.Left && _isLeftHandTracked)
       || (_wearableAnchor._chirality == Chirality.Right && _isRightHandTracked)) {

        float anchorDistance = Vector3.Distance(this.transform.position, _wearableAnchor.transform.position);
        if (anchorDistance < OVERRIDE_VELOCITY_RETURN_TO_ANCHOR_DISTANCE) {
          shouldActivateWorkstation = false;
        }
        else if (anchorDistance < VELOCITY_DEPENDENT_RETURN_TO_ANCHOR_DISTANCE) {
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
    private bool _isWorkstation = false;

    private float _optimalWorkstationDistance = 0.6F;

    private bool _isRigidbodyUpdateScheduled = false;
    private Vector3 _scheduledRigidbodyPosition;
    private Quaternion _scheduledRigidbodyRotation;
    private Vector3 _scheduledRigidbodyVelocity;

    public bool IsWorkstation {
      get { return _isWorkstation; }
    }
    public Transform WorkstationTargetTransform {
      get { return _workstationTargetLocation; }
    }

    public virtual float GetWorkstationDangerZoneRadius() {
      return 0.2F;
    }

    private Transform _lerpFrom;
    private Transform _lerpTo;
    private Tween ConstructWorkstationPlacementTween(Transform from, Transform to, float overTime) {
      _lerpFrom = from;
      _lerpTo = to;
      return Tween.Single().Target(this.transform)
        .Rotation(from, to)
        .Value(0F, 1F, OnGetWorkstationEmergeTransformLerpAmount)
        .OverTime(overTime)
        .Smooth(SmoothType.Smooth)
        .OnLeaveStart(DoOnMovementToWorkstationBegan)
        .OnReachEnd(DoOnMovementToWorkstationFinished);
    }

    private void OnGetWorkstationEmergeTransformLerpAmount(float lerpAmount) {
      this.transform.position = Vector3.Lerp(_lerpFrom.position, _lerpTo.position, lerpAmount);
    }

    protected void ActivateWorkstationTransitionFromAnchor() {
      ActivateWorkstationTransition(true);
    }

    private void ActivateWorkstationTransition(bool ignoreLowVelocity=false) {
      _isAttached = false;
      _isWorkstation = true;

      // Create rigidbody throw simulation object
      if (_simulatedThrownBody == null) {
        GameObject bodyObj = new GameObject(this.name + " Rigidbody Simulation Object");
        _simulatedThrownBody = bodyObj.AddComponent<Rigidbody>();
        _simulatedThrownBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _simulatedThrownBody.interpolation = RigidbodyInterpolation.Interpolate;
        bodyObj.AddComponent<DestroyAfterDuration>();
      }
      else {
        _simulatedThrownBody.GetComponent<DestroyAfterDuration>().SetTimer(5F);
      }
      Vector3 throwVelocity = GetGrabVelocity();
      _simulatedThrownBody.transform.rotation = this.transform.rotation;
      ScheduleRigidbodyUpdate(this.transform.position, this.transform.rotation, throwVelocity);

      _throwEffect.PlayOnTransform(transform, Mathf.Clamp01(throwVelocity.magnitude / _maxVolumeVelocity));

      // Construct target workstation location object
      if (_workstationTargetLocation != null) {
        Destroy(_workstationTargetLocation.gameObject);
      }
      _workstationTargetLocation = new GameObject("WearableUI Workstation Target Location Object").GetComponent<Transform>();
      SetGoodWorkstationTransform(_workstationTargetLocation, this.transform.position, throwVelocity, ignoreLowVelocity);
    
      // Determine how long the tween should take
      Vector3 targetPosition = _workstationTargetLocation.position;
      Vector3 velocityOffsetPosition = this.transform.position + 0.5F * Vector3.Distance(targetPosition, this.transform.position) * (throwVelocity.normalized);
      float timeCoefficient = 1.5F;
      float timeToTween = timeCoefficient * Vector3.Distance(velocityOffsetPosition, targetPosition);

      // Construct tween and play it.
      ConstructWorkstationPlacementTween(_simulatedThrownBody.transform, _workstationTargetLocation, timeToTween).Play();
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

    private void SetGoodWorkstationTransform(Transform toSet, Vector3 initPosition, Vector3 initVelocity, bool ignoreLowVelocity=false) {
      Transform centerEyeAnchor = _manager.GetCenterEyeAnchor();

      Vector3 workstationPosition;
      bool modifyHeight = true;
      if (initVelocity.magnitude < 0.7F && !ignoreLowVelocity) {
        // Just use current position as the position to choose.
        workstationPosition = initPosition;
        modifyHeight = false;
      }
      else {
        // Find projection direction
        Vector3 projectDirection;
        Vector3 groundAlignedInitVelocity = new Vector3(initVelocity.x, 0F, initVelocity.z);
        Vector3 effectiveLookDirection = centerEyeAnchor.forward;
        effectiveLookDirection = new Vector3(effectiveLookDirection.x, 0F, effectiveLookDirection.z);
        if (effectiveLookDirection.magnitude < 0.01F) {
          if (effectiveLookDirection.y > 0F) {
            projectDirection = -centerEyeAnchor.up;
          }
          else {
            projectDirection = centerEyeAnchor.up;
          }
        }
        if (initVelocity.magnitude < 0.5F || groundAlignedInitVelocity.magnitude < 0.01F) {
          projectDirection = effectiveLookDirection;
        }
        else {
          projectDirection = groundAlignedInitVelocity;
        }

        // Add a little bit of the effective look direction to the projectDirection to skew towards winding up
        // in front of the user unless they really throw it hard behind them
        float forwardSkewAmount = 1F;
        projectDirection += effectiveLookDirection * forwardSkewAmount;
        projectDirection = projectDirection.normalized;

        // Find good workstation position based on projection direction
        Vector3 workstationDirection = (initPosition + (projectDirection * 20F) - centerEyeAnchor.position);
        Vector3 groundAlignedWorkstationDirection = new Vector3(workstationDirection.x, 0F, workstationDirection.z).normalized;
        workstationPosition = centerEyeAnchor.position + _optimalWorkstationDistance * groundAlignedWorkstationDirection;

        // Allow the WearableManager to pick a new location if the target location overlaps with another workstation
        workstationPosition = _manager.ValidateTargetWorkstationPosition(workstationPosition, this);
      }

      // Find a good workstation orientation
      Vector3 optimalLookVector = GetOptimalOrientationLookVector(centerEyeAnchor, workstationPosition);

      // Set the workstation target transform.
      toSet.position = new Vector3(workstationPosition.x,
        (modifyHeight ? centerEyeAnchor.position.y + GetOptimalWorkstationVerticalOffset() : workstationPosition.y),
        workstationPosition.z);
      toSet.rotation = Quaternion.LookRotation(optimalLookVector);
    }

    protected virtual float GetOptimalWorkstationVerticalOffset() { return -0.25F; }

    protected virtual Vector3 GetOptimalOrientationLookVector(Transform centerEyeAnchor, Vector3 fromPosition) {
      Vector3 towardsCamera = centerEyeAnchor.position - fromPosition;
      return new Vector3(towardsCamera.x, 0F, towardsCamera.z).normalized;
    }

    protected virtual void DoOnMovementToWorkstationBegan() {
      _isAttached = false;
    }

    protected virtual void DoOnMovementToWorkstationFinished() {
      OnWorkstationActivated();
    }

    #endregion

    #region Gizmos

    private bool _wearableUIGizmosEnabled = false;

    public override void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (_wearableUIGizmosEnabled) {
        drawer.color = Color.red;
        if (IsWorkstation) {
          drawer.DrawWireSphere(this.transform.position, GetWorkstationDangerZoneRadius());
        }
        else {
          drawer.DrawWireSphere(this.transform.position, GetAnchoredDangerZoneRadius());
        }
      }
    }

    #endregion

  }

}

