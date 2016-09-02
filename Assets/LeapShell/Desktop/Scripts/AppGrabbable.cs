using UnityEngine;
using Leap.Unity.Attributes;

public class AppGrabbable : MonoBehaviour {

  [SerializeField]
  private CurvedSpace _space;

  [SerializeField]
  private SpriteRenderer _iconRenderer;

  [AutoFind]
  [SerializeField]
  private Shelf _shelf;

  private AppData _data;

  public AppData appData {
    get {
      return _data;
    }
  }

  public void InitGrabbable(AppData data) {
    _data = data;
    _iconRenderer.sprite = data.sprite;
  }

  public void UpdatePosition(Vector3 worldPosition) {
    transform.position = worldPosition;

    Vector3 rectPos = _space.WorldToRect(worldPosition);
    transform.rotation = _space.RectToWorld(Quaternion.Euler(_shelf.IconAngle, 0, 0), rectPos);
  }

}
