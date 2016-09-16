using UnityEngine;
using System.Collections;

public class GrabbingTip : MonoBehaviour {

  public WearableManager Manager;
  public TextMesh text;

  private bool isShowing = false;
  private bool hasShownOnce = false;

  private LineRenderer firstLine;
  private Vector3[] line1 = new Vector3[2];
  private float fade = 0f;

  TweenHandle disappearTween;

  void Start() {
    text.gameObject.SetActive(false);
    //Anchor.OnAnchorBeginDisappearing += DoOnBeginDisappearing;
    Manager.OnGrabBegin += DoOnGrab;
    Manager.OnGrabEnd += DoOnUnGrab;

    firstLine = gameObject.AddComponent<LineRenderer>();
    firstLine.useWorldSpace = true;
    firstLine.material = new Material(Shader.Find("Standard"));
    firstLine.SetColors(Color.black, Color.black);
    firstLine.SetPositions(line1);
    firstLine.SetWidth(0.002f, 0.002f);

    disappearTween = Tween.Value(new Color(0.9f, 0.9f, 0.9f, 1f), new Color(0.9f, 0.9f, 0.9f, 0f), SetOpacity)
      .OverTime(0.3f)
      .Smooth(TweenType.SMOOTH)
      .OnReachEnd(Hide)
      .Keep();
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
      text.text = "You can put the bubble back\nin your hand when you're done.";
      _workstationTipGiven = true;
    }
  }

  // Update is called once per frame
  void Update() {
    Vector3 marblePos = Vector3.zero;
    if (isShowing) {
      if (Manager._leftGrabbedWearable != null) {
        marblePos = (Manager._leftGrabbedWearable as WearableUI).transform.position;
        thrownMarble = Manager._leftGrabbedWearable as WearableUI;
      } else if (Manager._rightGrabbedWearable != null) {
        marblePos = (Manager._rightGrabbedWearable as WearableUI).transform.position;
        thrownMarble = Manager._rightGrabbedWearable as WearableUI;
      }

      if (thrownMarble == null) {
        Debug.Log("BERP");
      }

      transform.position = Vector3.Lerp(transform.position, marblePos + Vector3.up * 0.04f, 0.1f);
      transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

      firstLine.enabled = true;
      firstLine.SetPosition(0, Vector3.Lerp(transform.position, marblePos, Mathf.Lerp(0f, 0.9f, fade)));
      firstLine.SetPosition(1, Vector3.Lerp(transform.position, marblePos, 0.3f));
    } else if (_workstationTipGiven && text.gameObject.activeInHierarchy) {
      transform.position = Vector3.Lerp(transform.position, thrownMarble.transform.position - (Vector3.Cross(thrownMarble.transform.position - Camera.main.transform.position, Vector3.up).normalized * 0.32f), 0.1f);
      transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }
    firstLine.enabled = isShowing;
  }

  public void SetOpacity(Color color) {
    fade = 1f - color.a;
    Material textMat = text.GetComponent<Renderer>().material;
    textMat.color = color;
  }
}