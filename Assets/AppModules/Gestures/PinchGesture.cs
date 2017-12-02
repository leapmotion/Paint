using UnityEngine;

namespace Leap.Unity.Gestures {

  public class PinchGesture : OneHandedGesture, IPoseGesture {

    // TODO: Incorporate intention system for exclusivity
    //[Header("Intention System")]
    //[SerializeField]
    //private bool _requireIntent = true;
    
    private Pose _lastPinchPose = Pose.identity;

    public static float GetCustomPinchStrength(Hand h) {
      var indexDistal = h.GetIndex().bones[3].PrevJoint.ToVector3();
      var indexTip = h.GetIndex().TipPosition.ToVector3();
      var thumbDistal = h.GetThumb().bones[3].PrevJoint.ToVector3();
      var thumbTip = h.GetThumb().TipPosition.ToVector3();

      //var pinchDistance = (indexTip - thumbTip).magnitude;
      var pinchDistance = Segment2SegmentDisplacement(indexDistal, indexTip,
                                                      thumbDistal, thumbTip).magnitude;

      if (Input.GetKeyDown(KeyCode.C)) {
        Debug.Log(pinchDistance);
      }

      return pinchDistance.MapUnclamped(0.0168f, 0.08f, 1f, 0f);
    }

    #region Segment-to-Segment Displacement (John S)

    public static Vector3 Segment2SegmentDisplacement(Vector3 a1, Vector3 a2,
                                                      Vector3 b1, Vector3 b2) {
      float outTimeToA2 = 0f, outTimeToB2 = 0f;
      return Segment2SegmentDisplacement(a1, a2, b1, b2, out outTimeToA2, out outTimeToB2);
    }

    public static Vector3 Segment2SegmentDisplacement(Vector3 a1, Vector3 a2,
                                                      Vector3 b1, Vector3 b2,
                                                      out float timeToa2,
                                                      out float timeTob2) {
      Vector3 u = a2 - a1; //from a1 to a2
      Vector3 v = b2 - b1; //from b1 to b2
      Vector3 w = a1 - b1;
      float a = Vector3.Dot(u, u);         // always >= 0
      float b = Vector3.Dot(u, v);
      float c = Vector3.Dot(v, v);         // always >= 0
      float d = Vector3.Dot(u, w);
      float e = Vector3.Dot(v, w);
      float D = a * c - b * b;        // always >= 0
      float sc, sN, sD = D;       // sc = sN / sD, default sD = D >= 0
      float tc, tN, tD = D;       // tc = tN / tD, default tD = D >= 0

      // compute the line parameters of the two closest points
      if (D < Mathf.Epsilon) { // the lines are almost parallel
        sN = 0.0f;         // force using point P0 on segment S1
        sD = 1.0f;         // to prevent possible division by 0.0 later
        tN = e;
        tD = c;
      }
      else {                 // get the closest points on the infinite lines
        sN = (b * e - c * d);
        tN = (a * e - b * d);
        if (sN < 0.0f) {        // sc < 0 => the s=0 edge is visible
          sN = 0.0f;
          tN = e;
          tD = c;
        }
        else if (sN > sD) {  // sc > 1  => the s=1 edge is visible
          sN = sD;
          tN = e + b;
          tD = c;
        }
      }

      if (tN < 0.0) {            // tc < 0 => the t=0 edge is visible
        tN = 0.0f;
        // recompute sc for this edge
        if (-d < 0.0)
          sN = 0.0f;
        else if (-d > a)
          sN = sD;
        else {
          sN = -d;
          sD = a;
        }
      }
      else if (tN > tD) {      // tc > 1  => the t=1 edge is visible
        tN = tD;
        // recompute sc for this edge
        if ((-d + b) < 0.0)
          sN = 0;
        else if ((-d + b) > a)
          sN = sD;
        else {
          sN = (-d + b);
          sD = a;
        }
      }
      // finally do the division to get sc and tc
      sc = (Mathf.Abs(sN) < Mathf.Epsilon ? 0.0f : sN / sD);
      tc = (Mathf.Abs(tN) < Mathf.Epsilon ? 0.0f : tN / tD);

      // get the difference of the two closest points
      Vector3 dP = w + (sc * u) - (tc * v);  // =  S1(sc) - S2(tc)
      timeToa2 = sc; timeTob2 = tc;
      return dP;   // return the closest distance
    }

    #endregion

    #region OneHandedGesture

    [Header("Debug")]
    public bool _drawDebug = false;
    public bool _drawDebugPath = false;

    private DeltaFloatBuffer pinchStrengthBuffer = new DeltaFloatBuffer(5);

    private DeltaBuffer handPositionBuffer = new DeltaBuffer(5);

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
          handPositionBuffer.Add(hand.PalmPosition.ToVector3(), Time.time);

          if (pinchStrengthBuffer.IsFull) {
            var pinchStrengthVelocity = pinchStrengthBuffer.Delta();

            var handFOVAngle = Vector3.Angle(Camera.main.transform.forward,
            hand.PalmPosition.ToVector3() - Camera.main.transform.position);
            var handWithinFOV = handFOVAngle < Camera.main.fieldOfView / 2.2f;

            if (_drawDebug) {
              RuntimeGizmos.BarGizmo.Render(pinchStrengthVelocity,
                Camera.main.transform.position
                + Camera.main.transform.forward * 1f, Vector3.up, Color.red, 0.02f);
            }

            var pinchActivateVelocity = 3.5f;
            var handVelocity = handPositionBuffer.Delta();

            pinchActivateVelocity = handVelocity.magnitude.Map(0f, 2f, 2f, 10f);

            if (pinchStrengthVelocity > pinchActivateVelocity
                && latestPinchStrength > 1.0f
                && handWithinFOV) {
              shouldActivate = true;
              if (_drawDebug) {
                DebugPing.Ping(hand.GetPredictedPinchPosition(), Color.red, 0.20f);
              }
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

          if (_drawDebug) {
            DebugPing.Ping(hand.GetPredictedPinchPosition(), Color.black, 0.20f);
          }
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
      if (_drawDebugPath) {
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
        if (_drawDebug) {
          DebugPing.Ping(hand.WristPosition.ToVector3(), Color.black, 0.10f);
        }
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

    #endregion

  }

}