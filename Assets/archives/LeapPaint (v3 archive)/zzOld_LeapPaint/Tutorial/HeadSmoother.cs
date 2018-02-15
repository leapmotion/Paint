using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Recording;

[ExecuteInEditMode]
public class HeadSmoother : MonoBehaviour {

  public DiscontinuityCalculator discontinuity;
  public Transform src;

  [Header("Smoothing Settings")]
  public bool performSmoothing = true;
  public float returnStrengthMult = 1;
  public AnimationCurve positionReturnStrength;
  public AnimationCurve rotationReturnStrength;

  private Vector3 _offsetPosition;
  private Quaternion _offsetRotation;

  private void OnEnable() {
    discontinuity.OnUpdate += onUpdateDiscontiuity;
  }

  private void OnDisable() {
    discontinuity.OnUpdate -= onUpdateDiscontiuity;
  }

#if UNITY_EDITOR
  private void Update() {
    if (!Application.isPlaying) {
      transform.position = src.position;
      transform.rotation = src.rotation;
      return;
    }
  }
#endif

  private void onUpdateDiscontiuity(bool isDiscontiuity) {
    if (isDiscontiuity) {
      _offsetPosition = src.position.To(transform.position);
      _offsetRotation = src.rotation.To(transform.rotation);
    }

    if (!performSmoothing) {
      _offsetPosition = Vector3.zero;
      _offsetRotation = Quaternion.identity;
    }

    transform.position = src.position.Then(_offsetPosition);
    transform.rotation = src.rotation.Then(_offsetRotation);

    float positionReturn = returnStrengthMult * positionReturnStrength.Evaluate(_offsetPosition.magnitude);
    float rotationReturn = returnStrengthMult * rotationReturnStrength.Evaluate(Quaternion.Angle(_offsetRotation, Quaternion.identity));

    _offsetPosition = Vector3.Lerp(_offsetPosition, Vector3.zero, positionReturn);
    _offsetRotation = Quaternion.Lerp(_offsetRotation, Quaternion.identity, rotationReturn);
  }
}
