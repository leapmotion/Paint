using Leap.Unity.Interaction;
using Leap.Unity.Query;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  using IntObj = InteractionBehaviour;

  public class HeldPivotLook : MonoBehaviour {

    public IntObj intObj;

    public Transform lookFrom;
    public Transform lookTarget;

    public bool flip180;

    private Pose _handleToPanelPose;

    void Start() {
      intObj.OnGraspedMovement -= onGraspedMovement;
      intObj.OnGraspedMovement += onGraspedMovement;

      _handleToPanelPose = lookFrom.ToPose().From(intObj.transform.ToPose());
    }

    void Reset() {
      if (intObj == null) intObj = GetComponent<IntObj>();
    }

    private void onGraspedMovement(Vector3 oldPos, Quaternion oldRot,
                                   Vector3 newPos, Quaternion newRot,
                                   List<InteractionController> controllers) {

      // An important reason this works is because even though the various rigidbodies
      // have constantly-fluctuated poses with respect to one another, there are rigid
      // "ideal" poses and relative poses stored on Start() that allow target positions
      // to always be rigidly calculatable.

      // Introduce any sort of dynamic "relative pose" calculation INSIDE this method,
      // and you'll see a ton of erratic behaviour and terrible instability.

      var newPose = new Pose(newPos, newRot);

      var newPanelPose = newPose.Then(_handleToPanelPose);

      var graspingCentroid = controllers.Query()
                                        .Select(c => c.GetGraspPoint())
                                        .Fold((acc, p) => p + acc)
                             / controllers.Count;

      var targetLookFromPose = PivotLook.Solve(newPanelPose,
                                               graspingCentroid,
                                               lookTarget.position,
                                               Vector3.up,
                                               flip180: flip180);
      var targetHandlePose = targetLookFromPose.Then(_handleToPanelPose.inverse);

      intObj.rigidbody.MovePosition(targetHandlePose.position);
      intObj.rigidbody.MoveRotation(targetHandlePose.rotation);
      intObj.rigidbody.position = targetHandlePose.position;
      intObj.rigidbody.rotation = targetHandlePose.rotation;
    }

  }

}
