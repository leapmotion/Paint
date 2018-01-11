using UnityEngine;

namespace Leap.Unity.Gestures {

  public class PinchGesture : OneHandedGesture, IPoseGesture {

    // TODO: Incorporate intention system for exclusivity
    //[Header("Intention System")]
    //[SerializeField]
    //private bool _requireIntent = true;

    #region Inspector

    public bool requireSafetyPinch = false;

    [Tooltip("Higher = pinky must be opened further out to begin a pinch")]
    [Range(0f, 1f)]
    public float maxPinkyCurl = 0.2f;

    [Tooltip("Higher = index must curl faster relative to pinky curl velocity to pinch")]
    [Range(-1f, 7f)]
    public float minIndexMinusPinkyCurlVel = 1.5f;

    [Range(0f, 5f)]
    public float minIndexCurlVel = 0.5f;

    [Range(0f, 1f)]
    public float minSafetyProduct = 0.50f;

    [Range(0f, 2f)]
    public float activationVelocityMult = 1f;

    [Header("Pinky Feedback")]
    public Color activeColor = LeapColor.lime;
    public Color readyColor = Color.Lerp(LeapColor.lime, LeapColor.red, 0.3f);
    public Color inactiveColor = LeapColor.red;
    public Material feedbackMaterial = null;
    
    private Pose _lastPinchPose = Pose.identity;

    #endregion

    #region Custom Pinch Strength

    public static Vector3 PinchSegment2SegmentDisplacement(Hand h) {
      var indexDistal = h.GetIndex().bones[3].PrevJoint.ToVector3();
      var indexTip = h.GetIndex().TipPosition.ToVector3();
      var thumbDistal = h.GetThumb().bones[3].PrevJoint.ToVector3();
      var thumbTip = h.GetThumb().TipPosition.ToVector3();

      return Segment2SegmentDisplacement(indexDistal, indexTip, thumbDistal, thumbTip);
    }

    public static float GetCustomPinchStrength(Hand h) {
      var pinchDistance = PinchSegment2SegmentDisplacement(h).magnitude;

      pinchDistance -= 0.04f;
      pinchDistance = pinchDistance.Clamped01();

      if (Input.GetKeyDown(KeyCode.C)) {
        Debug.Log(pinchDistance);
      }

      return pinchDistance.MapUnclamped(0.0168f, 0.08f, 1f, 0f);
    }

    #endregion

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

    #region Safety Pinch

    //private float _middleSafetyAmt = 0f;
    //private float _ringSafetyAmt   = 0f;
    private float _pinkySafetyAmt  = 0f;

    //private float _safetySum = 0f;

    private void updateSafetyPinch(Hand hand) {
      var knuckleDir = hand.DistalAxis();

      //_middleSafetyAmt = hand.GetMiddle().bones[3].Direction
      //                     .Dot(knuckleDir.ToVector()).Map(0f, 1f, 0f, 1f);
      //_ringSafetyAmt = hand.GetRing().bones[3].Direction
      //                     .Dot(knuckleDir.ToVector()).Map(0f, 1f, 0f, 1f);
      _pinkySafetyAmt = hand.GetPinky().bones[1].Direction
                           .Dot(knuckleDir.ToVector()).Map(0f, 1f, 0f, 1f);

      if (hand.GetPinky().bones[1].Direction.ToVector3().Dot(-hand.PalmarAxis()) > 0f) {
        _pinkySafetyAmt = 1f;
      }

      //_safetySum = _middleSafetyAmt + _ringSafetyAmt + _pinkySafetyAmt;
    }

    private bool isSafetyActivationSatisfied() {
      return _pinkySafetyAmt > minSafetyProduct;
    }

    #endregion

    #region Index & Pinky Curl

    private DeltaFloatBuffer _pinkyCurlBuffer = new DeltaFloatBuffer(5);
    private DeltaFloatBuffer _indexCurlBuffer = new DeltaFloatBuffer(5);

    private void updatePinkyCurl(Hand h) {
      var pinky = h.GetPinky();
      var pinkyCurl = getCurl(h, pinky);

      _pinkyCurlBuffer.Add(pinkyCurl, Time.time);
    }

    private void updateIndexCurl(Hand h) {
      var index = h.GetIndex();
      var indexCurl = getCurl(h, index);

      _indexCurlBuffer.Add(indexCurl, Time.time);
    }

    private float getCurl(Hand h, Finger f) {
      return (getBaseCurl(h, f) + getGripCurl(h, f)) / 2f;
    }

    private float getBaseCurl(Hand h, Finger f) {
      var palmAxis = h.PalmarAxis();
      var leftPositiveThumbAxis = h.RadialAxis() * (h.IsLeft ? 1f : -1f);
      int baseBoneIdx = 1;
      if (f.Type == Finger.FingerType.TYPE_THUMB) baseBoneIdx = 2;
      var baseCurl = f.bones[baseBoneIdx].Direction.ToVector3()
                      .SignedAngle(palmAxis, leftPositiveThumbAxis)
                      .Map(0f, 90f, 1f, 0f);

      return baseCurl;
    }

    private float getGripCurl(Hand h, Finger f) {
      var leftPositiveThumbAxis = h.RadialAxis() * (h.IsLeft ? 1f : -1f);
      int baseBoneIdx = 1;
      if (f.Type == Finger.FingerType.TYPE_THUMB) baseBoneIdx = 2;
      var baseDir = f.bones[baseBoneIdx].Direction.ToVector3();

      var gripAngle = baseDir.SignedAngle(
                                f.bones[3].Direction.ToVector3(),
                                leftPositiveThumbAxis);

      if (gripAngle < -30f) {
        gripAngle += 360f;
      }

      var gripCurl = gripAngle.Map(0f, 150f, 0f, 1f);
      return gripCurl;
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

      updateSafetyPinch(hand);

      updatePinkyCurl(hand);
      updateIndexCurl(hand);

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

            pinchActivateVelocity = handVelocity.magnitude.Map(0f, 2f, 1.5f, 8f);

            var pinchDist = PinchSegment2SegmentDisplacement(hand).magnitude;

            pinchActivateVelocity *= pinchDist.Map(0f, 0.02f, 0f, 1f);

            pinchActivateVelocity *= activationVelocityMult;


            var pinkyCurlSample = _pinkyCurlBuffer.GetLatest();
            if (_pinkyCurlBuffer.IsFull) {
              //var pinkyCurlVelocity = _pinkyCurlBuffer.Delta();
            }

            var indexMinusPinkyCurlVel = 10f;
            var indexCurlVel = 10f;
            if (_pinkyCurlBuffer.IsFull && _indexCurlBuffer.IsFull) {
              var pinkyCurlVel = _pinkyCurlBuffer.Delta();
              indexCurlVel = _indexCurlBuffer.Delta();

              indexMinusPinkyCurlVel = indexCurlVel - pinkyCurlVel;
            }

            if (feedbackMaterial != null) {
              if ((pinkyCurlSample < maxPinkyCurl)
                  && (indexMinusPinkyCurlVel > minIndexMinusPinkyCurlVel)
                  && (indexCurlVel > minIndexCurlVel)) {
                feedbackMaterial.color = activeColor;
              }
              else if ((pinkyCurlSample < maxPinkyCurl)) {
                feedbackMaterial.color = readyColor;

                RuntimeGizmos.RuntimeGizmoDrawer drawer;
                if (RuntimeGizmos.RuntimeGizmoManager.TryGetGizmoDrawer(out drawer)) {
                  drawer.color = readyColor;
                  drawer.DrawWireCapsule(hand.GetThumb().TipPosition.ToVector3(),
                                         hand.GetIndex().TipPosition.ToVector3(),
                                         0.005f);
                }
              }
              else {
                feedbackMaterial.color = inactiveColor;
              }
            }

            if ((pinchStrengthVelocity > pinchActivateVelocity)
                && latestPinchStrength > 1.0f
                && (!requireSafetyPinch || isSafetyActivationSatisfied())
                && (pinkyCurlSample < maxPinkyCurl)
                && (indexMinusPinkyCurlVel > minIndexMinusPinkyCurlVel)
                && (indexCurlVel > minIndexCurlVel)
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
        var pinchDistance = PinchSegment2SegmentDisplacement(hand).magnitude;

        if (pinchDistance > 0.06f) {
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

    public Pose pose {
      get {
        return _lastPinchPose;
      }
    }

    #endregion

  }

}