using UnityEngine;
using System.Collections;

public class OrbPickingTip : MonoBehaviour {
  //public WearableAnchor Anchor;
  public WearableManager Manager;
  public WearableUI MenuUI;
  public WearableUI ColorUI;
  public WearableUI BrushUI;
  public TextMesh text;
  public TweenHandle disappearTween;

  private bool hasShownOnce = false;

  void Start() {
    text.gameObject.SetActive(true);
    Manager.OnGrabBegin += DoOnBeginDisappearing;
    MenuUI.OnActivateMarble += DoOnBeginDisappearing;
    ColorUI.OnActivateMarble += DoOnBeginDisappearing;
    BrushUI.OnActivateMarble += DoOnBeginDisappearing;
    disappearTween = Tween.Value(new Color(0.9f, 0.9f, 0.9f, 0.9f), new Color(0.9f, 0.9f, 0.9f, 0f), SetOpacity)
      .OverTime(0.3f)
      .Smooth(TweenType.SMOOTH)
      .OnReachEnd(Hide)
      .Keep();
  }

  void DoOnBeginDisappearing() {
    if (!hasShownOnce) {
      disappearTween.Play();
      hasShownOnce = true;
    }
  }

  void Hide() {
    text.gameObject.SetActive(false);
  }

  // Update is called once per frame
  void Update() {
    if (MenuUI.AnchorChirality == Leap.Unity.Chirality.Left) {
      transform.localPosition = new Vector3(-3f, transform.localPosition.y, 0.55f);
    } else if (MenuUI.AnchorChirality == Leap.Unity.Chirality.Right) {
      transform.localPosition = new Vector3(3f, transform.localPosition.y, -0.55f);
    }
  }

  public void SetOpacity(Color color) {
    Material textMat = text.GetComponent<Renderer>().material;
    textMat.color = color;
  }
}