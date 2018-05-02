using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Leap.Unity.Interaction;
using Leap.Unity.Gestures;
using Leap.Unity.LeapPaint_v3;
using Leap.Unity.Attributes;

public class TutorialControl : MonoBehaviour {

  public TextFader text;
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

  [Header("Runtime")]
  [Disable]
  public bool colorPalleteHasBeenTouched = false;

  [Disable]
  public bool colorPalleteHasBeenExpanded = false;

  private string _prevText = "";
  private int _possibilityIndex = 0;

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
    if (text.Contains("#")) {
      string[] possibilities = text.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
      if (text != _prevText) {
        _possibilityIndex = 0;
      }
      _prevText = text;

      this.text.SetText(possibilities[_possibilityIndex]);
      _possibilityIndex = (_possibilityIndex + 1) % possibilities.Length;
    } else {
      this.text.SetText(text);
    }
  }

  public void ClearText() {
    text.SetText("");
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

  public void NotifyTutorialCompleted() {
    LobbyControl.hasExperiencedTutorial = true;
  }

  public void UndoStroke() {
    if (strokeParent.childCount == 0) {
      Debug.LogWarning("Could not undo a stroke because there are none!");
      return;
    }

    DestroyImmediate(strokeParent.GetChild(strokeParent.childCount - 1).gameObject);
  }

  public void UndoAllStrokes() {
    GameObject[] children = new GameObject[strokeParent.childCount];
    for (int i = 0; i < strokeParent.childCount; i++) {
      children[i] = strokeParent.GetChild(i).gameObject;
    }

    foreach (var child in children) {
      DestroyImmediate(child);
    }
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
