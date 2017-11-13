using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class TransformHandle : MonoBehaviour {

    private DeltaBuffer           _deltaPosBuffer = new DeltaBuffer(5);
    private DeltaQuaternionBuffer _deltaRotBuffer = new DeltaQuaternionBuffer(5);

    protected virtual void OnEnable() {
      _deltaPosBuffer.Clear();
      _deltaRotBuffer.Clear();

      _lastPose = this.pose;
    }

    private void Update() {
      var time = Time.time;
      var curPose = pose;
      _deltaPosBuffer.Add(curPose.position, time);
      _deltaRotBuffer.Add(curPose.rotation, time);

      _movement = new Movement(_deltaPosBuffer.Delta(), _deltaRotBuffer.Delta());

      updateHeldState();
      updateMovedState();
      updateReleaseState();
      updateThrownState();
    }

    public Pose pose {
      get { return transform.ToPose(); }
    }

    private Movement _movement = Movement.identity;
    public Movement movement {
      get { return _movement; }
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
      if (_wasHeld && !_sawWasHeld) {
        _sawWasHeld = true;
      }
      else if (_wasHeld && _sawWasHeld) {
        _wasHeld = false;
        _sawWasHeld = false;
      }
    }

    private bool _wasMoved = false;
    private bool _sawWasMoved = false;
    public bool wasMoved {
      get {
        return _wasMoved;
      }
    }

    private Pose _lastPose = Pose.identity;
    private void updateMovedState() {
      if (!this.pose.ApproxEquals(_lastPose)) {
        _wasMoved = true;
      }

      if (_wasMoved && !_sawWasMoved) {
        _sawWasMoved = true;
      }
      else if (_wasMoved && _sawWasMoved) {
        _wasMoved = false;
        _sawWasMoved = false;
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

    public void Release() {
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
