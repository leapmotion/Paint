using UnityEngine;
using System.Collections;
using Leap.Unity.Interaction;
using UnityEngine.Events;

public class GraspCallbacksBehaviour : InteractionBehaviour {

  public UnityEvent OnGraspBeginEvent;
  public UnityEvent OnGraspEndEvent;

  [System.Serializable]
  public class FloatEvent : UnityEvent<float> { }

  [SerializeField]
  public FloatEvent PalmVelocityOnReleaseEvent;

  protected override void OnGraspBegin() {
    base.OnGraspBegin();

    OnGraspBeginEvent.Invoke();
  }

  protected override void OnGraspEnd(Leap.Hand lastHand) {
    base.OnGraspEnd(lastHand);

    OnGraspEndEvent.Invoke();

    PalmVelocityOnReleaseEvent.Invoke(lastHand.PalmVelocity.Magnitude);
  }

}
