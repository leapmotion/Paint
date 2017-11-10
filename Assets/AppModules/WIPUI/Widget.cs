using Leap.Unity;
using Leap.Unity.Animation;
using Leap.Unity.Attributes;
using Leap.Unity.Layout;
using Leap.Unity.PhysicalInterfaces;
using UnityEngine;

namespace Leap.Unity.WIPUI {

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
    [Tooltip("This component handles how the widget moves to its target pose when placed or "
         + "thrown by the user.")]
    private MonoBehaviour _movementToPose;
    public IMoveToPose movementToPose {
      get { return _movementToPose as IMoveToPose; }
    }

    #endregion

    #region Unity Events

    void Start() {
      handle.OnPickedUp += onHandlePickedUp;
      handle.OnMoved += onHandleMoved;
      handle.OnPlaced += onHandlePlaced;
      handle.OnThrown += onHandleThrown;
      handle.OnPlacedInContainer += onHandlePlacedInContainer;

      movementToPose.OnMovementUpdate += onMovementUpdate;
      movementToPose.OnReachTarget += onPlacementTargetReached;
    }

    #endregion

    #region Handle Events

    private void onHandlePickedUp() {
      movementToPose.Cancel();

      if (stateController != null) {
        if (stateController.currentState.Equals(panelOpenState)) {
          stateController.SwitchTo(panelClosedState);
        }
      }
    }

    private void onHandleMoved() {
      // (no logic)
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
      var targetPose = targetPoseProvider.GetTargetPose();

      movementToPose.MoveToTarget(targetPose);
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
