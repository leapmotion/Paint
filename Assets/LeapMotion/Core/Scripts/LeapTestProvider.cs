using Leap.Unity.Attributes;
using Leap.Unity.Query;
using Leap.Unity.Recording;
using System.IO;
using UnityEngine;

namespace Leap.Unity {

  [ExecuteInEditMode]
  public class LeapTestProvider : LeapProvider {

    #region Inspector

    [Header("Runtime Basis Transforms")]

    [Tooltip("At runtime, if this Transform is non-null, the LeapTestProvider will "
           + "create a test-pose left hand at this transform's position and rotation."
           + "Setting this binding to null at runtime will cause the hand to disappear "
           + "from Frame data, as if it stopped tracking.")]
    public Transform leftHandBasis;
    private Hand _leftHand = null;
    private Hand _cachedLeftHand = null;

    [Tooltip("At runtime, if this Transform is non-null, the LeapTestProvider will "
           + "create a test-pose right hand at this transform's position and rotation."
           + "Setting this binding to null at runtime will cause the hand to disappear "
           + "from Frame data, as if it stopped tracking.")]
    public Transform rightHandBasis;
    private Hand _rightHand = null;
    private Hand _cachedRightHand = null;

    #endregion

    #region LeapProvider Implementation

    public Frame frame;
    public override Frame CurrentFrame { get { return frame; } }
    public override Frame CurrentFixedFrame { get { return frame; } }

    #endregion
    
    void Awake() {
      _cachedLeftHand  = TestHandFactory.MakeTestHand(isLeft: true,
                                           unitType: TestHandFactory.UnitType.UnityUnits);
      _cachedRightHand = TestHandFactory.MakeTestHand(isLeft: false,
                                           unitType: TestHandFactory.UnitType.UnityUnits);
    }

    void Update() {
      if (_leftHand == null && leftHandBasis != null) {
        _leftHand = _cachedLeftHand;
        frame.Hands.Add(_leftHand);
      }
      if (_leftHand != null && leftHandBasis == null) {
        frame.Hands.Remove(_leftHand);
        _leftHand = null;
      }
      if (_leftHand != null) {
        _leftHand.SetTransform(leftHandBasis.position, leftHandBasis.rotation);
      }

      if (_rightHand == null && rightHandBasis != null) {
        _rightHand = _cachedRightHand;
        frame.Hands.Add(_rightHand);
      }
      if (_rightHand != null && rightHandBasis == null) {
        frame.Hands.Remove(_rightHand);
        _rightHand = null;
      }
      if (_rightHand != null) {
        _rightHand.SetTransform(rightHandBasis.position, rightHandBasis.rotation);
      }

      if (Application.isPlaying) {
        DispatchUpdateFrameEvent(frame);
      }

      // Test Pose
      updateTestPose();
    }

    void FixedUpdate() {
      if (Application.isPlaying) {
        DispatchFixedFrameEvent(frame);
      }
    }

    #region Test Pose

    #region Inspector

    [Header("Test Pose")]

    public TestPoseMode testPoseMode;
    public enum TestPoseMode { EditTimePose, CapturedPose }

    public StreamingFolder poseFolder;
    public string poseName;

    public bool captureModeEnabled = false;

    [DisableIf("captureModeEnabled", isEqualTo: false)]
    public LeapProvider poseCaptureSource;

    [DisableIf("captureModeEnabled", isEqualTo: false)]
    public KeyCode captureKey = KeyCode.C;

    #endregion

    #region Unity Events

    private void updateTestPose() {

      // Capturing
      if (captureModeEnabled && Input.GetKeyDown(captureKey)) {
        if (Application.isPlaying) {
          Debug.Log("Can only capture during playmode.");
        }
        else {
          if (poseCaptureSource == null) {
            Debug.Log("Null capture source; can't capture pose.");
          }
          else {
            var hand = poseCaptureSource.CurrentFrame
                                         .Hands.Query()
                                         .FirstOrDefault(h => !h.IsLeft);
            if (hand == null) {
              Debug.Log("Null hand, no capture.");
            }
            else {
              var vectorHand = new VectorHand(hand);
              //var bytes = null;
              // NEED TO GET the WIP networking module up in here!!!!
              var filePath = Path.Combine(poseFolder.Path,
                                          Path.ChangeExtension(poseName, ".vectorhand"));
              //File.WriteAllBytes(filePath, )
            }
          }
        }
      }
    }

    #endregion

    #endregion

  }

}
