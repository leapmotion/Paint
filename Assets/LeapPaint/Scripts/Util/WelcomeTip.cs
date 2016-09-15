using UnityEngine;
using System.Collections;
using System;

public class WelcomeTip : MonoBehaviour {
  public WearableAnchor Anchor;
  public PinchStrokeProcessor RPinchDrawer;
  public PinchStrokeProcessor LPinchDrawer;
  public TextMesh text;

  private bool hasPainted = false;
  private bool hasOpenedMenu = false;
  TweenHandle disappearTween;
  TweenHandle transitionTween;
  float xLocalRot;

  void Start() {
    Anchor.OnAnchorBeginAppearing += DoOnBeginAppearing;
    text.gameObject.SetActive(true);
    xLocalRot = transform.rotation.eulerAngles.x;
    disappearTween = Tween.Value(new Color(0.9f, 0.9f, 0.9f, 0.9f), new Color(0.9f, 0.9f, 0.9f, 0f), SetOpacity)
      .OverTime(0.5f)
      .Smooth(TweenType.SMOOTH)
      .OnReachEnd(Hide)
      .Keep();

    transitionTween = Tween.Value(new Color(0.9f, 0.9f, 0.9f, 0.9f), new Color(0.9f,0.9f,0.9f,0f), SetOpacity)
      .OverTime(0.5f)
      .Smooth(TweenType.SMOOTH)
      .OnReachEnd(TextChange)
      .Keep();
  }

  void DoOnBeginAppearing() {
    if (!hasOpenedMenu) {
      hasOpenedMenu = true;
    }
    ChangeState();
  }

  void Hide() {
    text.gameObject.SetActive(false);
  }

  void TextChange() {
    transitionTween.Play(TweenDirection.BACKWARD);
    text.text = "View palm for more options.";
  }

  void ChangeState() {
    if (hasOpenedMenu && hasPainted) {
      transitionTween.Stop();
      disappearTween.Play();
    } else if(hasPainted) {
      transitionTween.Play();
    }
  }

  void Update() {
    if (!hasPainted && RPinchDrawer.drawTime + LPinchDrawer.drawTime > 1f) {
      hasPainted = true;
      ChangeState();
    }
  }

  public void SetOpacity(Color color) {
    Material textMat = text.GetComponent<Renderer>().material;
    textMat.color = color;
  }
}