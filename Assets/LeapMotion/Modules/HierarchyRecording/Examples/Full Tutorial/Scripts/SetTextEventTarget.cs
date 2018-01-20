using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetTextEventTarget : MonoBehaviour {

  public void ClearText() {
    GetComponent<Text>().text = "";
  }

  public void SetText(string text) {
    GetComponent<Text>().text = text;
  }
}
