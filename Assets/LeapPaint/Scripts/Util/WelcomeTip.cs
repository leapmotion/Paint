using UnityEngine;
using System.Collections;

public class WelcomeTip : MonoBehaviour {
  public WearableAnchor Anchor;
  public TextMesh text;

  private bool hasShownOnce = false;

  void Start() {
    text.gameObject.SetActive(true);
    Anchor.OnAnchorBeginAppearing += DoOnBeginAppearing;
  }

  void DoOnBeginAppearing() {
    if (!hasShownOnce) {
      text.gameObject.SetActive(false);
      hasShownOnce = true;
    }
  }

  // Update is called once per frame
  void Update() {

  }
}