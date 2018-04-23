﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Leap.Unity.Interaction;
using Leap.Unity.Gestures;
using Leap.Unity.LeapPaint_v3;

public class TutorialControl : MonoBehaviour {

  public Text text;
  public PinchGesture leftPinch, rightPinch;
  public Transform strokeParent;
  public Widget colorWidget;
  public Widget brushWidget;
  public Widget menuWidget;
  public EmergeableBehaviour bigColorEmergable;
  public GameObject brushThicknessObject;

  [NonSerialized]
  public bool colorPalleteHasBeenTouched = false;

  [NonSerialized]
  public bool colorPalleteHasBeenExpanded = false;

  private void Update() {
    if (bigColorEmergable.IsEmergedOrEmerging) {
      colorPalleteHasBeenExpanded = true;
    }
  }

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

  public void EnableBrushThickness() {
    brushThicknessObject.SetActive(true);
  }

  public void EnableWidgetGrasping() {
    colorWidget.ieBehaviour.ignoreGrasping = false;
    menuWidget.ieBehaviour.ignoreGrasping = false;
    brushWidget.ieBehaviour.ignoreGrasping = false;

    colorWidget.trigger.enabled = true;
    menuWidget.trigger.enabled = true;
    brushWidget.trigger.enabled = true;
  }

  public void EnableFinalMenu() {
    menuWidget.Enable();
  }

  public void NotifyColorPalleteTouched() {
    colorPalleteHasBeenTouched = true;
  }

  public void NotifyColorPalleteExpanded() {
    colorPalleteHasBeenExpanded = true;
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
    public InteractionBehaviour ieBehaviour;
    public IndexUIActivator_PassTriggerEvents trigger;

    public IEnumerator Enable() {
      ring.SetActive(true);



      widget.GetComponent<WearableUI>().forceHide = false;
      widget.GetComponent<WearableUI>().RefreshVisibility();

      if (emergable != null) {
        emergable.TryEmerge(isInWorkstation: true);
      }

      yield return null;
    }
  }
}