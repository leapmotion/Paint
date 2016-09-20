using UnityEngine;
using System.Collections;
using System;

public class WelcomeTip : MonoBehaviour {

  public WearableAnchor Anchor;
  public PinchStrokeProcessor RPinchDrawer;
  public PinchStrokeProcessor LPinchDrawer;
  public TextMesh text;
  public MeshRenderer pinchImageRenderer;

  private bool hasPainted = false;
  private bool menuOpen = false;
  private float menuOpenTimer = 0F;
  private float menuOpenSatisfyDuration = 0.75F;
  private float hoverDistance = 1F;
  TweenHandle disappearTween;
  TweenHandle transitionTween;
  TweenHandle handImageDisappearTween;
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

    handImageDisappearTween = Tween.Target(pinchImageRenderer.material).Value(1F, 0F, "_Alpha")
      .OverTime(0.5F)
      .Smooth(TweenType.SMOOTH)
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
    pinchImageRenderer.gameObject.SetActive(false);
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
      handImageDisappearTween.Play();
    }
  }

  void Update() {
    if (text.gameObject.activeSelf) {
      Vector3 lookVector = Camera.main.transform.forward;
      Vector3 desiredPosition = Camera.main.transform.position + new Vector3(lookVector.x, -0.1F, lookVector.z) * hoverDistance;
      this.transform.position = Vector3.Lerp(this.transform.position, desiredPosition, 0.02F);
      this.transform.rotation = Quaternion.LookRotation(this.transform.position - Camera.main.transform.position);
    }

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