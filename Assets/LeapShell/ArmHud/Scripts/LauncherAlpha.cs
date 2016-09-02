using UnityEngine;
using System.Collections;

public class LauncherAlpha : MonoBehaviour {

  public ArmBandTransition _armBand;

  public CanvasGroup group;

  public Transform startAnchor, endAnchor;

  void Update() {
    float t = _armBand.DisplayPercent;
    transform.position = Vector3.Lerp(startAnchor.position, endAnchor.position, t);
    transform.rotation = Quaternion.Slerp(startAnchor.rotation, endAnchor.rotation, t);

    group.alpha = _armBand.DisplayPercent;
  }

}
