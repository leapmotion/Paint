using Leap.Unity.Interaction;
using Leap.Unity.PhysicalInterfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Leap.Unity.Attributes;

namespace Leap.Unity.PhysicalInterfaces {

  public class InteractionObjectHandle : MonoBehaviour,
                                         IHandle {

    #region Inspector

    public InteractionBehaviour intObj;

    void Reset() {
      if (intObj == null) intObj = GetComponent<InteractionBehaviour>();
    }

    #endregion

    #region Unity Events

    private DeltaBuffer _deltaPosBuffer = new DeltaBuffer(5);
    private DeltaQuaternionBuffer _deltaRotBuffer = new DeltaQuaternionBuffer(5);

    void OnEnable() {
      _deltaPosBuffer.Clear();
      _deltaRotBuffer.Clear();
    }

    void Update() {

    }

    #endregion

    #region IHandle

    public Pose pose { get { return intObj.transform.ToPose(); } }

    public Movement movement {

    }

    public bool isHeld => throw new NotImplementedException();

    public bool wasHeld => throw new NotImplementedException();

    public bool wasMoved => throw new NotImplementedException();

    public bool wasReleased => throw new NotImplementedException();

    public bool wasThrown => throw new NotImplementedException();

    public bool wasTeleported => throw new NotImplementedException();

    public void Hold() {
      throw new NotImplementedException();
    }

    public void Move(Pose newPose) {
      throw new NotImplementedException();
    }

    public void Release() {
      throw new NotImplementedException();
    }

    public void Teleport(Pose newPose) {
      throw new NotImplementedException();
    }

    public void Throw() {
      throw new NotImplementedException();
    }

    #endregion

  }

}
