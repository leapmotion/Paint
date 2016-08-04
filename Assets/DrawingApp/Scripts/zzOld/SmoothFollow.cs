using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class SmoothFollow : MonoBehaviour {

  [SerializeField]
  private Transform _followTarget = null;

  public UnityEvent OnReachedTarget;

  private bool _invokedReachedTarget = false;

  public void SetFollowTarget(Transform target) {
    _followTarget = target;
    _invokedReachedTarget = false;
  }

  protected virtual void Update() {
    if (_followTarget != null) {
      this.transform.position += (_followTarget.position - this.transform.position) * 0.25F;
      if ((this.transform.position - _followTarget.position).magnitude < 0.001F && !_invokedReachedTarget) {
        OnReachedTarget.Invoke();
        _invokedReachedTarget = true;
      }
    }
  }

}
