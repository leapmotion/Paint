using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class PoseStreamProcessor_RibbonFlight : MonoBehaviour,
                                                  IStreamReceiver<Pose>,
                                                  IStream<Pose> {



    public event Action OnOpen  = () => { };
    public event Action<Pose> OnSend = (pose) => { };
    public event Action OnClose = () => { };

    private RingBuffer<Pose> _buffer = new RingBuffer<Pose>(3);

    public void Open() {
      _buffer.Clear();

      OnOpen();
    }

    public void Receive(Pose data) {
      _buffer.Add(data);

      if (_buffer.Length == 2) {
        Pose a = _buffer.Get(0), b = _buffer.Get(1);
        var ab = b.position - a.position;

        var handDorsal = a.rotation * Vector3.up;

        Quaternion initRot;

        var initAngle = Vector3.Angle(handDorsal, ab);
        if (initAngle < 10f) {
          var handDistal = a.rotation * Vector3.forward;
          var ribbonRight = Vector3.Cross(ab, handDistal).normalized;
          var ribbonUp = Vector3.Cross(ribbonRight, ab).normalized;

          initRot = Quaternion.LookRotation(ab, ribbonUp);
        }
        else {
          var ribbonRight = Vector3.Cross(ab, handDorsal).normalized;
          var ribbonUp = Vector3.Cross(ribbonRight, ab).normalized;

          initRot = Quaternion.LookRotation(ab, ribbonUp);
        }

        var initPose = new Pose(a.position, initRot);
        _buffer.Set(0, initPose);
        OnSend(initPose);
      }

      if (_buffer.IsFull) {
        Pose a = _buffer.Get(0), b = _buffer.Get(1), c = _buffer.Get(2);

        var ab = b.position - a.position;
        var bc = c.position - b.position;

        ab = ab.normalized;
        bc = bc.normalized;

        var right = a.rotation * Vector3.right;
        var up = a.rotation * Vector3.up;
        var forward = a.rotation * Vector3.forward;

        var bc_pitchProjection = Vector3.ProjectOnPlane(bc, right);
        var pitchAngle = Vector3.SignedAngle(forward, bc_pitchProjection, right);

        var Q_right_theta = Quaternion.AngleAxis(pitchAngle, right);
        var newUp = Q_right_theta * up;
        var newForward = Q_right_theta * forward;
        
        var yawAngle = Vector3.SignedAngle(newForward, bc, newUp);
        var Q_reachBC = Quaternion.AngleAxis(yawAngle, newUp);

        var rolllessRot = Q_reachBC * Q_right_theta * a.rotation;

        var midPose = new Pose(b.position,
                               Quaternion.Slerp(a.rotation, rolllessRot, 0.5f));

        _buffer.Set(1, midPose);
        OnSend(midPose);
      }
    }

    public void Close() {

      OnClose();
    }

  }

}
