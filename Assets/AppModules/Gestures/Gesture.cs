using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Gestures {
  
  /// <summary>
  /// A (very thin!) layer of general abstraction for one-handed and two-handed
  /// gestures.
  /// </summary>
  public abstract class Gesture : MonoBehaviour {

    public abstract bool isGestureActive { get; }

    public Action OnGestureActivated = () => { };
    public Action OnGestureDeactivated = () => { };

    protected enum DeactivationReason {
      FinishedGesture,
      CancelledGesture,
    }

  }

}