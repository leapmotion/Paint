using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public abstract class MovementObservingBehaviour : MonoBehaviour {

    private DeltaBuffer           _deltaPosBuffer = new DeltaBuffer(5);
    private DeltaQuaternionBuffer _deltaRotBuffer = new DeltaQuaternionBuffer(5);

    protected virtual void OnEnable() {
      _deltaPosBuffer.Clear();
      _deltaRotBuffer.Clear();
    }

    protected virtual void Update() {
      var time = Time.time;
      var curPose = pose;
      _deltaPosBuffer.Add(curPose.position, time);
      _deltaRotBuffer.Add(curPose.rotation, time);

      _movement = new Movement(_deltaPosBuffer.Delta(), _deltaRotBuffer.Delta());
    }

    public abstract Pose pose { get; }

    private Movement _movement = Movement.identity;
    public Movement movement {
      get { return _movement; }
    }

  }
  
}
