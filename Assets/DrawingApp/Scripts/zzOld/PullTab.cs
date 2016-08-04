using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// A PullTab attaches to a UI element and allows the UI element to be
/// gripped and pulled along an arbitrary one-dimensional axis by an
/// arbitrary transform. The PullTab will snap back into place if it is
/// released, and can optionally trigger events based on a specified
/// "activation" distance along its pull axis.
/// </summary>
public class PullTab : MonoBehaviour {

  #region PUBLIC ATTRIBUTES

  [Tooltip("The transform that is attached as a pullable object to this pull tab.")]
  public Transform _pullout;

  [Tooltip("The normalized local-space direction that the pull tab can be moved when being pulled by something. (Auto-normalizes.)")]
  public Vector3 _pullAxis = Vector3.right;

  [Tooltip("The distance along the pull axis past which the pull tab will trigger activation events.")]
  public float _activationDistance = 1F;

  #endregion

  #region UNITY EVENTS

  [Tooltip("Called when the pull tab is pressed down.")]
  public UnityEvent OnGrabbed;

  [Tooltip("Called when the pull tab is released, whether or not it was pulled out far enough to activate.")]
  public UnityEvent OnReleased;

  [Tooltip("Called when the pull tab is pulled beyond the activation threshold, so that it will activate if released.")]
  public UnityEvent OnPastActivationThreshold;

  //[Tooltip("Called when the pull tab is released, and wasn't pulled out far enough to activate.")]
  //public UnityEvent OnReleasedWithoutActivation;

  #endregion

  #region PRIVATE ATTRIBUTES

  /// <summary>
  /// The lcoal-space position that the pull tab tends towards when it's not
  /// being pulled. This is set automatically in Start() based on the
  /// initial transform of the pull tab.
  /// </summary>
  private Vector3 _restPosition;

  /// <summary>
  /// The position of the pullout object in the PullTab's local space.
  /// The pullout object is the UI element that the PullTab pulls on.
  /// This property is automatically set in Start() based on the
  /// transforms of the pullout and the pull tab.
  /// </summary>
  private Vector3 _pulloutOffset = Vector3.zero;

  /// <summary>
  /// PullTabs can also be grabbed by pointer events from the UI interface
  /// event system instead of by 3D Transforms.
  /// </summary>
  private bool _pointerAttached = false;

  /// <summary>
  /// If there is a pointer grabbing the Pulltab, this is the pointer's position
  /// in world space.
  /// </summary>
  private Vector3 _worldPointerPosition = Vector3.zero;

  /// <summary>
  /// Whether or not we have received the pointer position from the LeapInputModule
  /// callback.
  /// </summary>
  private bool _havePointerPosition = false;

  /// <summary>
  /// If there is an object attached to the pull tab (pulling it),
  /// this is the distance to that attached object's position along
  /// the pull axis (all in local space).
  /// </summary>
  private float _distanceAlongPullAxis;

  private Vector3 _localPointerOffset;

  private Canvas _canvas;

  /// <summary>
  /// Is the pull tab pulled out past the activation threshold distance
  /// along its pull axis?
  /// </summary>
  private bool _isPastActivationThreshold;

  #endregion

  #region PUBLIC PROPERTIES

  public bool IsAttached {
    get { return _pointerAttached == true; }
  }

  #endregion

  #region UNITY CALLBACKS

  protected virtual void OnValidate() {
    _pullAxis = _pullAxis.normalized;
  }

  protected virtual void Start() {
    _canvas = GetComponentInParent<Canvas>();
    _restPosition = this.transform.localPosition;
    if (_pullout != null) {
      _pulloutOffset = this.transform.InverseTransformPoint(_pullout.transform.position);
    }
  }

  protected virtual void Update() {
    if (IsAttached) {
      if (_pointerAttached && _havePointerPosition) {
        if (_pullAxis.z != 0F) {
          Debug.LogWarning("2D canvas (XY) grabbing can't move the PullTab along the Z axis!");
        }
        Vector3 restPositionToPointer = _canvas.transform.InverseTransformPoint(_worldPointerPosition) - _localPointerOffset - _restPosition;
        _distanceAlongPullAxis = Mathf.Max(0F, Vector3.Dot(restPositionToPointer, _pullAxis));
        this.transform.localPosition = _restPosition + (_pullAxis * _distanceAlongPullAxis);
      }

      // Trigger activation depending on the pull tab's progress along its pull axis.
      if (_distanceAlongPullAxis >= _activationDistance && !_isPastActivationThreshold) {
        _isPastActivationThreshold = true;
        OnPastActivationThreshold.Invoke();
      }
      else if (_isPastActivationThreshold) {
        _isPastActivationThreshold = false;
      }
    }
    else {
      // Snap the pull tab back into place. (Speed linearly correlated to the distance to the resting position.)
      float snapCoefficient = 0.25F;
      this.transform.localPosition += (_restPosition - this.transform.localPosition) * snapCoefficient;
    }

    // Have the pull tab pull on the pullout object it is attached to.
    if (_pullout != null) {
      _pullout.transform.position = _canvas.transform.TransformPoint(this.transform.localPosition +_pulloutOffset);
    }
  }

  #endregion

  #region PUBLIC METHODS

  public void Grab() {
    _pointerAttached = true;
    OnGrabbed.Invoke();
  }

  public void Release() {
    // UI pointer grabbing
    _pointerAttached = false;
    _havePointerPosition = false;

    OnReleased.Invoke();
  }

  #endregion

  #region Event System Callbacks

  public void OnDrag(Vector3 worldPointerPosition) {
    if (_pointerAttached) {
      _worldPointerPosition = worldPointerPosition;
      if (!_havePointerPosition) {
        _localPointerOffset = this.transform.InverseTransformPoint(_worldPointerPosition);
      }
      _havePointerPosition = true;
    }
  }

  #endregion

}
