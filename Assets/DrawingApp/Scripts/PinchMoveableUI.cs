using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Leap.Unity;

[System.Serializable]
public class Vector3Event : UnityEvent<Vector3> { }

public class PinchMoveableUI : MonoBehaviour {

  #region PUBLIC FIELDS

  [Tooltip("The Transform that will move the UI element. Can be this GameObject's Transform.")]
  public Transform _moveableAnchor;

  #endregion

  #region PRIVATE FIELDS

  private Transform _pinchCursor = null;
  private Vector3 _cursorAnchorOffset = Vector3.zero;
  private IHandModel _pinchedByHand = null;

  #endregion

  #region PUBLIC PROPERTIES

  public bool IsPinched {
    get { return _pinchCursor != null; }
  }

  #endregion

  #region UNITY EVENTS

  public UnityEvent PrePreOnGrabbed;
  public UnityEvent PreOnGrabbed;
  public UnityEvent OnGrabbed;
  public UnityEvent PreOnReleased;
  public UnityEvent OnReleased;
  public Vector3Event OnReleaseVelocity;

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

    _pinchedByHand = pinchPoint.GetComponentInParent<PinchDetector>().GetHandModel();

    _pinchCursor = pinchPoint;
    _cursorAnchorOffset = _moveableAnchor.position - _pinchCursor.position;
    PrePreOnGrabbed.Invoke();
    PreOnGrabbed.Invoke();
    OnGrabbed.Invoke();
  }

  public void ReleaseFromPinch() {
    if (!IsPinched) {
      Debug.LogWarning("[PinchMoveableUI] ReleaseFromPinch() called but this PinchMoveableUI is not currently pinched.");
    }
    else {
      _pinchCursor = null;
      _cursorAnchorOffset = Vector3.zero;
      PreOnReleased.Invoke();
      OnReleased.Invoke();
      OnReleaseVelocity.Invoke(_pinchedByHand.GetLeapHand().PalmVelocity.ToVector3());
    }
  }

  #endregion

}
