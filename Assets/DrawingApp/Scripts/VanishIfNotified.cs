using UnityEngine;
using System.Collections;

public class VanishIfNotified : MonoBehaviour {

  public EmergeableUI _emergeableUI;

  private bool _notified = false;

  public void Notify() {
    _notified = true;
  }

  public void Unnotify() {
    _notified = false;
  }

  public void VanishIfWasNotified() {
    if (_notified) {
      _emergeableUI.EnsureVanished();
      _notified = false;
    }
  }

}
