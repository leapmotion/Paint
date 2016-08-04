using UnityEngine;
using System.Collections;

public class WearableWorkstationSwitcher : MonoBehaviour {

  #region PUBLIC ATTRIBUTES

  public WearableUI _wearableInterface;
  public WorkstationUI _workstationInterface;
  public bool _startAsWearable = true;

  #endregion

  #region PRIVATE ATTRIBUTES

  private bool foo;

  #endregion

  #region UNITY CALLBACKS

  protected virtual void Start() {
    if (_startAsWearable) {
      ChangeToWearable();
    }
    else {
      ChangeToWorkstation();
    }
  }

  #endregion

  #region PUBLIC METHODS

  public void ChangeToWearable() {
    _wearableInterface.EnableDisplay();
    _workstationInterface.DisableDisplay();
  }

  public void ChangeToWorkstation() {
    _workstationInterface.EnableDisplay();
    _wearableInterface.DisableDisplay();
  }

  #endregion

}
