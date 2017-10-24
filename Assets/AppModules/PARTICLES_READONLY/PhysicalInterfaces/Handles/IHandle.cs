using Leap.Unity.Attributes;
using System;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  /// <summary>
  /// A physical interface handle is an object that can be picked up, moved around,
  /// and placed.
  /// 
  /// The handle interface distinguishes between the handle's own position and rotation
  /// and the current position at which the user is holding the handle. It also 
  /// distinguishes between whether the user throws the handle (OnThrown), places it in
  /// free space (OnPlaced), or places it in some container (OnPlacedInContainer).
  /// 
  /// (Not every implementation needs to distinguish placement with this granularity, in
  /// which case, OnPlaced is the default placement event to expect.)
  /// </summary>
  public interface IHandle {

    /// <summary>
    /// The current position and rotation of this handle in world space.
    /// </summary>
    Pose pose { get; }

    /// <summary>
    /// Sets the world-space pose of this handle.
    /// </summary>
    void SetPose(Pose pose);

    /// <summary>
    /// The current velocity and angular velocity (as an angle-axis vector) of this
    /// handle in world space. These should be in standard units of m/s and radians/s.
    /// </summary>
    Movement movement { get; }

    /// <summary>
    /// The change in position and the change in rotation (as an angle-axis vector) of
    /// this handle in world space between the current render frame and the last render
    /// frame.
    /// 
    /// These values should not be divided by frame-time and are useful for manipulating
    /// objects attached to the handles without any loss of precision.
    /// </summary>
    Pose deltaPose { get; }

    /// <summary>
    /// Whether or not this handle is currently being held by the user.
    /// </summary>
    bool isHeld { get; }

    /// <summary>
    /// The world-space point at which the handle is currently held, if the handle is
    /// currently being held.
    /// </summary>
    Vector3 heldPosition { get; }

    /// <summary> Fired when the user has picked up this handle. </summary>
    event Action OnPickedUp;

    /// <summary>
    /// Fired when the user has picked up this handle, and passes itself as the event
    /// argument.
    /// </summary>
    event Action<IHandle> OnPickedUpHandle;


    /// <summary> Fired as the user is moving this handle. </summary>
    event Action OnMoved;

    /// <summary>
    /// Fired when the user is moving this handle, and passes itself and where it moved
    /// as event arguments.
    /// </summary>
    event Action<IHandle, Pose> OnMovedHandle;


    /// <summary>
    /// Fired when the user has placed this handle without much velocity. This is also
    /// the default event to expect if throwing the handle or placing the handle inside a
    /// container are not relevant actions for this handle.
    /// 
    /// If this event is fired, OnPlacedInContainer and OnThrown are not fired for this
    /// user-placement action.
    /// </summary>
    event Action OnPlaced;

    /// <summary>
    /// As OnPlaced, but also includes the handle itself as an event argument.
    /// </summary>
    event Action<IHandle> OnPlacedHandle;


    /// <summary>
    /// Fired when the user has placed this handle inside an handle container, such as an
    /// inventory slot or an anchor.
    /// 
    /// If this event is fired, OnPlaced and OnThrown are not fired for this user-
    /// placement action.
    /// </summary>
    event Action OnPlacedInContainer;

    /// <summary>
    /// As OnPlacedInContainer, but also includes the handle itself as an event argument.
    /// </summary>
    event Action<IHandle> OnPlacedHandleInContainer;
    

    /// <summary>
    /// Fired when the user throws this handle. The provided vector represents the throw
    /// velocity of the handle.
    /// 
    /// If this event is fired, OnPlaced and OnPlacedInContainer are not fired for this
    /// user-placement action.
    /// </summary>
    event Action<Vector3> OnThrown;

    /// <summary>
    /// As OnThrown, but also includes the handle itself as an event argument.
    /// </summary>
    event Action<IHandle, Vector3> OnThrownHandle;

  }
  
  [System.Serializable]
  public struct SerializeableHandle {
    [SerializeField]
    [ImplementsInterface(typeof(IHandle))]
    private MonoBehaviour _handle;
    public IHandle handle {
      get { return _handle as IHandle; }
    }
  }

  public static class IHandleExtensions {

    public static HandledObject GetHandledObject(this IHandle handle) {
      var handleBehaviour = handle as MonoBehaviour;
      if (handleBehaviour != null) {
        return handleBehaviour.GetComponentInParent<HandledObject>();
      }
      return null;
    }

  }

}