using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity {

  [ExecuteInEditMode]
  public class ShaderUniformHandSupport : MonoBehaviour {

    private const int NUM_FINGERS = 5;

    public const string LEFT_FINGERTIPS_UNIFORM_NAME = "_Leap_LH_Fingertips";
    public const string RIGHT_FINGERTIPS_UNIFORM_NAME = "_Leap_RH_Fingertips";

    [SerializeField, Disable]
    private int _leftFingertipsUniformParamId = 0;
    [SerializeField, Disable]
    private int _rightFingertipsUniformParamId = 0;

    private Vector4[] _leftFingertips = new Vector4[5];
    private Vector4[] _rightFingertips = new Vector4[5];

    private void OnValidate() {
      refreshParamIDs();

      uploadEditTimeFingerAtTransform();
    }
    private void Start() {
      refreshParamIDs();
    }

    private void refreshParamIDs() {
      _leftFingertipsUniformParamId = Shader.PropertyToID(LEFT_FINGERTIPS_UNIFORM_NAME);
      _rightFingertipsUniformParamId = Shader.PropertyToID(RIGHT_FINGERTIPS_UNIFORM_NAME);
    }

    private void uploadEditTimeFingerAtTransform() {
      _leftFingertips[0] = this.transform.position.WithW(1);
      _rightFingertips[0] = this.transform.position.WithW(1);
      Shader.SetGlobalVectorArray(_leftFingertipsUniformParamId, _leftFingertips);
      Shader.SetGlobalVectorArray(_rightFingertipsUniformParamId, _rightFingertips);
    }

    private void Update() {

      var leftHand = Hands.Left;
      if (leftHand != null) {
        for (int i = 0; i < NUM_FINGERS; i++) {
          _leftFingertips[i] = leftHand.Fingers[i].TipPosition.ToVector3().WithW(1);
        }
      }
      else {
        for (int i = 0; i < NUM_FINGERS; i++) {
          _leftFingertips[i] = (Vector3.one * 100000000f).WithW(1);
        }
      }
      Shader.SetGlobalVectorArray(_leftFingertipsUniformParamId, _leftFingertips);

      var rightHand = Hands.Right;
      if (rightHand != null) {
        for (int i = 0; i < NUM_FINGERS; i++) {
          _rightFingertips[i] = rightHand.Fingers[i].TipPosition.ToVector3().WithW(1);
        }
      }
      else {
        for (int i = 0; i < NUM_FINGERS; i++) {
          _rightFingertips[i] = (Vector3.one * 100000000f).WithW(1);
        }
      }
      Shader.SetGlobalVectorArray(_rightFingertipsUniformParamId, _rightFingertips);

      if (!Application.isPlaying) {
        uploadEditTimeFingerAtTransform();
      }
    }

  }

  public static class Vector4Extensions {

    public static Vector4 WithW(this Vector3 v, float w) {
      return new Vector4(v.x, v.y, v.z, w);
    }

  }

}
