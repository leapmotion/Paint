using Leap.Unity.Attributes;
using Leap.Unity.Layout;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class ThrownUIPoseProvider : MonoBehaviour, IPoseProvider {

    [SerializeField, ImplementsInterface(typeof(IKinematicStateProvider))]
    private MonoBehaviour _handleKinematicStateProvider;
    public IKinematicStateProvider handleKinematicStateProvider {
      get {
        return _handleKinematicStateProvider as IKinematicStateProvider;
      }
    }

    [SerializeField, ImplementsInterface(typeof(IAttachmentProvider))]
    private MonoBehaviour _handleAttachmentPoseProvider;
    public IAttachmentProvider handleAttachmentPoseProvider {
      get {
        return _handleAttachmentPoseProvider as IAttachmentProvider;
      }
    }

    public bool flip180 = false;

    public Pose GetPose() {
      var handleKinematicState = handleKinematicStateProvider.GetKinematicState();

      var handlePose = handleKinematicState.pose;

      var layoutPos = LayoutUtils.LayoutThrownUIPosition2(Camera.main.transform.ToPose(),
                                                     handlePose.position,
                                                     handleKinematicState.movement.velocity,
                                                     optimalDistanceMultiplier: 1f);

      var solvedHandlePose =
        new Pose(layoutPos,
                 Utils.FaceTargetWithoutTwist(layoutPos,
                                              Camera.main.transform.position,
                                              flip180))
            .Then(handleAttachmentPoseProvider.GetHandleToAttachmentPose().inverse);

      return solvedHandlePose;
    }

  }

}
