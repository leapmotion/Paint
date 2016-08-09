using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Collider))]
public class QueryableTrigger : MonoBehaviour {

  #region PRIVATE FIELDS

  private Collider _collider;
  private HashSet<GameObject> _collidingObjs = new HashSet<GameObject>();

  private bool _warnedAboutTriggerState = false;

  #endregion

  #region UNITY CALLBACKS

  protected void Start() {
    _collider = GetComponent<Collider>();
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
