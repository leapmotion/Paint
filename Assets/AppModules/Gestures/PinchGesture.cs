using UnityEngine;

namespace Leap.Unity.Gestures {

  public class PinchGesture : OneHandedGesture, IPoseGesture {

    [Header("Intention System")]
    [SerializeField]
    private bool _requireIntent = true;
    
    private Pose _lastPinchPose = Pose.identity;

    public static float GetCustomPinchStrength(Hand h) {
      var i = h.GetIndex().TipPosition.ToVector3();
      var t = h.GetThumb().TipPosition.ToVector3();

      var ti = i - t;

      if (Input.GetKeyDown(KeyCode.C)) {
        Debug.Log(ti.magnitude);
      }

      return ti.magnitude.MapUnclamped(0.0168f, 0.08f, 1f, 0f);
    }

    #region OneHandedGesture

    [Header("Debug")]
    public Transform renderPinchVelocityBar;
    public bool _drawDebug = false;

    private DeltaFloatBuffer pinchStrengthBuffer = new DeltaFloatBuffer(10);

    private const int MIN_REACTIVATE_TIME = 5;
    private int minReactivateTimer = 0;

    private const int MIN_REACTIVATE_TIME_SINCE_DEGENERATE_CONDITIONS = 6;
    private int minReactivateSinceDegenerateConditionsTimer = 0;

    protected override bool ShouldGestureActivate(Hand hand) {
      bool shouldActivate = false;

      if (minReactivateTimer > MIN_REACTIVATE_TIME) {

        if (minReactivateSinceDegenerateConditionsTimer
            > MIN_REACTIVATE_TIME_SINCE_DEGENERATE_CONDITIONS) {
          var latestPinchStrength = GetCustomPinchStrength(hand);
          

          pinchStrengthBuffer.Add(latestPinchStrength, Time.time);

          if (pinchStrengthBuffer.IsFull) {
            var pinchStrengthVelocity = pinchStrengthBuffer.Delta();

            if (Input.GetKeyDown("v")) {
              Debug.Log(pinchStrengthVelocity);
            }

            var handFOVAngle = Vector3.Angle(Camera.main.transform.forward,
            hand.PalmPosition.ToVector3() - Camera.main.transform.position);
            var handWithinFOV = handFOVAngle < Camera.main.fieldOfView / 2.2f;

            RuntimeGizmos.BarGizmo.Render(pinchStrengthVelocity, renderPinchVelocityBar, Color.red, 0.02f);

            if (pinchStrengthVelocity > 6f
                && latestPinchStrength > 1.0f
                && handWithinFOV) {
              shouldActivate = true;
              DebugPing.Ping(hand.GetPredictedPinchPosition(), Color.red, 0.20f);
              DebugPing.Ping(renderPinchVelocityBar.position, Color.white, 2.00f);
            }
          }
        }
        else {
          minReactivateSinceDegenerateConditionsTimer += 1;
        }

      }
      else {
        minReactivateTimer += 1;
      }

      if (shouldActivate) {
        minDeactivateTimer = 0;
      }

      return shouldActivate;
    }

    private const int MIN_DEACTIVATE_TIME = 5;
    private int minDeactivateTimer = 0;

    protected override bool ShouldGestureDeactivate(Hand hand,
                                                    out DeactivationReason?
                                                      deactivationReason) {
      deactivationReason = DeactivationReason.FinishedGesture;

      bool shouldDeactivate = false;

      if (minDeactivateTimer > MIN_DEACTIVATE_TIME) {
        var pinchStrength = GetCustomPinchStrength(hand);

        if (pinchStrength < 0.4f) {
          shouldDeactivate = true;

          DebugPing.Ping(hand.GetPredictedPinchPosition(), Color.black, 0.20f);
        }
      }
      else {
        minDeactivateTimer++;
      }

      if (shouldDeactivate) {
        minReactivateTimer = 0;
      }

      return shouldDeactivate;
    }

    protected override void WhileGestureActive(Hand hand) {
      if (_drawDebug) {
        DebugPing.Ping(hand.GetPredictedPinchPosition(), LeapColor.amber, 0.05f);
      }
    }

    protected override void WhenGestureDeactivated(Hand maybeNullHand,
                                                   DeactivationReason reason) {
      pinchStrengthBuffer.Clear();
    }

    protected override void WhileHandTracked(Hand hand) {
      _lastPinchPose = new Pose() {
        position = hand.GetPredictedPinchPosition(),
        rotation = hand.Rotation.ToQuaternion()
      };
      
      var lookingDownWrist = Vector3.Angle(hand.DistalAxis(),
         hand.PalmPosition.ToVector3() - Camera.main.transform.position) < 25f;
      if (lookingDownWrist) {
        DebugPing.Ping(hand.WristPosition.ToVector3(), Color.black, 0.10f);
        minReactivateSinceDegenerateConditionsTimer = 0;
      }
    }

    #endregion

    #region IPoseGesture

    public Pose currentPose {
      get {
        return _lastPinchPose;
      }
    }

    public bool wasActivated {
      get {
        return _wasGestureActivated;
      }
    }

    public bool isActive {
      get {
        return isGestureActive;
      }
    }

    public bool wasDeactivated {
      get {
        return _wasGestureDeactivated;
      }
    }

    #endregion

  }

}