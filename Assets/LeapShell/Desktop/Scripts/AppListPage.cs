using UnityEngine;
using Leap.Unity.Attributes;
using Procedural.DynamicMesh;

public class AppListPage : MonoBehaviour {

  [AutoFind]
  [SerializeField]
  private CurvedSpace _space;

  [SerializeField]
  private ProceduralMesh _panelMesh;

  [SerializeField]
  private CurvedRect _panelRect;

  [SerializeField]
  private GradientImage _backgroundGradient;

  [Header("Side Graphics")]
  [SerializeField]
  private ProceduralMesh _edgeMesh;

  [SerializeField]
  private Transform _leftGraphicAnchor;

  [SerializeField]
  private Transform _rightGraphicAnchor;

  [SerializeField]
  private MeshRenderer _leftGraphic;

  [SerializeField]
  private MeshRenderer _rightGraphic;

  [Header("Layout")]
  [Incrementable]
  [MinValue(1)]
  [SerializeField]
  private int _columns = 4;

  [Incrementable]
  [MinValue(1)]
  [SerializeField]
  private int _rows = 3;

  [MinValue(0)]
  [SerializeField]
  private float _edgeDistance = 0;

  [SerializeField]
  private float _verticalOffset = 0.05f;

  private int _currX = 0, _currY = 0;
  private AppCollection _apps;

  void OnValidate() {
    GetComponent<MeshFilter>().sharedMesh = _panelMesh.mesh;

    Vector2 leftRect = _leftGraphicAnchor.localPosition;
    _leftGraphic.transform.localPosition = _space.RectToLocal(leftRect, _leftGraphicAnchor.localPosition.z);
    _leftGraphic.transform.localRotation = _space.RectToLocal(Quaternion.identity, leftRect);

    Vector2 rightRect = _rightGraphicAnchor.localPosition;
    _rightGraphic.transform.localPosition = _space.RectToLocal(rightRect, _rightGraphicAnchor.localPosition.z);
    _rightGraphic.transform.localRotation = _space.RectToLocal(Quaternion.identity, rightRect);

    _leftGraphic.GetComponent<MeshFilter>().sharedMesh = _edgeMesh.mesh;
    _rightGraphic.GetComponent<MeshFilter>().sharedMesh = _edgeMesh.mesh;
  }

  void OnEnable() {
    _apps.enabled = true;
  }

  void OnDisable() {
    _apps.enabled = false;
  }

  void Start() {
    GetComponent<MeshFilter>().sharedMesh = _panelMesh.mesh;
  }

  public bool CanAddButton() {
    return _currY < _rows;
  }

  public void AddButton(AppButton button) {
    if (_apps == null) {
      _apps = GetComponent<AppCollection>();
    }

    float percentX = _currX / (_columns - 1.0f);
    float percentY = _currY / (_rows - 1.0f);

    float rectX = Mathf.Lerp(_panelRect.Width * -0.5f + _edgeDistance, _panelRect.Width * 0.5f - _edgeDistance, percentX);
    float rectY = Mathf.Lerp(_panelRect.Height * -0.5f + _edgeDistance, _panelRect.Height * 0.5f - _edgeDistance, percentY) + _verticalOffset;
    Vector2 rect = new Vector2(rectX, rectY);

    button.transform.SetParent(transform);
    button.transform.localPosition = Vector3.zero;
    button.transform.localRotation = Quaternion.identity;
    button.transform.localScale = Vector3.one;
    button.SetRectPos(rect, 0);
    button.gameObject.SetActive(true);

    _apps.Add(button);

    _currX++;
    if (_currX == _columns) {
      _currX = 0;
      _currY++;
    }
  }

  public void InterpolatePage(Vector3 from, Vector3 to,
                              float fromPos, float toPos,
                              float fromAlpha, float toAlpha,
                              bool isLeft, float percent) {
    Vector3 rect = Vector3.Lerp(from, to, percent);
    transform.localPosition = _space.RectToLocal(rect, rect.z);
    transform.localRotation = _space.RectToLocal(Quaternion.identity, rect);

    float pos = Mathf.Lerp(fromPos, toPos, percent);
    float fade = Mathf.Lerp(fromAlpha, toAlpha, percent);
    _backgroundGradient.SetGradient(isLeft, pos, fade);

    foreach (var b in _apps) {
      b.SetAlpha((1 - pos) * fade);
    }

    float leftAlpha = 0, rightAlpha = 0;
    if (rect.x > 0) {
      leftAlpha = pos * fade;
    } else {
      rightAlpha = pos * fade;
    }

    Color leftC = _leftGraphic.material.color;
    leftC.a = leftAlpha;
    _leftGraphic.material.color = leftC;

    Color rightC = _rightGraphic.material.color;
    rightC.a = rightAlpha;
    _rightGraphic.material.color = rightC;
  }
}
