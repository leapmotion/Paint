using Leap.Unity.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Gestures {

  /// <summary>
  /// Abstraction class for a two-handed gesture. Two-handed gestures require two
  /// tracked hands to be active; if a hand loses tracking mid-gesture, the gesture will
  /// fire its deactivation method as a cancellation.
  /// 
  /// For gestures that only require one hand (and possibly, for example, an object),
  /// see OneHandedGesture.
  /// </summary>
  [ExecuteInEditMode]
  public abstract class TwoHandedGesture : Gesture {

    #region Public API

    public override bool isGestureActive { get { return _isGestureActive; } }

    public Hand leftHand { get { return _lHand; } }

    public Hand rightHand { get { return _rHand; } }

    public Action<Hand, Hand> OnTwoHandedGestureActivated
                              = (leftHand, rightHand) => { };

    public Action<Hand, Hand> OnTwoHandedGestureDeactivated
                              = (maybeNullLeftHand, maybeNullRightHand) => { };

    #endregion

    #region Implementer's API

    /// <summary>
    /// Returns whether the gesture should activate this frame.
    /// This method is called once per Update frame if the gesture is currently inactive.
    /// The hands are guaranteed to be non-null.
    /// </summary>
    protected abstract bool ShouldGestureActivate(Hand leftHand, Hand rightHand);

    /// <summary>
    /// A two-handed gesture will deactivate if this method returns true OR if one of
    /// the hands loses tracking mid-gesture. The method is called once per Update frame
    /// if the gesture is currently active.
    /// 
    /// The deactivationReason will be provided to WhenGestureDeactivated if this method
    /// returns true on a given frame. If a hand lost tracking mid-gesture, the reason
    /// for deactivation will be CancelledGesture.
    /// 
    /// The hands provided to this method are guaranteed to be non-null.
    /// </summary>
    protected abstract bool ShouldGestureDeactivate(Hand leftHand, Hand rightHand,
                                                    out DeactivationReason? deactivationReason);

    /* Gesture / Hand State
     * 
     * The overrideable methods provided below are thorough, but optional.
     * Depending on the nature of the gesture to be implemented, it is likely that
     * only a subset of the methods provided will be necessary to override.
     * 
     * Gesture State contains methods called when the activation state of the
     * gesture changes and methods called while the gesture is active or inactive
     * on a given frame.
     * 
     * Hand State contains methods called per-hand when the tracked state of the
     * hand changes or while the hand is tracked or untracked.
     */

    #region Gesture State

    /// <summary>
    /// Called when the gesture has just been activated. Hands are guaranteed to be
    /// non-null.
    /// </summary>
    protected virtual void WhenGestureActivated(Hand leftHand, Hand rightHand) { }

    /// <summary>
    /// Called when the gesture has just been deactivated. Hands might be null; this
    /// will be true if a hand loses tracking while the gesture is active.
    /// </summary>
    protected virtual void WhenGestureDeactivated(Hand maybeNullLeftHand, Hand maybeNullRightHand, DeactivationReason reason) { }

    /// <summary>
    /// Called every Update frame while the gesture is active. Hands are guaranteed to
    /// be non-null.
    /// </summary>
    protected virtual void WhileGestureActive(Hand leftHand, Hand rightHand) { }

    /// <summary>
    /// Called every Update frame while the gesture is inactive. The hands might be null;
    /// this will be the case if a hand loses tracking while the gesture is active.
    /// </summary>
    protected virtual void WhileGestureInactive(Hand maybeNullLeftHand, Hand maybeNullRightHand) { }

    #endregion

    #region Hand State

    /// <summary>
    /// Called for the left hand if the left hand has started tracking,
    /// or for the right hand if the right hand has started tracking.
    /// 
    /// Refer to hand.IsLeft to know whether the hand is the left hand
    /// or the right hand.
    /// </summary>
    protected virtual void WhenHandBecomesTracked(Hand hand) { }

    /// <summary>
    /// Called for the left hand if the left hand has lost tracking,
    /// or for the right hand if the right hand has lost tracking.
    /// </summary>
    protected virtual void WhenHandLosesTracking(bool wasLeftHand) { }

    /// <summary>
    /// Called every Update frame while both hands are tracked.
    /// </summary>
    protected virtual void WhileBothHandsTracked(Hand leftHand, Hand rightHand) { }

    /// <summary>
    /// Called every Update frame while a hand is tracked.
    /// 
    /// Refer to hand.IsLeft to know whether the hand is the left hand
    /// or the right hand.
    /// </summary>
    protected virtual void WhileHandTracked(Hand hand) { }

    /// <summary>
    /// Called every Update frame while the hand is not tracked;
    /// called twice if both hands are not tracked, once for the left,
    /// and once for the right (with isLeftHand set accordingly).
    /// </summary>
    protected virtual void WhileHandUntracked(bool wasLeftHand) { }

    #endregion

    #endregion

    #region Base Implementation (Unity Callbacks)

    private Hand _lHand;
    private Hand _rHand;

    private bool _isGestureActive = false;
    private bool _wasLeftTracked = false;
    private bool _wasRightTracked = false;

    protected virtual void OnDisable() {
      if (_isGestureActive) {
        WhenGestureDeactivated(_lHand, _rHand, DeactivationReason.CancelledGesture);
        _isGestureActive = false;
      }
    }

    protected virtual void Start() {
      if (Application.isPlaying) {
        initUnityEvents();

        var provider = Hands.Provider;
        if (provider != null) {
          provider.OnUpdateFrame += onUpdateFrame;
        }
      }
      else {
        #if UNITY_EDITOR
        refreshEditorHands();
        #endif
      }
    }

    protected virtual void Update() {
      #if UNITY_EDITOR
      refreshEditorHands();
      #endif
    }

#if UNITY_EDITOR
    private void refreshEditorHands() {
      _lHand = TestHandUtil.MakeTestHand(isLeft: true);
      _rHand = TestHandUtil.MakeTestHand(isLeft: false);
    }
#endif

    protected virtual void onUpdateFrame(Frame frame) {
      _lHand = frame.Hands.Query().FirstOrDefault(h =>  h.IsLeft);
      _rHand = frame.Hands.Query().FirstOrDefault(h => !h.IsLeft);

      // Determine the tracked state of hands and fire appropriate methods.
      bool isLeftTracked  = _lHand != null;
      bool isRightTracked = _rHand != null;

      if (isLeftTracked != _wasLeftTracked) {
        if (isLeftTracked) {
          WhenHandBecomesTracked(_lHand);
        }
        else {
          WhenHandLosesTracking(true);
        }
      }
      if (isRightTracked != _wasRightTracked) {
        if (isRightTracked) {
          WhenHandBecomesTracked(_rHand);
        }
        else {
          WhenHandLosesTracking(false);
        }
      }

      if (isLeftTracked) {
        WhileHandTracked(_lHand);
      }
      else {
        WhileHandUntracked(true);
      }
      if (isRightTracked) {
        WhileHandTracked(_rHand);
      }
      else {
        WhileHandUntracked(false);
      }

      if (isLeftTracked && isRightTracked) {
        WhileBothHandsTracked(_lHand, _rHand);
      }
      _wasLeftTracked  = isLeftTracked;
      _wasRightTracked = isRightTracked;

      // Determine whether or not the gesture should be active or inactive.
      bool shouldGestureBeActive;
      DeactivationReason? deactivationReason = null;
      if (_lHand == null || _rHand == null) {
        shouldGestureBeActive = false;
      }
      else {
        if (!_isGestureActive) {
          if (ShouldGestureActivate(_lHand, _rHand)) {
            shouldGestureBeActive = true;
          }
          else {
            shouldGestureBeActive = false;
          }
        }
        else {
          if (ShouldGestureDeactivate(_lHand, _rHand, out deactivationReason)) {
            shouldGestureBeActive = false;
          }
          else {
            shouldGestureBeActive = true;
          }
        }
      }

      // Fire gesture state change events.
      if (shouldGestureBeActive != _isGestureActive) {
        if (shouldGestureBeActive) {
          _isGestureActive = true;
          WhenGestureActivated(_lHand, _rHand);
          OnGestureActivated();
          OnTwoHandedGestureActivated(_lHand, _rHand);
        }
        else {
          _isGestureActive = false;
          WhenGestureDeactivated(_lHand, _rHand, deactivationReason.GetValueOrDefault());
          OnGestureDeactivated();
          OnTwoHandedGestureDeactivated(_lHand, _rHand);
        }
      }

      // Fire per-update events.
      if (_isGestureActive) {
        WhileGestureActive(_lHand, _rHand);
      }
      else {
        WhileGestureInactive(_lHand, _rHand);
      }

    }

    #region Unity Events

    [SerializeField]
    private EnumEventTable _eventTable;

    public enum EventType {
      GestureActivated = 100,
      GestureDeactivated = 110,
    }

    private void initUnityEvents() {
      setupCallback(ref OnGestureActivated, EventType.GestureActivated);
      setupCallback(ref OnGestureDeactivated, EventType.GestureDeactivated);
    }

    private void setupCallback(ref Action action, EventType type) {
      if (_eventTable.HasUnityEvent((int)type)) {
        action += () => _eventTable.Invoke((int)type);
      }
      else {
        action += () => { };
      }
    }


    private void setupCallback<T, U>(ref Action<T, U> action, EventType type) {
      if (_eventTable.HasUnityEvent((int)type)) {
        action += (lh, rh) => _eventTable.Invoke((int)type);
      }
      else {
        action += (lh, rh) => { };
      }
    }

    #endregion

    #endregion

  }

}
