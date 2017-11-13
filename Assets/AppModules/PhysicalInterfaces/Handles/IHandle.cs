using Leap.Unity.Attributes;
using System;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public interface IHandle {
    
    Pose     pose     { get; }
    Movement movement { get; }
                              
    bool isHeld       { get; }
    bool wasHeld      { get; }
    void Hold();

    bool wasMoved     { get; }
    void Move(Pose newPose);

    bool wasReleased  { get; }
    void Release();

    bool wasThrown    { get; }

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