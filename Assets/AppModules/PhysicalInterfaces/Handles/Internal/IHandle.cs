using Leap.Unity.Attributes;
using System;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public interface IHandle {
    
    Pose pose           { get; }
    Pose targetPose     { get; set; }

    Movement movement   { get; }
                                
    bool isHeld         { get; }
    bool wasHeld        { get; }
    void Hold();        
                        
    bool wasMoved       { get; }

    bool wasReleased    { get; }
    void Release();     
                        
    bool wasThrown      { get; }

  }

}