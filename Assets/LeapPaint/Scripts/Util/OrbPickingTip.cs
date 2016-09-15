using UnityEngine;
using System.Collections;

public class OrbPickingTip : MonoBehaviour {
  //public WearableAnchor Anchor;
  public WearableManager Manager;
  public WearableUI MenuUI;
  public WearableUI ColorUI;
  public WearableUI BrushUI;
  public TextMesh text;

  private bool hasShownOnce = false;

  void Start() {
    text.gameObject.SetActive(true);
    //Anchor.OnAnchorBeginDisappearing += DoOnBeginDisappearing;
    Manager.OnGrabBegin += DoOnBeginDisappearing;
    MenuUI.OnActivateMarble += DoOnBeginDisappearing;
    ColorUI.OnActivateMarble += DoOnBeginDisappearing;
    BrushUI.OnActivateMarble += DoOnBeginDisappearing;
  }

  void DoOnBeginDisappearing() {
    if (!hasShownOnce) {
      //Tween Disappear
      text.gameObject.SetActive(false);
      hasShownOnce = true;
    }
  }

  // Update is called once per frame
  void Update() {

  }
}