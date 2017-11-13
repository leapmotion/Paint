using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class TransformHandle : HandleBase {

    public override Pose pose {
      get { return transform.ToPose(); }
    }

  }

}
