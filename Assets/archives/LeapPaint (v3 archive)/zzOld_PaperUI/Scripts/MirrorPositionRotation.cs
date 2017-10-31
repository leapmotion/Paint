using UnityEngine;
using System.Collections;

public class MirrorPositionRotation : MonoBehaviour {

  public Transform leftPalm;
  public Transform rightPalm;
  public Transform curPalm;
  public Transform[] anchors;

  protected void Update() {
    if (Input.GetKeyDown(KeyCode.Space)) {
      MirrorAnchors();
    }
  }

  private void MirrorAnchors() {
    Transform origPalm, newPalm;
    if (curPalm == leftPalm) {
      origPalm = leftPalm;
      newPalm = rightPalm;
    }
    else {
      origPalm = rightPalm;
      newPalm = leftPalm;
    }
    for (int i = 0; i < anchors.Length; i++) {
      Transform anchor = anchors[i];
      anchor.position = origPalm.InverseTransformPoint(anchor.position);
      anchor.position = new Vector3(anchor.position.x * -1F, anchor.position.y, anchor.position.z);
      anchor.position = newPalm.TransformPoint(anchor.position);

      anchor.rotation = Quaternion.Inverse(origPalm.rotation) * anchor.rotation;
      anchor.rotation = new Quaternion(anchor.rotation.x * -1F, anchor.rotation.y, anchor.rotation.z, anchor.rotation.w * -1F);
      anchor.rotation = newPalm.rotation * anchor.rotation;

      anchor.parent = newPalm;
    }
    curPalm = newPalm;
  }

}
