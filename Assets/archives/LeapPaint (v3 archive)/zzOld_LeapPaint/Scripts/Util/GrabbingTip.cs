using UnityEngine;
using System.Collections;
using Leap.Unity.Animation;
using Leap.Unity.Interaction;

namespace Leap.Unity.LeapPaint_v3 {

  public class GrabbingTip : MonoBehaviour {

    public InteractionHand rightHand;
    public TextMesh text;

    private bool isShowing = false;
    private bool hasShownOnce = false;

    private LineRenderer firstLine;
    private Vector3[] line1 = new Vector3[2];
    private float fade = 0f;

    public Tween disappearTween;

    void Start() {
      text.gameObject.SetActive(false);
      //Anchor.OnAnchorBeginDisappearing += DoOnBeginDisappearing;

      rightHand.OnGraspBegin += DoOnGrab;
      rightHand.OnGraspEnd += DoOnUnGrab;

      firstLine = gameObject.AddComponent<LineRenderer>();
      firstLine.useWorldSpace = true;
      firstLine.material = new Material(Shader.Find("Standard"));
      firstLine.startColor = firstLine.endColor = Color.black;
      firstLine.SetPositions(line1);
      firstLine.startWidth = firstLine.endWidth = 0.002F;

      disappearTween = Tween.Persistent().Value(new Color(0.9f, 0.9f, 0.9f, 1f), new Color(0.9f, 0.9f, 0.9f, 0f), SetOpacity)
        .OverTime(0.3f)
        .Smooth(SmoothType.Smooth)
        .OnReachEnd(Hide);
    }

    void Hide() {
      text.gameObject.SetActive(false);
    }

    void DoOnGrab() {
      if (!hasShownOnce) {
        isShowing = true;
        //Tween Appear
        text.gameObject.SetActive(true);
        //disappearTween.Progress = 1f;
        //disappearTween.Play(TweenDirection.BACKWARD);
      } else if (_workstationTipGiven) {
        text.gameObject.SetActive(false);
        //TweenDisappear
        //disappearTween.Progress = 0f;
        //disappearTween.Play();
      }
    }

    private WearableUI thrownMarble;
    void DoOnUnGrab() {
      if (!hasShownOnce) {
        text.gameObject.SetActive(false);
        //Tween Disappear
        //disappearTween.Progress = 0f;
        //disappearTween.Play();

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
        text.text = "Grab the bubble to put it back\nin your hand when you're done.";
        _workstationTipGiven = true;
        this.transform.position = thrownMarble.transform.position - (Vector3.Cross(thrownMarble.transform.position - Camera.main.transform.position, Vector3.up).normalized * 0.32f) + Vector3.up * 10F;
      }
    }

    // Update is called once per frame
    private bool _showWorkstationTipLine = false;
    void Update() {
      Vector3 marblePos = Vector3.zero;
      if (isShowing) {
        if (rightHand.graspedObject != null) {
          var marble = (rightHand.graspedObject as MonoBehaviour).GetComponent<WearableUI>();
          if (marble != null) {
            marblePos = marble.transform.position;
            thrownMarble = marble;
          }
        }

        transform.position = Vector3.Lerp(transform.position, marblePos + Vector3.up * 0.06f, 0.1f);
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

        firstLine.enabled = true;
        firstLine.SetPosition(0, Vector3.Lerp(transform.position, marblePos, Mathf.Lerp(0.3f, 0.9f, fade)));
        firstLine.SetPosition(1, Vector3.Lerp(transform.position, marblePos, 0.5f));
      }
      else if (_workstationTipGiven && text.gameObject.activeInHierarchy) {
        transform.position = Vector3.Lerp(transform.position, thrownMarble.transform.position - (Vector3.Cross(thrownMarble.transform.position - Camera.main.transform.position, Vector3.up).normalized * 0.32f), 0.1f);
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

        firstLine.SetPosition(0, Vector3.Lerp(transform.position, thrownMarble.transform.position, 0.5F));
        firstLine.SetPosition(1, Vector3.Lerp(transform.position, thrownMarble.transform.position, 0.9F));

        if (!_showWorkstationTipLine && Vector3.Distance(transform.position, thrownMarble.transform.position) < 0.4F) {
          _showWorkstationTipLine = true;
        }
      }

      firstLine.enabled = isShowing || _showWorkstationTipLine;
    }

    public void SetOpacity(Color color) {
      fade = 1f - color.a;
      Material textMat = text.GetComponent<Renderer>().material;
      textMat.color = color;
    }
  }
}
