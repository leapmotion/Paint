using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WorkstationUI : MonoBehaviour {

  public void EnableDisplay() {
    Debug.Log("[WorkstationUI] Display enabled.");
    foreach (Graphic graphic in GetComponentsInChildren<Graphic>()) {
      graphic.enabled = true;
    }
  }

  public void DisableDisplay() {
    Debug.Log("[WorkstationUI] Display disabled.");
    foreach (Graphic graphic in GetComponentsInChildren<Graphic>()) {
      graphic.enabled = false;
    }
  }

}
