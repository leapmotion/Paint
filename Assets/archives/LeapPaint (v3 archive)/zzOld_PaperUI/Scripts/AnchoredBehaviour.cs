using UnityEngine;
using System.Collections;
using Leap.Unity.RuntimeGizmos;

[ExecuteInEditMode]
public class AnchoredBehaviour : MonoBehaviour, IRuntimeGizmoComponent {

  #region PUBLIC FIELDS

  public Transform _anchorTransform;
  public bool _isAttached = true;
  public bool _shouldAnchorRotation = false;
  public bool _alignToAnchorRotation = false;

  #endregion

  #region PRIVATE FIELDS

  private Quaternion _rotationOffset = Quaternion.identity;
  private bool _initializedRotation = false;
  private bool _oldAnchorRotationValue = false;

  #endregion

  #region PROPERTIES

  public bool IsAttached {
    get { return _isAttached; }
  }

  #endregion

  #region UNITY CALLBACKS

  protected virtual void Awake() {
    if (!_initializedRotation) {
      RecalculateRotationOffset();
    }
  }

  protected virtual void OnEnable() {
    Update();
  }

  protected virtual void OnValidate() {
    if (_shouldAnchorRotation != _oldAnchorRotationValue && _shouldAnchorRotation == true) {
      if (_alignToAnchorRotation) {
        _rotationOffset = Quaternion.identity;
      }
      else {
        RecalculateRotationOffset();
      }
    }
  }

  protected virtual void Update() {
    if (_isAttached && _anchorTransform != null) {
      this.transform.position = _anchorTransform.position;
      if (_shouldAnchorRotation) {
        this.transform.rotation = _anchorTransform.rotation * _rotationOffset;
      }
    }
  }

  #endregion

  public void AttachToAnchor() {
    _isAttached = true;
  }

  public void ReleaseFromAnchor() {
    _isAttached = false;
  }

  #region PRIVATE METHODS

  private void RecalculateRotationOffset() {
    if (_anchorTransform != null) {
      _rotationOffset = Quaternion.Inverse(_anchorTransform.rotation) * this.transform.rotation;
    }
  }

  #endregion

  #region GIZMOS

  protected bool _gizmosEnabled = false;

  public virtual void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    if (_gizmosEnabled) {
      drawer.color = Color.red;
      drawer.DrawSphere(_anchorTransform.transform.position, 0.005F);
    }
  }

  #endregion

}
