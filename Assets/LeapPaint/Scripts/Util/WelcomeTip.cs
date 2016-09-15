using UnityEngine;
using System.Collections;

public class WelcomeTip : MonoBehaviour {
  public WearableAnchor Anchor;
  public PinchStrokeProcessor RPinchDrawer;
  public PinchStrokeProcessor LPinchDrawer;
  public TextMesh text;

  private bool hasPainted = false;
  private bool hasOpenedMenu = false;

  void Start() {
    Anchor.OnAnchorBeginAppearing += DoOnBeginAppearing;
    text.gameObject.SetActive(true);
  }

  void DoOnBeginAppearing() {
    if (!hasOpenedMenu) {
      hasOpenedMenu = true;
    }
    ChangeState();
  }

  void ChangeState() {
    if (hasOpenedMenu && hasPainted) {
      //Tween Disappear
      text.gameObject.SetActive(false);
    } else if(hasPainted) {
      //Tween Disappear
      text.text = "View palm for more options.";
      //Tween Appear
    }
  }

  // Update is called once per frame
  void Update() {
    if (!hasPainted && RPinchDrawer.drawTime + LPinchDrawer.drawTime > 1f) {
      hasPainted = true;
      ChangeState();
    }
  }
}