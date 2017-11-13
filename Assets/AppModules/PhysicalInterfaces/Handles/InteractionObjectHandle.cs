using Leap.Unity.Interaction;
using Leap.Unity.PhysicalInterfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Leap.Unity.Attributes;
using UnityEngine.EventSystems;

namespace Leap.Unity.PhysicalInterfaces {

  public class InteractionObjectHandle : TransformHandle {

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

    private void initInspector() {
      if (intObj == null) intObj = GetComponent<InteractionBehaviour>();

      subscribeIntObjCallbacks();
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

    #endregion

    #region Interaction Object Handle

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
      Hold();
    }

    private void onGraspEnd() {
      Release();
    }

    private void onGraspedMovement(Vector3 oldPosition, Quaternion oldRotation,
                                   Vector3 newPosition, Quaternion newRotation,
                                   List<InteractionController> graspingControllers) {
      var oldPose = new Pose() {
        position = oldPosition,
        rotation = oldRotation
      };
      var newPose = new Pose() {
        position = newPosition,
        rotation = newRotation
      };
      filterGraspedMovement(oldPose, newPose);
    }

    private void filterGraspedMovement(Pose oldPose, Pose newPose) {
      var sqrDist = (oldPose.position - newPose.position).sqrMagnitude;
      float angle = Quaternion.Angle(oldPose.rotation, newPose.rotation);

      var smoothedPose = new Pose(Vector3.Lerp(oldPose.position, newPose.position,
                                   sqrDist.Map(0.00001f, 0.0004f, 0.2f, 0.8f)),
                                  Quaternion.Slerp(oldPose.rotation,
                                    newPose.rotation,
                                    angle.Map(0.3f, 4f, 0.01f, 0.8f)));

      Move(smoothedPose);
    }

    public override void Move(Pose newPose) {
      intObj.rigidbody.MovePosition(newPose.position);
      intObj.rigidbody.MoveRotation(newPose.rotation);
    }

    #endregion

  }

}
