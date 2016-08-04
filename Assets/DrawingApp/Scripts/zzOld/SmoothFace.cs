using UnityEngine;
using System.Collections;

public class SmoothFace : MonoBehaviour {

  [SerializeField]
  private Transform _faceTransform;

  public void SetFaceTarget(Transform toFace) {
    _faceTransform = toFace;
  }

  protected void Update() {
    if (_faceTransform != null) {
      Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation((_faceTransform.position - this.transform.position).normalized), 0.5F);
    }
  }

}
