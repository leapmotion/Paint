using Leap.Unity.Attributes;
using Leap.Unity.Encoding;
using Leap.Unity.Query;
using Leap.Unity.Recording;
using System.IO;
using System.Text;
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

    #region Unity Events
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

    #endregion

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
              var vectorHand = Pool<VectorHand>.Spawn();
              try {
                var bytes = new byte[vectorHand.numBytesRequired];
                vectorHand.FillBytes(bytes, from: hand);

                var filePath = Path.Combine(poseFolder.Path,
                                            Path.ChangeExtension(poseName, ".vectorhand"));
                File.WriteAllBytes(filePath, bytes);

                StringBuilder sb = new StringBuilder();
                sb.Append("thumb_0: " + vectorHand.jointPositions[ 0].ToString("R") + "\n");
                sb.Append("thumb_1: " + vectorHand.jointPositions[ 1].ToString("R") + "\n");
                sb.Append("thumb_2: " + vectorHand.jointPositions[ 2].ToString("R") + "\n");
                sb.Append("thumb_3: " + vectorHand.jointPositions[ 3].ToString("R") + "\n");
                sb.Append("thumb_4: " + vectorHand.jointPositions[ 4].ToString("R") + "\n");

                sb.Append("index_0: " + vectorHand.jointPositions[ 5].ToString("R") + "\n");
                sb.Append("index_1: " + vectorHand.jointPositions[ 6].ToString("R") + "\n");
                sb.Append("index_2: " + vectorHand.jointPositions[ 7].ToString("R") + "\n");
                sb.Append("index_3: " + vectorHand.jointPositions[ 8].ToString("R") + "\n");
                sb.Append("index_4: " + vectorHand.jointPositions[ 9].ToString("R") + "\n");

                sb.Append("middle_0: " + vectorHand.jointPositions[10].ToString("R") + "\n");
                sb.Append("middle_1: " + vectorHand.jointPositions[11].ToString("R") + "\n");
                sb.Append("middle_2: " + vectorHand.jointPositions[12].ToString("R") + "\n");
                sb.Append("middle_3: " + vectorHand.jointPositions[13].ToString("R") + "\n");
                sb.Append("middle_4: " + vectorHand.jointPositions[14].ToString("R") + "\n");

                sb.Append("ring_0: " + vectorHand.jointPositions[15].ToString("R") + "\n");
                sb.Append("ring_1: " + vectorHand.jointPositions[16].ToString("R") + "\n");
                sb.Append("ring_2: " + vectorHand.jointPositions[17].ToString("R") + "\n");
                sb.Append("ring_3: " + vectorHand.jointPositions[18].ToString("R") + "\n");
                sb.Append("ring_4: " + vectorHand.jointPositions[19].ToString("R") + "\n");

                sb.Append("pinky_0: " + vectorHand.jointPositions[20].ToString("R") + "\n");
                sb.Append("pinky_1: " + vectorHand.jointPositions[21].ToString("R") + "\n");
                sb.Append("pinky_2: " + vectorHand.jointPositions[22].ToString("R") + "\n");
                sb.Append("pinky_3: " + vectorHand.jointPositions[23].ToString("R") + "\n");
                sb.Append("pinky_4: " + vectorHand.jointPositions[24].ToString("R") + "\n");

                sb.Append("palm: " + hand.WristPosition.ToVector3().From(hand.PalmPosition.ToVector3()).ToString("R") + "\n");

                var textPath = Path.Combine(poseFolder.Path,
                                            Path.ChangeExtension(poseName, ".vhtextdesc"));
                File.WriteAllText(textPath, sb.ToString());
              }
              finally {
                Pool<VectorHand>.Recycle(vectorHand);
              }
            }
          }
        }
      }
    }

    #endregion

    #endregion

  }

}
