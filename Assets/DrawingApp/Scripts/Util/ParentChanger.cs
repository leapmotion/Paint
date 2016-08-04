using UnityEngine;
using System.Collections;

public class ParentChanger : MonoBehaviour {

  public void ChangeParentTo(Transform newParent) {
    this.transform.parent = newParent;
  }

}
