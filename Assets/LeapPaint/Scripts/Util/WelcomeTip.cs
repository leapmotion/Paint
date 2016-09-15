using UnityEngine;
using System.Collections;

public class WelcomeTip : MonoBehaviour {
  public WearableAnchor Anchor;
  public PinchStrokeProcessor RPinchDrawer;
  public PinchStrokeProcessor LPinchDrawer;
  public TextMesh text;

  private bool hasChanged = false;
  private bool isFinished = false;

  void Start() {
    text.gameObject.SetActive(true);
  }

  void DoOnBeginAppearing() {
    if (!isFinished) {
      //Tween Disappear
      text.gameObject.SetActive(false);
      isFinished = true;
    }
  }

  // Update is called once per frame
  void Update() {
    if (!hasChanged && RPinchDrawer.drawTime + LPinchDrawer.drawTime > 1f) {
      Anchor.OnAnchorBeginAppearing += DoOnBeginAppearing;
      //Tween Disappear
      text.text = "View palm for more options.";
      //Tween Appear
      hasChanged = true;
    }
  }
}