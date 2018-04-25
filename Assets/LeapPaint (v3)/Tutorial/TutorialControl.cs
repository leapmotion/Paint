using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Leap.Unity.Interaction;
using Leap.Unity.Gestures;
using Leap.Unity.LeapPaint_v3;

public class TutorialControl : MonoBehaviour {

  public Text text;
  public PinchGesture rightPinch;
  public Transform strokeParent;
  public Widget colorWidget;
  public Widget brushWidget;
  public Widget menuWidget;
  public EmergeableBehaviour bigColorEmergable;
  public GameObject brushThicknessObject;

  [Header("Tutorial Objects")]
  public GameObject tutorialBot;
  public GameObject tutorialPostProcess;

  [NonSerialized]
  public bool colorPalleteHasBeenTouched = false;

  [NonSerialized]
  public bool colorPalleteHasBeenExpanded = false;

  private void Update() {
    if (bigColorEmergable.IsEmergedOrEmerging) {
      colorPalleteHasBeenExpanded = true;
    }
  }

  public void EnableTutorial() {
    // Set the player rig state to reflect what the beginning of the tutorial expects.
    rightPinch.enabled = false;

    StartCoroutine(colorWidget.Disable());
    StartCoroutine(brushWidget.Disable());
    StartCoroutine(menuWidget.Disable());

    tutorialBot.SetActive(true);
    tutorialPostProcess.SetActive(true);

    brushThicknessObject.SetActive(false);
  }

  public void DisableTutorial() {
    // Unlock everything!
    rightPinch.enabled = true;

    StartCoroutine(colorWidget.Enable());
    StartCoroutine(brushWidget.Enable());
    StartCoroutine(menuWidget.Enable());

    tutorialBot.SetActive(false);
    tutorialPostProcess.SetActive(false);

    brushThicknessObject.SetActive(true);
  }

  public void SetText(string text) {
    this.text.text = text;
  }

  public void ClearText() {
    text.text = "";
  }

  public void EnablePinching() {
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
    StartCoroutine(menuWidget.Enable());
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
    public EmergeableBehaviour[] emergables;
    public InteractionBehaviour ieBehaviour;
    public IndexUIActivator_PassTriggerEvents trigger;

    public IEnumerator Enable() {
      yield return new WaitForEndOfFrame();

      ring.SetActive(true);

      widget.GetComponent<WearableUI>().forceHide = false;
      widget.GetComponent<WearableUI>().RefreshVisibility();

      foreach (var emergable in emergables) {
        if (emergable != null) {
          emergable.TryEmerge(isInWorkstation: false);
        }
      }

      yield return null;
    }

    public IEnumerator Disable() {
      yield return new WaitForEndOfFrame();

      foreach (var emergable in emergables) {
        if (emergable != null) {
          emergable.TryVanish(isInWorkstation: false);
        }
      }

      widget.GetComponent<WearableUI>().forceHide = true;
      widget.GetComponent<WearableUI>().RefreshVisibility();

      ring.SetActive(false);
    }
  }
}
