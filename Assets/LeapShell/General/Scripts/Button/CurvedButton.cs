using UnityEngine;
using Leap;
using Leap.Unity;
using Leap.Unity.Attributes;
using Leap.Unity.RuntimeGizmos;

public class CurvedButton : ButtonBase {

  [Header("Object Settings")]
  [SerializeField]
  protected CurvedSpace _space;

  [SerializeField]
  protected SpriteRenderer _iconRenderer;

  [SerializeField]
  protected SpriteRenderer _shadowRenderer;

  [MinValue(0)]
  [SerializeField]
  protected float _width;

  [MinValue(0)]
  [SerializeField]
  protected float _height;

  [SerializeField]
  protected float _depthOffset = 0.01f;

  [Header("Animation Settings")]
  [MinValue(1)]
  [SerializeField]
  protected float _hoverScale = 1.3f;

  [MinValue(0)]
  [SerializeField]
  protected float _hoverTweenTime = 0.1f;

  [SerializeField]
  protected Vector2 _dropShadowOffset = new Vector2(0.1f, 0.1f);

  [Range(0, 1)]
  [SerializeField]
  protected float _shadowAlpha = 0.5f;

  protected Vector2 _rectPos;
  protected float _offsetRadius;

  protected override TweenHandle buildHoverTween() {
    return Tween.Target(_iconRenderer.transform).LocalScale(_iconRenderer.transform.localScale, _iconRenderer.transform.localScale * _hoverScale).
                 Target(_shadowRenderer.transform).LocalScale(_shadowRenderer.transform.localScale, _shadowRenderer.transform.localScale * _hoverScale).
                 OverTime(_hoverTweenTime).
                 Smooth(TweenType.SMOOTH).
                 Keep();
  }

  public void SetRectPos(Vector2 rectPos, float offsetRadius, bool updateRotation = true) {
    _rectPos = rectPos;
    _offsetRadius = offsetRadius;

    transform.localPosition = _space.RectToLocal(_rectPos, _offsetRadius);

    if (updateRotation) {
      transform.localRotation = _space.RectToLocal(Quaternion.identity, _rectPos);
    }
  }

  public override void SetDepth(float depth) {
    depth = Mathf.Clamp(depth, SelectDepth, PressDepth);

    base.SetDepth(depth);

    _iconRenderer.transform.localPosition = new Vector3(0, 0, -CurrentDepth);

    float shadowPercent = (CurrentDepth - SelectDepth) / (PressDepth - SelectDepth);
    Vector2 flatShadowOffset = _dropShadowOffset * shadowPercent;
    _shadowRenderer.transform.localPosition = new Vector3(flatShadowOffset.x, flatShadowOffset.y, _shadowRenderer.transform.localPosition.z);
  }

  public virtual void SetAlpha(float alpha) {
    _iconRenderer.color = new Color(1, 1, 1, alpha);
    _shadowRenderer.color = new Color(0, 0, 0, _shadowAlpha * alpha);
  }

  public override float GetHandDistance(Hand hand) {
    Vector2 rect = _space.WorldToRect(_iconRenderer.transform.position);
    Vector2 b = new Vector2(_width, _height);
    float minDist = float.MaxValue;

    for (int i = 0; i < 5; i++) {
      Finger finger = hand.Fingers[i];
      if (!finger.IsExtended) {
        continue;
      }

      if (finger.Type == Finger.FingerType.TYPE_THUMB) {
        continue;
      }

      Vector3 tip = finger.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3();
      Vector2 tipRect = _space.WorldToRect(tip);

      RuntimeGizmoDrawer drawer;
      if (RuntimeGizmoManager.TryGetGizmoDrawer(gameObject, out drawer)) {
        Vector3 projectedTip = _space.RectToWorld(tipRect, _offsetRadius);
        drawer.DrawLine(projectedTip, tip);
        drawer.DrawSphere(projectedTip, 0.01f);
      }

      Vector2 localTip = tipRect - rect;
      Vector2 d = new Vector2(Mathf.Abs(localTip.x), Mathf.Abs(localTip.y)) - b;
      float dist = Mathf.Min(Mathf.Max(d.x, d.y), 0) + new Vector2(Mathf.Max(d.x, 0), Mathf.Max(d.y, 0)).magnitude;
      minDist = Mathf.Min(dist, minDist);
    }

    return minDist;
  }

  public override float GetHandDepth(Hand hand) {
    float minDist = float.MaxValue;

    for (int i = 0; i < 5; i++) {
      Finger finger = hand.Fingers[i];
      Vector3 tip = finger.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3();
      float dist = -transform.InverseTransformPoint(tip).z;
      //float dist = _space.WorldDistance(tip) + _offsetRadius;

      minDist = Mathf.Min(minDist, dist);
    }

    return minDist + _depthOffset;
  }
}
