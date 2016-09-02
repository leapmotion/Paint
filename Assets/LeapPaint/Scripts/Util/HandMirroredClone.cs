using UnityEngine;
using System.Collections;

public class HandMirroredClone : MonoBehaviour {

  [Tooltip("The object to be cloned.")]
  public GameObject _targetObject;
  [Tooltip("The effective origin of the target object for mirroring purposes.")]
  public Transform _targetRelativeRoot;

  public Transform _cloneParent;
  [Tooltip("If this field is null, a new object will be instantiated. Otherwise the given GameObject will be replaced with any rebuilt clone.")]
  public GameObject _mirroredCloneObject;
  [Tooltip("The cloned object will have the same relative position to this Transform as the target object has to its own target relative root, mirrored.")]
  public Transform _mirroredCloneRelativeRoot;

  /// <summary>
  /// Clones the target GameObject into the _mirroredCloneObject,
  /// 
  /// </summary>
  public void MirrorCloneUsingYZPlane() {
    // Create clone
    DestroyImmediate(_mirroredCloneObject);
    _mirroredCloneObject = Instantiate(_targetObject, _cloneParent) as GameObject;

    // Mirror position
    Vector3 relativePosition = _targetRelativeRoot.transform.position - _targetObject.transform.position;
    Vector3 clonePosition = _mirroredCloneRelativeRoot.transform.position + new Vector3(relativePosition.x, -relativePosition.y, -relativePosition.z);
    _mirroredCloneObject.transform.position = clonePosition;

    // Mirror rotation
    // See: http://stackoverflow.com/a/33999726
    Quaternion targetRotation = _targetObject.transform.rotation;
    Quaternion cloneRotation = new Quaternion(targetRotation.x, -targetRotation.y, -targetRotation.z, targetRotation.w);
    _mirroredCloneObject.transform.rotation = cloneRotation;
  }

}
