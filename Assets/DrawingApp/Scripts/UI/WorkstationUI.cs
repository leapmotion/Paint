using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WorkstationUI : MonoBehaviour {

  public Transform _centerEyeAnchor;

  public void LerpToWorkstationPosition(Transform toLerp) {
    StartCoroutine(EaseToPosition(toLerp, GetReasonableWorkstationPosition()));
  }

  private Vector3 GetReasonableWorkstationPosition() {
    float scaleFactor = _centerEyeAnchor.transform.parent.localScale.x;
    float reasonableDistance = 0.5F * scaleFactor;
    Vector3 lookVector = _centerEyeAnchor.transform.forward;
    Vector3 flattened = new Vector3(lookVector.x, -reasonableDistance/3F, lookVector.z);
    return _centerEyeAnchor.position + (flattened.normalized * reasonableDistance);
  }

  private IEnumerator EaseToPosition(Transform toLerp, Vector3 worldPosition) {
    toLerp.position = worldPosition;
    yield return null;
  }

}
