using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Leap.Unity.Gestures;
using Leap.Unity.LeapPaint_v3;

public class TutorialControl : MonoBehaviour {

  public Text text;
  public PinchGesture leftPinch, rightPinch;
  public Transform strokeParent;
  public Widget colorWidget;
  public Widget brushWidget;
  public Widget menuWidget;

  [NonSerialized]
  public bool colorPalleteHasBeenTouched = false;

  public void SetText(string text) {
    this.text.text = text;
  }

  public void ClearText() {
    text.text = "";
  }

  public void EnablePinching() {
    leftPinch.enabled = true;
    rightPinch.enabled = true;
  }

  public void EnableColorPallete() {
    StartCoroutine(colorWidget.Enable());
  }

  public void EnableUndoRedo() {
    StartCoroutine(brushWidget.Enable());
  }

  public void NotifyColorPalleteTouched() {
    colorPalleteHasBeenTouched = true;
  }

  public void UndoStroke() {
    if (strokeParent.childCount == 0) {
      Debug.LogWarning("Could not undo a stroke because there are none!");
      return;
    }

    DestroyImmediate(strokeParent.GetChild(strokeParent.childCount - 1).gameObject);
  }

  [Serializable]
  public struct Widget {
    public GameObject widget;
    public GameObject ring;
    public EmergeableBehaviour emergable;
    public PassTriggerEvents marbleTrigger;

    public IEnumerator Enable() {
      widget.SetActive(true);
      ring.SetActive(true);

      yield return null;

      emergable.TryEmerge(isInWorkstation: true);
    }
  }
}
