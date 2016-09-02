using UnityEngine;
using Leap.Unity.Attributes;
using Leap.Unity.RuntimeGizmos;

public class CurvedSpace : MonoBehaviour, IRuntimeGizmoComponent {

  [MinValue(0.00001f)]
  [SerializeField]
  private float _curvature = 0;

  [SerializeField]
  private bool _showGizmos = false;

  public Vector3 RectToLocal(Vector2 rect) {
    float theta = rect.x * _curvature;
    float dx = Mathf.Sin(theta) / _curvature;
    float dz = (Mathf.Cos(theta) - 1) / _curvature;

    return new Vector3(dx, rect.y, dz);
  }

  public Vector3 RectToLocal(Vector2 rect, float radius) {
    return RectToLocal(rect) + GetLocalNormal(rect) * radius;
  }

  public Vector3 RectToWorld(Vector2 rect) {
    return transform.TransformPoint(RectToLocal(rect));
  }

  public Vector3 RectToWorld(Vector2 rect, float radius) {
    return transform.TransformPoint(RectToLocal(rect, radius));
  }

  public Vector2 LocalToRect(Vector3 local) {
    float cosT = 1 + local.z * _curvature;
    float sinT = local.x * _curvature;
    float theta = Mathf.Atan2(sinT, cosT);
    return new Vector2(theta / _curvature, local.y);
  }

  public Vector2 WorldToRect(Vector3 world) {
    return LocalToRect(transform.InverseTransformPoint(world));
  }

  public Quaternion RectToLocal(Quaternion rot, Vector2 rect) {
    return Quaternion.Euler(0, Mathf.Rad2Deg * _curvature * rect.x, 0) * rot;
  }

  public Quaternion RectToWorld(Quaternion rot, Vector2 rect) {
    return transform.rotation * RectToLocal(rot, rect);
  }

  public Vector3 GetLocalNormal(Vector2 rect) {
    return Quaternion.Euler(0, Mathf.Rad2Deg * _curvature * rect.x, 0) * Vector3.forward;
  }

  public float LocalDistance(Vector3 local) {
    float rad = 1 / _curvature;
    return rad - Vector2.Distance(new Vector2(local.x, local.z), new Vector2(0, -rad));
  }

  public float WorldDistance(Vector3 world) {
    return LocalDistance(transform.InverseTransformPoint(world));
  }

  public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    if (!_showGizmos) {
      return;
    }

    drawer.color = Color.red;

    for (float i = -1; i <= 1; i += 0.05f) {
      for (float j = -1; j < 1; j += 0.05f) {
        drawer.DrawLine(RectToWorld(new Vector2(i, j)), RectToWorld(new Vector2(i, j + 0.05f)));
        drawer.DrawLine(RectToWorld(new Vector2(j, i)), RectToWorld(new Vector2(j + 0.05f, i)));
      }
    }
  }
}
