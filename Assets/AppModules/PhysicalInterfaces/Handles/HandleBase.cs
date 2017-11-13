using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public abstract class HandleBase : MovementObservingBehaviour, IHandle {

    protected override void OnEnable() {
      base.OnEnable();

      _lastPose = this.pose;
    }

    protected override void Update() {
      base.Update();

      updateHeldState();
      updateMovedState();
      updateReleaseState();
      updateThrownState();
    }

    private bool _isHeld = false;
    public bool isHeld {
      get { return _isHeld; }
    }

    private bool _wasHeld = false;
    private bool _sawWasHeld = false;
    public bool wasHeld {
      get { return _wasHeld && _sawWasHeld; }
    }

    public void Hold() {
      _isHeld = true;

      _wasHeld = true;
    }

    private void updateHeldState() {
      if (_wasReleased) {
        // The release signal cancels out any hold signal.
        _wasHeld = false;
        _sawWasHeld = false;
      }

      if (_wasHeld && !_sawWasHeld) {
        _sawWasHeld = true;
      }
      else if (_wasHeld && _sawWasHeld) {
        _wasHeld = false;
        _sawWasHeld = false;
      }
    }

    private bool _wasMoved = false;
    public bool wasMoved {
      get {
        return _wasMoved;
      }
    }

    private Pose _lastPose = Pose.identity;
    private void updateMovedState() {
      if (_wasMoved) {
        _wasMoved = false;
      }
      if (!this.pose.ApproxEquals(_lastPose)) {
        _wasMoved = true;

        _lastPose = this.pose;
      }
    }

    public virtual void Move(Pose newPose) {
      transform.SetWorldPose(newPose);
    }

    private bool _wasReleased = false;
    private bool _sawWasReleased = false;
    public bool wasReleased {
      get { return _wasReleased && _sawWasReleased; }
    }

    public virtual void Release() {
      _isHeld = false;
      _wasReleased = true;

      if (movement.velocity.sqrMagnitude > PhysicalInterfaceUtils.MIN_THROW_SPEED_SQR) {
        _wasThrown = true;
      }
    }

    private void updateReleaseState() {
      if (_wasReleased && !_sawWasReleased) {
        _sawWasReleased = true;
      }
      else if (_wasReleased && _sawWasReleased) {
        _wasReleased = false;
        _sawWasReleased = false;
      }
    }

    private bool _wasThrown = false;
    private bool _sawWasThrown = true;
    public bool wasThrown {
      get {
        return _wasThrown && _sawWasThrown;
      }
    }

    private void updateThrownState() {
      if (_wasThrown && !_sawWasThrown) {
        _sawWasThrown = true;
      }
      else if (_wasThrown && _sawWasThrown) {
        _wasThrown = false;
        _sawWasThrown = false;
      }
    }

  }

}
