using System.Collections;
using System.Collections.Generic;
using Leap.Unity.PhysicalInterfaces;
using UnityEngine;
using Leap.Unity.Attributes;

namespace Leap.Unity.Layout {

  public class UIPoseProvider : MonoBehaviour,
                                IPoseProvider {

    #region Inspector

    [SerializeField, ImplementsInterface(typeof(IHandle))]
    private MonoBehaviour _uiAnchorHandle;
    public IHandle uiAnchorHandle {
      get {
        return _uiAnchorHandle as IHandle;
      }
    }

    [SerializeField, ImplementsInterface(typeof(IWorldPositionProvider))]
    private MonoBehaviour _uiLookAnchor;
    public  IWorldPositionProvider uiLookAnchor {
      get {
        return _uiLookAnchor as IWorldPositionProvider;
      }
    }

    public bool flip180 = false;

    [Header("Runtime Gizmo Debugging")]
    public bool drawDebug = false;

    #endregion

    public Vector3 GetTargetPosition() {
      Vector3 layoutPos;

      if (uiAnchorHandle.movement.velocity.magnitude
            <= PhysicalInterfaceUtils.MIN_THROW_SPEED) {

        layoutPos = uiAnchorHandle.pose.position;

        if (drawDebug) {
          DebugPing.Ping(layoutPos, Color.white);
        }
      }
      else {
        // When the UI is thrown, utilize the static thrown UI util to calculate a decent
        // final position relative to the user's head given the position and velocity of
        // the throw.
        layoutPos = LayoutUtils.LayoutThrownUIPosition2(Camera.main.transform.ToWorldPose(),
                                                       uiAnchorHandle.pose.position,
                                                       uiAnchorHandle.movement.velocity);

        // However, UIs whose central "look" anchor is in a different position than their
        // grabbed/thrown anchor shouldn't be placed directly at the determined position.
        // Rather, we need to adjust this position so that the _look anchor,_ not the
        // thrown handle, winds up in the calculated position from the throw.

        // Start with the "final" pose as it would currently be calculated.
        // We need to know the target rotation of the UI based on the target position in
        // order to adjust the final position properly.
        Pose finalUIPose = new Pose(layoutPos, GetTargetRotationForPosition(layoutPos));

        // We assume the uiAnchorHandle and the uiLookAnchor are rigidly connected.
        Vector3 curHandleToLookAnchorOffset = (uiLookAnchor.GetTargetWorldPosition()
                                               - uiAnchorHandle.pose.position);

        // We undo the current rotation of the UI handle and apply that rotation
        // on the current world-space offset between the handle and the look anchor.
        // Then we apply the final rotation of the UI to this unrotated offset vector,
        // giving us the expected final offset between the position that was calculated
        // by the layout function and the 
        Vector3 finalRotatedLookAnchorOffset =
          finalUIPose.rotation
            * (Quaternion.Inverse(uiAnchorHandle.pose.rotation)
               * curHandleToLookAnchorOffset);

        // We adjust the layout position by this offset, so now the UI should wind up
        // with its lookAnchor at the calculated location instead of the handle.
        layoutPos = layoutPos - finalRotatedLookAnchorOffset;

        // We also adjust any interface positions down a bit.
        layoutPos += (Camera.main.transform.parent != null ?
                      -Camera.main.transform.parent.up
                      : Vector3.down) * 0.19f;

        if (drawDebug) {
          DebugPing.Ping(layoutPos, Color.red);
        }
      }

      return layoutPos;
    }

    public Quaternion GetTargetRotation() {
      return GetTargetRotationForPosition(uiLookAnchor.GetTargetWorldPosition());
    }

    private Quaternion GetTargetRotationForPosition(Vector3 worldPosition) {
      return NewUtils.FaceTargetWithoutTwist(worldPosition,
                                             Camera.main.transform.position,
                                             flip180: flip180);
    }

  }

}
