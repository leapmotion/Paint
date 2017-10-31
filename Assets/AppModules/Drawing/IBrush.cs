using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Drawing {

  public interface IBrush {

    bool isBrushing { get; }
    
    Pose currentPose { get; }

    void Move(Pose newPose);

    void Begin();

    void End();

  }

}
