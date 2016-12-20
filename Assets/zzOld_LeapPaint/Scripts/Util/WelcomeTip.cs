using UnityEngine;
using System.Collections;
using System;
using Leap.Unity.Animation;

namespace Leap.zzOldPaint {

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
    Tween disappearTween;
    Tween transitionTween;
    Tween handImageDisappearTween;
    public float xLocalRot;

    void Start() {
      Anchor.OnAnchorBeginAppearing += DoOnMenuBeginAppearing;
      Anchor.OnAnchorBeginDisappearing += DoOnMenuBeginDisappearing;
      text.gameObject.SetActive(true);
      xLocalRot = transform.rotation.eulerAngles.x;
      disappearTween = Tween.Persistent().Value(new Color(0.9f, 0.9f, 0.9f, 0.9f), new Color(0.9f, 0.9f, 0.9f, 0f), SetOpacity)
        .OverTime(0.5f)
        .Smooth(SmoothType.Smooth)
        .OnReachEnd(Hide);

      handImageDisappearTween = Tween.Persistent().Value(new Color(0.9f, 0.9f, 0.9f, 0.9f), new Color(0.9f, 0.9f, 0.9f, 0f), SetHandImageOpacity)
        .OverTime(0.5F)
        .Smooth(SmoothType.Smooth);

      transitionTween = Tween.Persistent().Value(new Color(0.9f, 0.9f, 0.9f, 0.9f), new Color(0.9f, 0.9f, 0.9f, 0f), SetOpacity)
        .OverTime(0.5f)
        .Smooth(SmoothType.Smooth)
        .OnReachEnd(TextChange);
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
      transitionTween.Play(Direction.Backward);
      text.text = "Look at your palm for more options.";
    }

    void ChangeState() {
      if (menuOpenTimer > menuOpenSatisfyDuration && hasPainted) {
        transitionTween.Stop();
        disappearTween.Play();
        handImageDisappearTween.Play();
      }
      else if (hasPainted) {
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

    private void SetHandImageOpacity(Color color) {
      pinchImageRenderer.material.SetFloat(Shader.PropertyToID("_Alpha"), color.a);
    }
  }

}