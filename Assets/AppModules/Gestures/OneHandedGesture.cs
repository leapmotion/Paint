using Leap.Unity.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Gestures {

  /// <summary>
  /// Abstraction class for a one-handed gesture. One-handed gestures require
  /// a tracked hand to be active; if the hand loses tracking mid-gesture, the
  /// gesture will fire its deactivation method as a cancellation.
  /// 
  /// If either hand can perform the implementing gesture, create a Gesture instance for
  /// each hand. For gestures that require both hands or interactions between
  /// hands to perform, use a TwoHandedGesture.
  /// </summary>
  public abstract class OneHandedGesture : Gesture {

    /// <summary>
    /// Which hand does the gesture apply to? If either hand can perform the
    /// gesture, create an instance for each hand.
    /// </summary>
    [EditTimeOnly]
    public Chirality whichHand;

    #region Public API

    public override bool isGestureActive { get { return _isGestureActive; } }

    public Action<Hand> OnOneHandedGestureActivated
                              = (hand) => { };

    public Action<Hand> OnOneHandedGestureDeactivated
                              = (maybeNullHand) => { };

    #endregion

    #region Implementer's API

    /// <summary>
    /// Returns whether the gesture should activate this frame.
    /// This method is called once per Update frame if the gesture is currently inactive.
    /// The hand is guaranteed to be non-null.
    /// </summary>
    protected abstract bool ShouldGestureActivate(Hand hand);

    /// <summary>
    /// The gesture will deactivate if this method returns true OR if the hand loses tracking
    /// mid-gesture. The method is called once per Update frame if the gesture is currently
    /// active and the hand is tracked.
    /// 
    /// The deactivationReason will be provided to WhenGestureDeactivated if this method
    /// returns true on a given frame. If a hand lost tracking mid-gesture, the reason for
    /// deactivation will be CancelledGesture.
    /// 
    /// The hand provided to this method is guaranteed to be non-null.
    /// </summary>
    protected abstract bool ShouldGestureDeactivate(Hand hand,
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
     * Hand State contains methods called when the tracked state of the
     * hand changes or while the hand is tracked or untracked.
     */

    #region Gesture State

    /// <summary>
    /// Called when the gesture has just been activated. The hand is guaranteed to
    /// be non-null.
    /// </summary>
    protected virtual void WhenGestureActivated(Hand hand) { }

    /// <summary>
    /// Called when the gesture has just been deactivated. The hand might be null; this
    /// will be true if the hand loses tracking while the gesture is active.
    /// </summary>
    protected virtual void WhenGestureDeactivated(Hand maybeNullHand, DeactivationReason reason) { }

    /// <summary>
    /// Called every Update frame while the gesture is active. The hand is guaranteed to
    /// be non-null.
    /// </summary>
    protected virtual void WhileGestureActive(Hand hand) { }

    /// <summary>
    /// Called every Update frame while the gesture is inactive. the hand might be null;
    /// this will be the case if a hand loses tracking while the gesture is active.
    /// </summary>
    protected virtual void WhileGestureInactive(Hand maybeNullHand) { }

    #endregion

    #region Hand State

    /// <summary>
    /// Called when the hand this gesture pays attention to begins tracking.
    /// </summary>
    protected virtual void WhenHandBecomesTracked(Hand hand) { }

    /// <summary>
    /// Called when the hand this gesture pays attention to loses tracking.
    /// </summary>
    protected virtual void WhenHandLosesTracking(bool wasLeftHand) { }

    /// <summary>
    /// Called every Update frame while the hand this gesture pays attention to
    /// is tracked.
    /// 
    /// Refer to hand.IsLeft to know whether the hand is the left or right hand.
    /// </summary>
    protected virtual void WhileHandTracked(Hand hand) { }

    /// <summary>
    /// Called every Update frame while the hand this gesture pays attention to
    /// is NOT tracked.
    /// 
    /// The provided boolean indicates whether this gesture is paying attention to
    /// the left hand. (Otherwise, it is paying attention to the right hand.)
    /// </summary>
    protected virtual void WhileHandUntracked(bool isLeftHand) { }

    #endregion

    #endregion

    #region Base Implementation (Unity Callbacks)

    private bool _isGestureActive = false;
    private bool _wasHandTracked = false;

    // Whether the gesture was activated this frame.
    protected bool _wasGestureActivated = false;
    // Whether the gesture was deactivated this frame.
    protected bool _wasGestureDeactivated = false;

    protected virtual void OnDisable() {
      if (_isGestureActive) {
        var hand = Hands.Get(whichHand);
        WhenGestureDeactivated(hand, DeactivationReason.CancelledGesture);
        _isGestureActive = false;
      }
    }

    protected virtual void Update() {
      _wasGestureActivated = false;
      _wasGestureDeactivated = false;

      var hand = Hands.Get(whichHand);

      // Determine the tracked state of hands and fire appropriate methods.
      bool isHandTracked = hand != null;

      if (isHandTracked != _wasHandTracked) {
        if (isHandTracked) {
          WhenHandBecomesTracked(hand);
        }
        else {
          WhenHandLosesTracking(whichHand == Chirality.Left);
        }
      }

      if (isHandTracked) {
        WhileHandTracked(hand);
      }
      else {
        WhileHandUntracked(whichHand == Chirality.Left);
      }

      _wasHandTracked = isHandTracked;

      // Determine whether or not the gesture should be active or inactive.
      bool shouldGestureBeActive;
      DeactivationReason? deactivationReason = null;
      if (!isHandTracked) {
        shouldGestureBeActive = false;
      }
      else {
        if (!_isGestureActive) {
          if (ShouldGestureActivate(hand)) {
            shouldGestureBeActive = true;
          }
          else {
            shouldGestureBeActive = false;
          }
        }
        else {
          if (ShouldGestureDeactivate(hand, out deactivationReason)) {
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
          _wasGestureActivated = true;
          _isGestureActive = true;
          WhenGestureActivated(hand);
          OnGestureActivated();
          OnOneHandedGestureActivated(hand);
        }
        else {
          _wasGestureDeactivated = true;
          _isGestureActive = false;
          WhenGestureDeactivated(hand, deactivationReason.GetValueOrDefault());
          OnGestureDeactivated();
          OnOneHandedGestureDeactivated(hand);
        }
      }

      // Fire per-update events.
      if (_isGestureActive) {
        WhileGestureActive(hand);
      }
      else {
        WhileGestureInactive(hand);
      }
    }

    #endregion

  }

}