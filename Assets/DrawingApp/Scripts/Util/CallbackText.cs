using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class CallbackText : Text {

  public UnityEvent OnTextChanged;

  public void SetText(string text) {
    this.text = text;
    OnTextChanged.Invoke();
  }

}
