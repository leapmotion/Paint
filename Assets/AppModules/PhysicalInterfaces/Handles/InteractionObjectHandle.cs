using Leap.Unity.Interaction;
using Leap.Unity.PhysicalInterfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Leap.Unity.Attributes;
using UnityEngine.EventSystems;

namespace Leap.Unity.PhysicalInterfaces {

  public class InteractionObjectHandle : HandleBase {

    #region Inspector

    [SerializeField, OnEditorChange("intObj")]
    private InteractionBehaviour _intObj;
    public InteractionBehaviour intObj {
      get { return _intObj; }
      set {
        if (_intObj != value) {
          if (isHeld) Release();

          if (_intObj != null && Application.isPlaying) {
            unsubscribeIntObjCallbacks();
          }

          if (value != null && Application.isPlaying) {
            subscribeIntObjCallbacks();
          }

          _intObj = value;
        }
      }
    }
    

    #endregion

    #region Unity Events

    protected virtual void Reset() {
      initInspector();
    }

    protected virtual void OnValidate() {
      initInspector();
    }

    protected virtual void Awake() {
      initInspector();
    }

    private void initInspector() {
      if (intObj == null) intObj = GetComponent<InteractionBehaviour>();

      subscribeIntObjCallbacks();
    }

    #endregion

    #region Interaction Object Handle

    public override Pose pose {
      get { return intObj.transform.ToPose(); }
      protected set {
        if (gameObject.activeInHierarchy) {
          intObj.rigidbody.MovePosition(value.position);
          intObj.rigidbody.MoveRotation(value.rotation);
        }
        else {
          intObj.transform.SetPose(value);
        }
      }
    }

    private void unsubscribeIntObjCallbacks() {
      intObj.OnGraspBegin -= onGraspBegin;
      intObj.OnGraspEnd -= onGraspEnd;

      intObj.OnGraspedMovement -= onGraspedMovement;
    }

    private void subscribeIntObjCallbacks() {
      intObj.OnGraspBegin += onGraspBegin;
      intObj.OnGraspEnd += onGraspEnd;

      intObj.OnGraspedMovement += onGraspedMovement;
    }

    private void onGraspBegin() {
      if (!isHeld) {
        Hold();
      }
    }

    private void onGraspEnd() {
      if (isHeld) {
        Release();
      }
    }

    public override void Release() {
      base.Release();

      if (intObj.isGrasped) {
        intObj.ReleaseFromGrasp();
      }
    }

    private void onGraspedMovement(Vector3 oldPosition, Quaternion oldRotation,
                                   Vector3 newPosition, Quaternion newRotation,
                                   List<InteractionController> graspingControllers) {

      var newPose = new Pose() {
        position = newPosition,
        rotation = newRotation
      };

      targetPose = newPose;
    }

    #endregion

  }

}
