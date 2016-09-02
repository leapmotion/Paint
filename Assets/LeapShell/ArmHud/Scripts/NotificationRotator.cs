using UnityEngine;
using System;

public class NotificationRotator : MonoBehaviour {

  [SerializeField]
  public ArmBandTransition _armBand;

  [SerializeField]
  public float hiddenAngle;

  [SerializeField]
  public float displayAngle = 0;

  [SerializeField]
  public CanvasAlpha[] canvasGroups;

  [Serializable]
  public struct CanvasAlpha {
    public CanvasGroup canvasGroup;
    public float finalAlpha;
  }

  void Update() {
    float p = _armBand.DisplayPercent;

    foreach (var group in canvasGroups) {
      group.canvasGroup.alpha = Mathf.Clamp01(group.finalAlpha - (1 - p));
    }

    transform.localEulerAngles = new Vector3(0, Mathf.Lerp(hiddenAngle, displayAngle, p), 0);
  }
}
