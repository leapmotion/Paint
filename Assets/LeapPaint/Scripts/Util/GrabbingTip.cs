using UnityEngine;
using System.Collections;

public class GrabbingTip : MonoBehaviour {
  //public WearableAnchor Anchor;
  public WearableManager Manager;
  public TextMesh text;

  private bool isShowing = true;
  private bool hasShownOnce = false;

  void Start() {
    text.gameObject.SetActive(false);
    //Anchor.OnAnchorBeginDisappearing += DoOnBeginDisappearing;
    Manager.OnGrabBegin += DoOnGrab;
    Manager.OnGrabEnd += DoOnUnGrab;
  }

  void DoOnGrab() {
    if (!hasShownOnce) {
      isShowing = true;
      //Tween Appear
      text.gameObject.SetActive(true);
    }
  }

  void DoOnUnGrab() {
    if (!hasShownOnce) {
      //Tween Disappear
      text.gameObject.SetActive(false);
      hasShownOnce = true;
      isShowing = false;
    }
  }

  // Update is called once per frame
  void Update() {
    if (isShowing) {
      if (Manager._leftGrabbedWearable != null) {
        transform.position = (Manager._leftGrabbedWearable as WearableUI).transform.position + Vector3.up * 0.06f;
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
      } else if (Manager._rightGrabbedWearable != null) {
        transform.position = (Manager._rightGrabbedWearable as WearableUI).transform.position + Vector3.up * 0.06f;
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
      }
    }
  }
}