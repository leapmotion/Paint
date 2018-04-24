using UnityEngine;
using System.Collections;

public class TargetObjectToggle : MonoBehaviour {

  #region PUBLIC FIELDS

  public GameObject _target;
  public bool _enabledOnStart = false;

  #endregion

  #region PROPERTIES

  public bool IsTargetEnabled {
    get { return _target.activeInHierarchy; }
  }

  #endregion

  #region UNITY CALLBACKS

  protected void Start() {
    if (_enabledOnStart) {
      EnableTarget();
    }
    else {
      DisableTarget();
    }
  }

  protected void OnEnable() {
    _target.SetActive(true);
  }

  protected void OnDisable() {
    _target.SetActive(false);
  }

  #endregion

  #region PUBLIC METHODS

  public void EnableTarget() {
    this.enabled = true;
    _target.SetActive(true);
  }

  public void DisableTarget() {
    this.enabled = false;
    _target.SetActive(false);
  }

  #endregion

}
