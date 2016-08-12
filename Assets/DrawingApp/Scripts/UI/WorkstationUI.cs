using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WorkstationUI : MonoBehaviour {

  public Transform _centerEyeAnchor;

  public void LerpWearableToWorkstationPositionIfNotAlready() {
    WearableUI wearable = GetComponentInParent<WearableUI>();
    if (wearable != null) {
      if (!wearable.IsIndependent) {
        wearable.MakeIndependent();
        LerpToWorkstationPosition(wearable.transform);
      }
    }
  }

  public void LerpToWorkstationPosition(Transform toLerp) {
    StartCoroutine(EaseToPosition(toLerp, GetReasonableWorkstationPosition()));
  }

  public Vector3 GetReasonableWorkstationPosition() {
    float scaleFactor = _centerEyeAnchor.transform.parent.localScale.x;
    float reasonableDistance = 0.5F * scaleFactor;
    Vector3 lookVector = _centerEyeAnchor.transform.forward;
    Vector3 flattened = new Vector3(lookVector.x, -reasonableDistance/3F, lookVector.z);
    return _centerEyeAnchor.position + (flattened.normalized * reasonableDistance);
  }

  private IEnumerator EaseToPosition(Transform toLerp, Vector3 worldPosition) {
    float lerpTime = 0.4F;
    float timer = 0F;
    AnimationCurve movementCurve = AnimationCurve.EaseInOut(0F, 0F, 1F, 1F);
    Vector3 origPosition = toLerp.position;
    while (Vector3.Distance(toLerp.position, worldPosition) > 0.0001F) {
      timer += Time.deltaTime;
      toLerp.position = Vector3.Lerp(origPosition, worldPosition, movementCurve.Evaluate(timer / lerpTime));
      yield return new WaitForEndOfFrame();
    }
    toLerp.position = worldPosition;
  }


  [Header("For Color UI (TODO: Changeme)")]

  public EmergeableUI[] workstationEmergeables;
  public WearableUI wearableUI;

  public void DisableWorkstationIfActive() {
    if (IsWorkstationActive()) {
      for (int i = 0; i < workstationEmergeables.Length; i++) {
        workstationEmergeables[i].Unlock();
        workstationEmergeables[i].EnsureVanished();
      }
      StartCoroutine(DoWearableVanishAfterSeconds(0.3F));
    }
  }

  private bool IsWorkstationActive() {
    if (!wearableUI.IsIndependent) {
      return false;
    }
    for (int i = 0; i < workstationEmergeables.Length; i++) {
      if (!workstationEmergeables[i].IsEmerged) {
        return false;
      }
    }
    return true;
  }

  private IEnumerator DoWearableVanishAfterSeconds(float seconds) {
    float timer = 0F;
    while (timer < seconds) {
      timer += Time.deltaTime;
      yield return new WaitForEndOfFrame();
    }
    wearableUI.RemoveIndependence();
  }

}
