using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(SphereCollider))]
public class QueryableTrigger : MonoBehaviour {

  #region PUBLIC FIELDS

  [Tooltip("This will override the sphere collider's radius before querying.")]
  public float sphereColliderRadius = 1F;

  #endregion

  #region PRIVATE FIELDS

  private SphereCollider _collider;
  private HashSet<GameObject> _collidingObjs = new HashSet<GameObject>();

  private bool _warnedAboutTriggerState = false;

  #endregion

  #region UNITY CALLBACKS

  protected void Start() {
    _collider = GetComponent<SphereCollider>();
    _collider.isTrigger = true;
  }

  protected void Update() {
    if (!_collider.isTrigger && !_warnedAboutTriggerState) {
      _warnedAboutTriggerState = true;
    }
  }

  protected void OnTriggerEnter(Collider other) {
    if (!_collidingObjs.Contains(other.gameObject)) {
      _collidingObjs.Add(other.gameObject);
    }
  }

  protected void OnTriggerExit(Collider other) {
    if (_collidingObjs.Contains(other.gameObject)) {
      _collidingObjs.Remove(other.gameObject);
    }
  }

  #endregion

  #region PUBLIC METHODS

  /// <summary> O(N) with the number of GameObjects currently colliding with the QueryableTrigger. </summary>
  public T Query<T>() where T : Component {
    _collider.radius = sphereColliderRadius;
    foreach (GameObject obj in _collidingObjs) {
      T queryComponent = obj.GetComponentInParent<T>();
      if (queryComponent != null) {
        return queryComponent;
      }
    }
    return null;
  }

  #endregion

}
