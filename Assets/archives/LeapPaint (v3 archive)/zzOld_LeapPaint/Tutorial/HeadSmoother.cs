using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

[ExecuteInEditMode]
public class HeadSmoother : MonoBehaviour {

  public Transform src;
  public float positionThresh = 0.01f;
  public float rotationThresh = 10;
  public float returnStrengthMult = 1;
  public AnimationCurve positionReturnStrength;
  public AnimationCurve rotationReturnStrength;
  public int warmUpFrames = 5;

  private Vector3 _position0, _position1, _position2;
  private Quaternion _rotation0, _rotation1, _rotation2;

  private Vector3 _offsetPosition;
  private Quaternion _offsetRotation;

  private void Update() {
#if UNITY_EDITOR
    if (!Application.isPlaying) {
      transform.position = src.position;
      transform.rotation = src.rotation;
      return;
    }
#endif

    if (Time.frameCount < warmUpFrames) {
      transform.position = src.position;
      transform.rotation = src.rotation;
    }

    _position2 = _position1;
    _position1 = _position0;
    _position0 = src.position;

    _rotation2 = _rotation1;
    _rotation1 = _rotation0;
    _rotation0 = src.rotation;

    float deltaPosition21 = Vector3.Distance(_position2, _position1);
    float deltaPosition10 = Vector3.Distance(_position1, _position0);

    float deltaRotation21 = Quaternion.Angle(_rotation2, _rotation1);
    float deltaRotation10 = Quaternion.Angle(_rotation1, _rotation0);

    if (Mathf.Abs(deltaPosition10 - deltaPosition21) > positionThresh ||
        Mathf.Abs(deltaRotation10 - deltaRotation21) > rotationThresh) {
      _offsetPosition = _position0.To(transform.position);
      _offsetRotation = _rotation0.To(transform.rotation);
    }

    transform.position = src.position.Then(_offsetPosition);
    transform.rotation = src.rotation.Then(_offsetRotation);

    float positionReturn = returnStrengthMult * positionReturnStrength.Evaluate(_offsetPosition.magnitude);
    float rotationReturn = returnStrengthMult * rotationReturnStrength.Evaluate(Quaternion.Angle(_offsetRotation, Quaternion.identity));

    _offsetPosition = Vector3.Lerp(_offsetPosition, Vector3.zero, positionReturn);
    _offsetRotation = Quaternion.Lerp(_offsetRotation, Quaternion.identity, rotationReturn);
  }

}
