using UnityEngine;
using System.Collections;
using UnityEngine.Events;

/// <summary> Arguments: position, rotation, delta time. </summary>
[System.Serializable]
public class StrokeUpdateEvent : UnityEvent<Vector3, Quaternion, float> { }

public class StrokeManager : MonoBehaviour {

  #region PUBLIC FIELDS

  [Tooltip("The Transform that determines the position and rotation of stroke points.")]
  public Transform _strokeCursor;

  #endregion

  #region PRIVATE FIELDS

  private bool _strokeInProgress = false;

  #endregion

  #region UNITY EVENTS

  public UnityEvent OnStrokeBegin;
  public StrokeUpdateEvent OnStrokeUpdate;
  public UnityEvent OnStrokeEnd;

  #endregion

  #region UNITY CALLBACKS

  protected void Update() {
    if (_strokeInProgress) {
      OnStrokeUpdate.Invoke(_strokeCursor.position, _strokeCursor.rotation, Time.deltaTime);
    }
  }

  #endregion

  #region PUBLIC METHODS

  public void BeginStroke() {
    OnStrokeBegin.Invoke();
    _strokeInProgress = true;
  }

  public void EndStroke() {
    OnStrokeEnd.Invoke();
    _strokeInProgress = false;
  }

  #endregion

}
