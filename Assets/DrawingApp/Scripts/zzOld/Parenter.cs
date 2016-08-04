using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Parenter : MonoBehaviour {

  [SerializeField]
  private Transform _originalParent;

  /// <summary>
  /// Remembers the old parent and re-parents the transform to the new one.
  /// </summary>
  /// <param name="newParent"></param>
  public void Parent(Transform newParent) {
    _originalParent = this.transform.parent;
    this.transform.parent = newParent;
  }

  /// <summary>
  /// Reverts back to the last parent. (Only the last one, can't go back further.)
  /// </summary>
  public void Deparent() {
    this.transform.parent = _originalParent;
  }

}
