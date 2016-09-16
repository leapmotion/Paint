using UnityEngine;
using System.Collections;
using System;

public class WelcomeTip : MonoBehaviour {
  public WearableAnchor Anchor;
  public PinchStrokeProcessor RPinchDrawer;
  public PinchStrokeProcessor LPinchDrawer;
  public TextMesh text;

  private bool hasPainted = false;
  private bool menuOpen = false;
  private float menuOpenTimer = 0F;
  private float menuOpenSatisfyDuration = 0.75F;
  TweenHandle disappearTween;
  TweenHandle transitionTween;
  float xLocalRot;

  void Start() {
    Anchor.OnAnchorBeginAppearing += DoOnMenuBeginAppearing;
    Anchor.OnAnchorBeginDisappearing += DoOnMenuBeginDisappearing;
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

  void DoOnMenuBeginAppearing() {
    if (!menuOpen) {
      menuOpen = true;
    }
  }

  void DoOnMenuBeginDisappearing() {
    if (menuOpen) {
      menuOpen = false;
    }
  }

  void Hide() {
    text.gameObject.SetActive(false);
  }

  void TextChange() {
    transitionTween.Play(TweenDirection.BACKWARD);
    text.text = "Look at your palm for more options.";
  }

  void ChangeState() {
    if (menuOpenTimer > menuOpenSatisfyDuration && hasPainted) {
      transitionTween.Stop();
      disappearTween.Play();
    } else if (hasPainted) {
      transitionTween.Play();
    }
  }

  void Update() {
    if (!hasPainted && RPinchDrawer.drawTime + LPinchDrawer.drawTime > 0.75f) {
      hasPainted = true;
      ChangeState();
    }
    if (menuOpen && menuOpenTimer < menuOpenSatisfyDuration) {
      menuOpenTimer += Time.deltaTime;
      if (menuOpenTimer > menuOpenSatisfyDuration) {
        ChangeState();
      }
    }
  }

  public void SetOpacity(Color color) {
    Material textMat = text.GetComponent<Renderer>().material;
    textMat.color = color;
  }
}