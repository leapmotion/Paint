using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class WearableAnchorRadiusChecker : MonoBehaviour {

  public WearableUI _wearableUI;
  public float _captureRadius = 1F;

  public UnityEvent PreOnCheckedBeyondRadius;
  public UnityEvent OnCheckedBeyondRadius;
  public UnityEvent PreOnCheckedWithinRadius;
  public UnityEvent OnCheckedWithinRadius;

  public void CheckAnchorDistance() {
    if (Vector3.Distance(this.transform.position, _wearableUI._anchorPoint.position) < _captureRadius) {
      PreOnCheckedWithinRadius.Invoke();
      OnCheckedWithinRadius.Invoke();
    }
    else {
      PreOnCheckedBeyondRadius.Invoke();
      OnCheckedBeyondRadius.Invoke();
    }
  }

}
