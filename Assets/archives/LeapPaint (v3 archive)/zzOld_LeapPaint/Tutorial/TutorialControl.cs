using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Leap.Unity.Gestures;

public class TutorialControl : MonoBehaviour {

  public Text text;
  public PinchGesture leftPinch, rightPinch;

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

  }


}
