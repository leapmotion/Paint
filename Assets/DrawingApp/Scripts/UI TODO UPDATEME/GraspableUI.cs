using UnityEngine;
using System.Collections;

public class GraspableUI : MonoBehaviour {

  [SerializeField]
  protected Transform _returnAnchor;

  private Transform _graspedBy;

  public void GraspedBy(Transform graspAnchor) {
    _graspedBy = graspAnchor;
  }

  public void Released() {
    _graspedBy = null;
  }

  protected void Update() {
    if (_graspedBy != null) {
      this.transform.position = _graspedBy.transform.position;
      this.transform.rotation = _graspedBy.transform.rotation;
    }
    else {
      this.transform.position = _returnAnchor.transform.position;
      this.transform.rotation = _returnAnchor.transform.rotation;
    }
  }

}
