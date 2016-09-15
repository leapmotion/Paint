using UnityEngine;
using System.Collections;

public class GrabbingTip : MonoBehaviour {

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
    else if (_workstationTipGiven) {
      _workstationTipGiven = false;
      text.gameObject.SetActive(false);
    }
  }

  private WearableUI thrownMarble;
  void DoOnUnGrab() {
    if (!hasShownOnce) {
      //Tween Disappear
      text.gameObject.SetActive(false);
      hasShownOnce = true;
      isShowing = false;

      if (thrownMarble != null) {
        thrownMarble.OnWorkstationActivated += DoOnWorkstationActivated;
      }
    }
  }

  private bool _workstationTipGiven = false;
  private void DoOnWorkstationActivated() {
    if (!_workstationTipGiven) {
      text.gameObject.SetActive(true);
      transform.position = thrownMarble.transform.position + Vector3.right * 0.3F;
      transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
      text.text = "You can put the bubble back\nin your hand when you're done.";
      _workstationTipGiven = true;
    }
  }

  // Update is called once per frame
  void Update() {
    if (isShowing) {
      if (Manager._leftGrabbedWearable != null) {
        transform.position = (Manager._leftGrabbedWearable as WearableUI).transform.position + Vector3.up * 0.06f;
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        thrownMarble = Manager._leftGrabbedWearable as WearableUI;
        if (thrownMarble == null) {
          Debug.Log("BERP");
        }
      } else if (Manager._rightGrabbedWearable != null) {
        transform.position = (Manager._rightGrabbedWearable as WearableUI).transform.position + Vector3.up * 0.06f;
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        thrownMarble = Manager._rightGrabbedWearable as WearableUI;
        if (thrownMarble == null) {
          Debug.Log("BERP");
        }
      }
    }
  }
}