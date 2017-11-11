using Leap.Unity;
using Leap.Unity.Animation;
using Leap.Unity.Attributes;
using Leap.Unity.Layout;
using Leap.Unity.PhysicalInterfaces;
using UnityEngine;

namespace Leap.Unity.WIPUI {

  public enum HeldOrientabilityType { LockedFacing, SingleAxis, Free }

  public enum HeldVisiblityType { Hide, StayOpen }

  public class Widget : MonoBehaviour {

    #region Inspector

    [Header("Widget Handle")]

    [SerializeField]
    [ImplementsInterface(typeof(IHandle))]
    [Tooltip("This is what the user grabs and places to open a panel.")]
    private MonoBehaviour _handle;
    public IHandle handle {
      get {
        return _handle as IHandle;
      }
    }

    [Header("State Changes")]

    [Tooltip("This controller opens and closes the panel.")]
    public SwitchTreeController stateController;

    [Tooltip("Switch to this state when the widget panel should be closed.")]
    public string panelClosedState = "Widget Sphere";
    [Tooltip("Switch to this state when the widget panel should be open.")]
    public string panelOpenState   = "Panel Pivot";

    // Not "general" but only utilized at edit-time currently.
    // TODO: Make the Handled Object work at edit-time? hrmm....
    [QuickButton("Move Here", "MoveToBall")]
    public Transform ballTransform;
    [QuickButton("Move Here", "MoveToPanel")]
    public Transform panelTransform;

    [Header("Widget Placement")]

    [SerializeField]
    [ImplementsInterface(typeof(IPoseProvider))]
    [Tooltip("This component handles where the widget determines its target pose when "
         + "placed or thrown by the user.")]
    private MonoBehaviour _targetPoseProvider;
    public IPoseProvider targetPoseProvider {
      get { return _targetPoseProvider as IPoseProvider; }
    }

    [SerializeField]
    [ImplementsInterface(typeof(IMoveToPose))]
    [OnEditorChange("movementToPose")]
    [Tooltip("This component handles how the widget moves to its target pose when placed or "
         + "thrown by the user.")]
    private MonoBehaviour _movementToPose;
    public IMoveToPose movementToPose {
      get { return _movementToPose as IMoveToPose; }
      set {
        if (Application.isPlaying) {
          if (_movementToPose != null) {
            movementToPose.OnMovementUpdate -= onMovementUpdate;
            movementToPose.OnReachTarget -= onPlacementTargetReached;
          }
          _movementToPose = value as MonoBehaviour;

          movementToPose.OnMovementUpdate += onMovementUpdate;
          movementToPose.OnReachTarget += onPlacementTargetReached;
        }
      }
    }
    
    [Header("Held Orientability")]

    [SerializeField, OnEditorChange("heldOrientability")]
    private HeldOrientabilityType _heldOrientability;
    public HeldOrientabilityType heldOrientability {
      get { return _heldOrientability; }
      set {
        _heldOrientability = value;
      }
    }

    #endregion

    #region Unity Events

    void Start() {
      handle.OnPickedUp += onHandlePickedUp;
      handle.OnMovedHandle += onMovedHandle;
      handle.OnPlaced += onHandlePlaced;
      handle.OnThrown += onHandleThrown;
      handle.OnPlacedInContainer += onHandlePlacedInContainer;

      if (movementToPose != null) {
        movementToPose.OnMovementUpdate -= onMovementUpdate;
        movementToPose.OnReachTarget    -= onPlacementTargetReached;

        movementToPose.OnMovementUpdate += onMovementUpdate;
        movementToPose.OnReachTarget    += onPlacementTargetReached;
      }
    }

    #endregion

    #region Handle Events

    private void onHandlePickedUp() {
      if (movementToPose != null) {
        movementToPose.Cancel();
      }

      if (stateController != null) {
        if (stateController.currentState.Equals(panelOpenState)) {
          stateController.SwitchTo(panelClosedState);
        }
      }
    }

    private void onMovedHandle(IHandle handleMoved, Pose oldPose, Pose newPose) {

      var sqrDist = (handle.pose.position - newPose.position).sqrMagnitude;

      float angle = Quaternion.Angle(handle.pose.rotation, newPose.rotation);

      switch (heldOrientability) {
        case HeldOrientabilityType.Free:

          handle.SetPose(new Pose(Vector3.Lerp(handle.pose.position, newPose.position,
                                   sqrDist.Map(0.00001f, 0.0004f, 0.1f, 0.8f)),
                                  Quaternion.Slerp(handle.pose.rotation,
                                    newPose.rotation,
                                    angle.Map(0.3f, 4f, 0.01f, 0.8f))));


          break;
        case HeldOrientabilityType.LockedFacing:

          var targetRotation = targetPoseProvider.GetTargetRotation();

          handle.SetPose(new Pose(Vector3.Lerp(handle.pose.position, newPose.position,
                                   sqrDist.Map(0.00001f, 0.0004f, 0.1f, 0.8f)),
                                  Quaternion.Slerp(handle.pose.rotation, targetRotation, 0.1f)));

          break;
        case HeldOrientabilityType.SingleAxis:
          break;
      }
    }

    private void onHandlePlaced() {
      initiateMovementToTarget();
    }

    private void onHandleThrown(Vector3 velocity) {
      initiateMovementToTarget();
    }

    private void onHandlePlacedInContainer() {
      // (no logic)
    }

    #endregion

    private void initiateMovementToTarget() {
      if (targetPoseProvider != null) {
        var targetPose = targetPoseProvider.GetTargetPose();

        if (movementToPose != null) {
          movementToPose.MoveToTarget(targetPose);
        }
      }
    }

    private void onMovementUpdate() {
      movementToPose.targetPose = new Pose() {
        position = movementToPose.targetPose.position,
        rotation = targetPoseProvider.GetTargetRotation()
      };
    }

    private void onPlacementTargetReached() {
      if (stateController != null) {
        stateController.SwitchTo(panelOpenState);
      }
    }

    #region Move To Child // TODO: Needs generalization

    public void MoveToBall() {
      MoveTo(ballTransform);
    }

    public void MoveToPanel() {
      MoveTo(panelTransform);
    }

    public void MoveTo(Transform t) {
      Pose followingPose = t.ToWorldPose();

      this.transform.SetWorldPose(followingPose);

      t.SetWorldPose(followingPose);
    }

    #endregion

  }

}
