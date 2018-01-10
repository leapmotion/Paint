using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Gestures {

  public class DevCommandGesture : TwoHandedHeldGesture {

    public override bool IsGesturePoseHeld(Hand leftHand, Hand rightHand,
                                          out Vector3 positionOfInterest) {

      var leftThumb = leftHand.GetThumb();
      var leftThumbTip = leftThumb.TipPosition.ToVector3();
      var leftThumbDir = leftThumb.Direction.ToVector3();

      var rightIndex = rightHand.GetIndex();
      var rightIndexTip = rightIndex.TipPosition.ToVector3();
      var rightIndexDir = rightIndex.Direction.ToVector3();

      var tipsTouching = (leftThumbTip - rightIndexTip).sqrMagnitude
                         < MAX_TOUCHING_DISTANCE_SQR;
      if (drawHeldPoseDebug) {
        var touchingAmount = ((leftThumbTip - rightIndexTip).sqrMagnitude
                             - MAX_TOUCHING_DISTANCE_SQR).Map(0, -MAX_TOUCHING_DISTANCE_SQR, 0, 1);
        RuntimeGizmos.BarGizmo.Render(touchingAmount,
                                      Vector3.down * 0.2f + Vector3.right * 0.20f
                                        + Vector3.forward * 0.1f,
                                      Vector3.up,
                                      tipsTouching ?
                                        LeapColor.white
                                      : LeapColor.teal,
                                      scale: 0.2f);
      }

      var tipsAligned = Vector3.Dot(leftThumbDir, rightIndexDir) < -0.70f;
      if (drawHeldPoseDebug) {
        RuntimeGizmos.BarGizmo.Render(Vector3.Dot(leftThumbDir, rightIndexDir)
                                        .Map(-1, 1, 1, 0),
                                      Vector3.down * 0.2f + Vector3.right * 0.20f,
                                      Vector3.up,
                                      tipsAligned ?
                                        LeapColor.white
                                      : LeapColor.periwinkle,
                                      scale: 0.2f);
      }

      var leftIndexPointAmount = leftHand.GetIndexPointAmount();
      var rightIndexPointAmount = rightHand.GetIndexPointAmount();

      var leftIsIndexPointing = leftIndexPointAmount > 0.80f;
      var rightIsIndexPointing = rightIndexPointAmount > 0.80f;
      
      if (drawHeldPoseDebug) {
        RuntimeGizmos.BarGizmo.Render(leftIndexPointAmount,
                                      Vector3.down * 0.2f, Vector3.up,
                                      leftIsIndexPointing ?
                                        LeapColor.white
                                      : LeapColor.lavender,
                                      scale: 0.2f);
        RuntimeGizmos.BarGizmo.Render(rightIndexPointAmount,
                                      Vector3.down * 0.2f + Vector3.right * 0.10f,
                                      Vector3.up,
                                      rightIsIndexPointing ?
                                        LeapColor.white
                                      : LeapColor.red,
                                      scale: 0.2f);
      }

      positionOfInterest = Vector3.zero;
      bool isGesturePoseHeld = tipsTouching
                            && tipsAligned
                            && leftIsIndexPointing
                            && rightIsIndexPointing;
      if (isGesturePoseHeld) {

        var gesturePlaneDir = Vector3.Cross(
          leftHand.Fingers[0].Direction.ToVector3(),
          leftHand.Fingers[1].Direction.ToVector3());
        var upPlaneDir = Vector3.Cross(gesturePlaneDir,
          leftHand.Fingers[0].Direction.ToVector3()).normalized;

        positionOfInterest = (leftThumbTip + rightIndexTip) / 2f
                        + upPlaneDir * (leftHand.GetIndex().bones[1].PrevJoint
                                        - leftHand.GetIndex().TipPosition)
                                       .ToVector3().magnitude;
      }

      return isGesturePoseHeld;
    }

  }

}
