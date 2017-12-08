using Leap.Unity;
using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuplicatorHelper : MonoBehaviour {

  [QuickButton("Duplicate!", "Duplicate")]
  [MinValue(1)]
  public int numWidthCopies = 1;
  [MinValue(1)]
  public int numHeightCopies = 1;

  public float horizontalSpacing = 0.10f;
  public float verticalSpacing = 0.10f;

  [QuickButton("Clear Children!", "ClearDuplicationParentChildren")]
  public Transform duplicationParent;

  public GameObject toDuplicate;

  public void Duplicate() {
    if (duplicationParent == null) {
      Debug.LogError("Can't duplicate without a duplication parent. Warning: Pre-existing objects in the "
        + "duplication parent will be destroyed.", this);
      return;
    }
    if (toDuplicate == null) {
      Debug.LogError("Can't duplicate without target GameObject toDuplicate.", this);
      return;
    }

    ClearDuplicationParentChildren();

    for (int i = 0; i < numWidthCopies; i++) {
      for (int j = 0; j < numHeightCopies; j++) {
        var position = toDuplicate.transform.position
                       + i * horizontalSpacing * duplicationParent.transform.right
                       + j * verticalSpacing * -duplicationParent.transform.up;

        GameObject duplicate = GameObject.Instantiate(toDuplicate);
        duplicate.transform.parent = duplicationParent;
        duplicate.transform.position = position;
        duplicate.transform.rotation = toDuplicate.transform.rotation;
        duplicate.transform.localScale = toDuplicate.transform.localScale;
      }
    }
  }

  public void ClearDuplicationParentChildren() {
    var numChildren = duplicationParent.childCount;
    for (int i = numChildren - 1; i >= 0; i--) {
      DestroyImmediate(duplicationParent.GetChild(i).gameObject);
    }
  }

}
