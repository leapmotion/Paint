using UnityEngine;
using System.Collections.Generic;
using Leap.Unity.RuntimeGizmos;

namespace Leap.Unity.Interaction {

  public class SwipeHand : IHandModel, IRuntimeGizmoComponent {
    public float SwipeDistanceThreshold = 0.03f;

    [HideInInspector]
    public float Distance, SwipePosition, SwipeVelocity;
#pragma warning disable 0414
    private float Upness, prevSwipePosition;
#pragma warning restore 0414
    Vector3 ThumbBase, ThumbTip, IndexBase, IndexTip, displacement, closestIndexPoint;

    bool pressing, prevPressing = false;
    void Update() {
      if (gameObject.activeInHierarchy && _hand != null) {
        transform.position = _hand.PalmPosition.ToVector3();
        transform.rotation = _hand.Rotation.ToQuaternion();

        ThumbBase = _hand.Fingers[0].bones[3].PrevJoint.ToVector3();
        ThumbTip = _hand.Fingers[0].bones[3].NextJoint.ToVector3();

        IndexTip = _hand.Fingers[1].bones[3].NextJoint.ToVector3();
        IndexBase = Vector3.Lerp(IndexTip, _hand.Fingers[1].bones[1].PrevJoint.ToVector3(), 0.6f);

        displacement = segment2SegmentDisplacement(IndexBase, IndexTip, ThumbBase, ThumbTip, out SwipePosition, out Upness);
        closestIndexPoint = Vector3.Lerp(IndexBase, IndexTip, SwipePosition);
        SwipePosition *= (_hand.IsLeft ? -1f : 1f);
        Distance = displacement.magnitude;

        float primaryFingerCurl =
          Vector3.Dot(_hand.Direction.ToVector3(), _hand.Fingers[2].Direction.ToVector3()) +
          Vector3.Dot(_hand.Direction.ToVector3(), _hand.Fingers[3].Direction.ToVector3()) +
          Vector3.Dot(_hand.Direction.ToVector3(), _hand.Fingers[4].Direction.ToVector3());

        pressing = Distance < SwipeDistanceThreshold &&
          Vector3.Dot(Vector3.up, (_hand.IsLeft ? 1f : -1f) * transform.right) > 0.7f &&
          Vector3.Dot(_hand.Direction.ToVector3(), _hand.Fingers[1].Direction.ToVector3()) < -0.6f &&
          primaryFingerCurl < -1.5f;

        if (pressing && prevPressing) {
          SwipeVelocity = Mathf.Lerp(SwipeVelocity, SwipePosition - prevSwipePosition, 0.5f);
        }
        else if (!pressing) {
          SwipePosition = 0f;
          SwipeVelocity = 0f;
        }

        prevPressing = pressing;
        prevSwipePosition = SwipePosition;
      }
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (gameObject.activeInHierarchy && _hand != null) {
        drawer.color = Color.blue;
        drawer.DrawLine(ThumbBase, ThumbTip);
        drawer.color = Color.yellow;
        drawer.DrawLine(IndexBase, IndexTip);
        drawer.color = Color.red;
        drawer.DrawLine(closestIndexPoint, closestIndexPoint - displacement);

        drawer.RelativeTo(transform);

        if (pressing) {
          drawer.color = SwipeVelocity > 0f ? Color.red : Color.blue;
          drawer.DrawSphere(new Vector3(0f, (-SwipePosition / 5f) * (_hand.IsLeft ? -1f : 1f), 0.1f), 0.01f);
          drawer.DrawCube(new Vector3(0f, -0.1f, 0.15f), new Vector3(0.01f, Mathf.Abs(SwipeVelocity), 0.01f));
          //Upness is a shitty signal right now, so I'm not drawing it
        }
      }
    }

    //LINE SEGMENT <-> LINE SEGMENT ANALYSIS
    //-------------------------------------------------------------------
    public static Vector3 segment2SegmentDisplacement(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2, out float timeToa2, out float timeTob2) {
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


    //HAND CENTRIC FILLER STUFF
    //-------------------------------------------------------------------
    private Hand _hand;

    /** The model type. An InteractionBrushHand is a type of physics model. */
    public override ModelType HandModelType {
      get { return ModelType.Graphics; }
    }

    [SerializeField]
    private Chirality handedness;
    /** Whether this model can be used to represent a right or a left hand.*/
    public override Chirality Handedness {
      get { return handedness; }
      set { handedness = value; }
    }

    /** Gets the Leap.Hand object whose data is used to update this model. */
    public override Hand GetLeapHand() { return _hand; }
    /** Sets the Leap.Hand object to use to update this model. */
    public override void SetLeapHand(Hand hand) { _hand = hand; }

    public void Awake() {
      //gameObject.SetActive(true);
    }

    /** Initializes this hand model. */
    public override void InitHand() {
      base.InitHand();
      //gameObject.SetActive(false);
    }

    /** Updates this hand model. */
    public override void UpdateHand() {
#if UNITY_EDITOR
      if (!Application.isPlaying) {
        return;
      }
#endif
    }

    public override void BeginHand() {
      base.BeginHand();
      gameObject.SetActive(true);
    }

    public override void FinishHand() {
      gameObject.SetActive(false);
      base.FinishHand();
    }
  }
}