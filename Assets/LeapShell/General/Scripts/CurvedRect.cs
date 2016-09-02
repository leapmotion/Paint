using UnityEngine;
using Leap.Unity.Attributes;
using Leap.Unity.RuntimeGizmos;

public class CurvedRect : MonoBehaviour, IRuntimeGizmoComponent {
  public const int GIZMO_RESOLUTION = 16;


  [AutoFind]
  [SerializeField]
  private CurvedSpace _space;

  [MinValue(0)]
  [SerializeField]
  private float _width;

  [MinValue(0)]
  [SerializeField]
  private float _height;

  [SerializeField]
  private float _radiusOffset;

  public CurvedSpace Space {
    get {
      return _space;
    }
  }

  public float Width {
    get {
      return _width;
    }
  }

  public float Height {
    get {
      return _height;
    }
  }

  public Vector2 Size {
    get {
      return new Vector2(_width, _height);
    }
  }

  public float RadiusOffset {
    get {
      return _radiusOffset;
    }
  }

  public Vector2 Extents {
    get {
      return Size / 2;
    }
  }

  public bool IsPointInside(Vector2 rectPoint) {
    Rect r = new Rect((Vector2)transform.localPosition - Extents, Size);
    return r.Contains(rectPoint);
  }

  public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    if (_space == null) return;

    Vector2 rectPos = transform.localPosition;

    Vector2 topLeft = rectPos + new Vector2(-Width, Height) * 0.5f;
    Vector2 topRight = rectPos + new Vector2(Width, Height) * 0.5f;
    Vector2 bottomLeft = rectPos + new Vector2(-Width, -Height) * 0.5f;
    Vector2 bottomRight = rectPos + new Vector2(Width, -Height) * 0.5f;

    drawer.matrix *= Matrix4x4.TRS(_space.GetLocalNormal(rectPos) * transform.localPosition.z, Quaternion.identity, Vector3.one);

    for (int i = 0; i < GIZMO_RESOLUTION - 1; i++) {
      float percentA = (i + 0.0f) / (GIZMO_RESOLUTION - 1.0f);
      float percentB = (i + 1.0f) / (GIZMO_RESOLUTION - 1.0f);
      drawer.DrawLine(_space.RectToWorld(Vector2.Lerp(topLeft, topRight, percentA), _radiusOffset),
                      _space.RectToWorld(Vector2.Lerp(topLeft, topRight, percentB), _radiusOffset));
      drawer.DrawLine(_space.RectToWorld(Vector2.Lerp(bottomLeft, bottomRight, percentA), _radiusOffset),
                      _space.RectToWorld(Vector2.Lerp(bottomLeft, bottomRight, percentB), _radiusOffset));
    }

    for (int i = 0; i < GIZMO_RESOLUTION - 1; i++) {
      float percentA = (i + 0.0f) / (GIZMO_RESOLUTION - 1.0f);
      float percentB = (i + 1.0f) / (GIZMO_RESOLUTION - 1.0f);
      drawer.DrawLine(_space.RectToWorld(Vector2.Lerp(topLeft, bottomLeft, percentA), _radiusOffset),
                      _space.RectToWorld(Vector2.Lerp(topLeft, bottomLeft, percentB), _radiusOffset));
      drawer.DrawLine(_space.RectToWorld(Vector2.Lerp(topRight, bottomRight, percentA), _radiusOffset),
                      _space.RectToWorld(Vector2.Lerp(topRight, bottomRight, percentB), _radiusOffset));
    }
  }
}
