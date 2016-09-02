using UnityEngine;
using UnityEngine.UI;
using Leap;
using Leap.Unity;
using Leap.Unity.RuntimeGizmos;

public class KeyboardKey : CurvedButton {

  [Header("Keyboard settings")]
  [SerializeField]
  private Text _keyLabel;

  public Text keyLabel {
    get {
      return _keyLabel;
    }
  }

  public override float GetHandDistance(Hand hand) {
    Vector2 rect = _space.WorldToRect(_iconRenderer.transform.position);
    Vector2 b = new Vector2(_width, _height);
    float minDist = float.MaxValue;

    Finger finger = hand.Fingers[1];

    Vector3 tip = (finger.Bone(Bone.BoneType.TYPE_METACARPAL).NextJoint.ToVector3()) +
                  (finger.Bone(Bone.BoneType.TYPE_METACARPAL).Direction.ToVector3() * 0.065f) +
                  (hand.PalmNormal.ToVector3() * 0.03f);

    //tip = ((tip - Camera.main.transform.position) * 2f) + Camera.main.transform.position - Camera.main.transform.forward*0.3f;

    Vector2 tipRect = _space.WorldToRect(tip);
    //Vector3 tipRectWorld = _space.RectToWorld(tipRect);

    //tip = (tip - tipRectWorld) * 1.5f + tipRectWorld;

    Vector2 localTip = tipRect - rect;
    Vector2 d = new Vector2(Mathf.Abs(localTip.x), Mathf.Abs(localTip.y)) - b;
    float dist = Mathf.Min(Mathf.Max(d.x, d.y), 0) + new Vector2(Mathf.Max(d.x, 0), Mathf.Max(d.y, 0)).magnitude;
    minDist = Mathf.Min(dist, minDist);

    RuntimeGizmoDrawer drawer;
    if (RuntimeGizmoManager.TryGetGizmoDrawer(gameObject, out drawer)) {
      Vector3 projectedTip = _space.RectToWorld(tipRect);
      drawer.DrawWireSphere(tip, 0.015f);
      if (minDist < 0f) {
        drawer.DrawLine(projectedTip, tip);
        drawer.DrawWireSphere(projectedTip, 0.01f);
      }
    }

    return minDist;
  }

  public override float GetHandDepth(Hand hand) {
    
    float minDist = float.MaxValue;

    Finger finger = hand.Fingers[1];
    Vector3 tip = (finger.Bone(Bone.BoneType.TYPE_METACARPAL).NextJoint.ToVector3()) +
            (finger.Bone(Bone.BoneType.TYPE_METACARPAL).Direction.ToVector3() * 0.065f) +
            (hand.PalmNormal.ToVector3() * 0.03f);
    //tip = ((tip - Camera.main.transform.position) * 2f) + Camera.main.transform.position - Camera.main.transform.forward * 0.3f;
    Vector2 tipRect = _space.WorldToRect(tip);
    //Vector3 tipRectWorld = _space.RectToWorld(tipRect);
    //tip = (tip - tipRectWorld) * 1.5f + tipRectWorld;
    float dist = _space.WorldDistance(tip);
    minDist = Mathf.Min(minDist, dist);

    return Mathf.Min(minDist, (hand.PinchDistance * 0.001f) - 0.02f); //Pinch to press!
  }
}
