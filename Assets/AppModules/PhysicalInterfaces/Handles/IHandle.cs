using Leap.Unity.Attributes;
using System;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public interface IHandle {
    
    Pose        pose          { get; }
    Movement    movement      { get; }
                              
    bool isHeld               { get; }
                              
    bool wasHeld              { get; }
    bool wasMoved             { get; }
    bool wasReleased          { get; }
    bool wasThrown            { get; }
    bool wasTeleported        { get; }
    
    void Hold();
    void Move(Pose newPose);
    void Release();
    void Throw();
    void Teleport(Pose newPose);

  }

  public static class IHandleExtensions {

    public static void Move(this IHandle handle, Vector3 newPosition) {
      handle.Move(new Pose(newPosition));
    }
    public static void Move(this IHandle handle, Quaternion newRotation) {
      handle.Move(new Pose(newRotation));
    }

  }

}