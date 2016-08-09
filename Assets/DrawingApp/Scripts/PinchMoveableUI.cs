using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PinchMoveableUI : MonoBehaviour {

  #region PUBLIC FIELDS

  [Tooltip("The Transform that will move the UI element. Can be this GameObject's Transform.")]
  public Transform _moveableAnchor;

  #endregion

  #region PRIVATE FIELDS

  private Transform _pinchCursor = null;
  private Vector3 _cursorAnchorOffset = Vector3.zero;

  #endregion

  #region PUBLIC PROPERTIES

  public bool IsPinched {
    get { return _pinchCursor != null; }
  }

  #endregion

  #region UNITY EVENTS

  public UnityEvent OnGrabbed;
  public UnityEvent OnReleased;

  #endregion

  #region UNITY CALLBACKS

  protected void Update() {
    if (IsPinched) {
      _moveableAnchor.position = _pinchCursor.position + _cursorAnchorOffset;
    }
  }

  #endregion

  #region PUBLIC METHODS

  public void BePinchedBy(Transform pinchPoint) {
    _pinchCursor = pinchPoint;
    _cursorAnchorOffset = _moveableAnchor.position - _pinchCursor.position;
    OnGrabbed.Invoke();
  }

  public void ReleaseFromPinch() {
    if (!IsPinched) {
      Debug.LogWarning("[PinchMoveableUI] ReleaseFromPinch() called but this PinchMoveableUI is not currently pinched.");
    }
    else {
      _pinchCursor = null;
      _cursorAnchorOffset = Vector3.zero;
      OnReleased.Invoke();
    }
  }

  #endregion

}
