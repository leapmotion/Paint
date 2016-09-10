using UnityEngine;
using System.Collections;

public static class MirrorUtil {

  public static Vector3 GetMirroredPosition(Vector3 positionToMirror, Transform mirrorAnchor, Transform newAnchor) {
    positionToMirror = mirrorAnchor.InverseTransformPoint(positionToMirror);
    positionToMirror = new Vector3(positionToMirror.x * -1, positionToMirror.y, positionToMirror.z);
    positionToMirror = newAnchor.TransformPoint(positionToMirror);
    return positionToMirror;
  }

  public static Vector3 GetMirroredPosition(Vector3 positionToMirror, Transform mirrorAnchor) {
    return GetMirroredPosition(positionToMirror, mirrorAnchor, mirrorAnchor);
  }

  public static Quaternion GetMirroredRotation(Quaternion rotationToMirror, Transform mirrorAnchor, Transform newAnchor) {
    rotationToMirror = Quaternion.Inverse(mirrorAnchor.rotation) * rotationToMirror;
    rotationToMirror = new Quaternion(rotationToMirror.x * -1F, rotationToMirror.y, rotationToMirror.z, rotationToMirror.w * -1F);
    rotationToMirror = newAnchor.rotation * rotationToMirror;
    return rotationToMirror;
  }

  public static Quaternion GetMirroredRotation(Quaternion rotationToMirror, Transform mirrorAnchor) {
    return GetMirroredRotation(rotationToMirror, mirrorAnchor, mirrorAnchor);
  }

}
