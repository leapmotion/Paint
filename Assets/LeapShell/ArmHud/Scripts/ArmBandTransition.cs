using UnityEngine;
using System.Collections.Generic;
using Leap;
using Leap.Unity;
using Leap.Unity.Attributes;
using Leap.Unity.RuntimeGizmos;

public class ArmBandTransition : MonoBehaviour, IRuntimeGizmoComponent {

  [SerializeField]
  private LeapProvider _provider;

  [SerializeField]
  private Transform _startAnchor;

  [SerializeField]
  private Transform _endAnchor;

  [SerializeField]
  private Transform _bandAnchor;

  [SerializeField]
  private Renderer _bandRenderer;

  [Range(0, 0.1f)]
  [MinValue(0)]
  [SerializeField]
  private float _bandWidth = 1f;

  [Range(0, 1)]
  [SerializeField]
  private float _bandPercent = 0;

  [SerializeField]
  private AnimationCurve _percentCurve;

  [SerializeField]
  private MomentumSlider _bandMomentum;

  private Vector3 _bandStart, _bandCenter, _bandEnd, _direction, _armStart, _armEnd;

  public float DisplayPercent {
    get {
      return _percentCurve.Evaluate(_bandPercent);
    }
  }

  void Awake() {
    _bandMomentum.CanHandInteract = h => h.IsRight;
    _bandMomentum.HandToPosition = getHandPosition;
    _bandMomentum.HandToDistance = getHandDistance;
  }

  void OnEnable() {
    _bandMomentum.OnPosition += updatePosition;
  }

  void OnDisable() {
    _bandMomentum.OnPosition -= updatePosition;
  }

  void Update() {
    UpdateBandInfo();

    _bandMomentum.Update(_provider.CurrentFrame.Hands);
  }

  private void updatePosition(float newPosition) {
    _bandPercent = Mathf.Clamp01(newPosition);

    UpdateBandInfo();
    _bandAnchor.position = _bandCenter;
    _bandAnchor.rotation = _startAnchor.rotation;
  }

  private float getHandPosition(Hand hand) {
    for (int i = 0; i < 5; i++) {
      Finger finger = hand.Fingers[i];
      if (finger.Type == Finger.FingerType.TYPE_INDEX) {
        Bone bone = finger.Bone(Bone.BoneType.TYPE_DISTAL);
        float pointLength = Vector3.Project(bone.NextJoint.ToVector3() - _armStart, _direction.normalized).magnitude;
        return pointLength / (_direction.magnitude);
      }
    }

    return -1;
  }

  private float getHandDistance(Hand hand) {
    float radius = float.MaxValue;

    for (int i = 0; i < 5; i++) {
      Finger finger = hand.Fingers[i];

      for (int j = 0; j < 4; j++) {
        Bone bone = finger.bones[j];

        Vector3 b0 = bone.PrevJoint.ToVector3();
        Vector3 b1 = bone.NextJoint.ToVector3();

        radius = Mathf.Min(radius, distanceBetweenSegments(b0, b1, _bandStart, _bandEnd));
      }
    }

    return radius;
  }

  public void UpdateBandInfo() {
    _armStart = _startAnchor.position;
    _armEnd = _endAnchor.position;

    _direction = _armEnd - _armStart;
    float length = _direction.magnitude;

    float percentWidth = _bandWidth / length;

    float startPercent = _bandPercent - percentWidth * 0.5f;
    float endPercent = startPercent + percentWidth;

    _bandStart = _armStart + _direction * startPercent;
    _bandCenter = _armStart + _direction * _bandPercent;
    _bandEnd = _armStart + _direction * endPercent;
  }

  public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    UpdateBandInfo();

    drawer.color = new Color(0, 1, 0, _bandMomentum.IsConnected ? 1 : 0.2f);
    /*
    drawer.DrawCirlce(_bandStart, _direction, _bandMomentum.ActivationDistance);
    drawer.DrawCirlce(_bandEnd, _direction, _bandMomentum.ActivationDistance);

    drawer.color = new Color(1, 0, 0, _bandMomentum.IsConnected ? 0.2f : 1);
    drawer.DrawCirlce(_bandStart, _direction, _bandMomentum.DeactivationDistance);
    drawer.DrawCirlce(_bandEnd, _direction, _bandMomentum.DeactivationDistance);

    drawer.color = Color.white;
    drawer.DrawCirlce(_armStart, _direction, _bandMomentum.DeactivationDistance * 1.1f);
    drawer.DrawCirlce(_armEnd, _direction, _bandMomentum.DeactivationDistance * 1.1f);
    */

    drawer.color = Color.yellow;
    drawer.DrawLine(_bandCenter, _bandCenter + 0.1f * _direction.normalized * _bandMomentum.Velocity);
  }

  private static float distanceBetweenSegments(Vector3 s1p0, Vector3 s1p1, Vector3 s2p0, Vector3 s2p1) {
    Vector3 u = s1p1 - s1p0;
    Vector3 v = s2p1 - s2p0;
    Vector3 w = s1p0 - s2p0;
    float a = Vector3.Dot(u, u);
    float b = Vector3.Dot(u, v);
    float c = Vector3.Dot(v, v);
    float d = Vector3.Dot(u, w);
    float e = Vector3.Dot(v, w);
    float D = a * c - b * b;
    float sc, sN, sD = D;
    float tc, tN, tD = D;

    if (D < float.Epsilon) {
      sN = 0.0f;
      sD = 1.0f;
      tN = e;
      tD = c;
    } else {
      sN = (b * e - c * d);
      tN = (a * e - b * d);
      if (sN < 0.0f) {        // sc < 0 => the s=0 edge is visible
        sN = 0.0f;
        tN = e;
        tD = c;
      } else if (sN > sD) {  // sc > 1  => the s=1 edge is visible
        sN = sD;
        tN = e + b;
        tD = c;
      }
    }

    if (tN < 0.0f) {            // tc < 0 => the t=0 edge is visible
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
    } else if (tN > tD) {      // tc > 1  => the t=1 edge is visible
      tN = tD;
      // recompute sc for this edge
      if ((-d + b) < 0.0f)
        sN = 0.0f;
      else if ((-d + b) > a)
        sN = sD;
      else {
        sN = (-d + b);
        sD = a;
      }
    }
    // finally do the division to get sc and tc
    sc = (Mathf.Abs(sN) < float.Epsilon ? 0.0f : sN / sD);
    tc = (Mathf.Abs(tN) < float.Epsilon ? 0.0f : tN / tD);

    // get the difference of the two closest points
    Vector3 dP = w + (sc * u) - (tc * v);  // =  S1(sc) - S2(tc)

    return dP.magnitude;   // return the closest distance
  }
}
