using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Leap.Unity.Attributes;
using Leap.Unity.RuntimeGizmos;

public class Shelf : MonoBehaviour, IRuntimeGizmoComponent {

  [AutoFind]
  [SerializeField]
  private AppList _appList;

  [AutoFind]
  [SerializeField]
  private GraspManager _graspManager;

  [SerializeField]
  private CurvedSpace _space;

  [SerializeField]
  private CurvedRect[] _shelves;

  [MinValue(1)]
  [SerializeField]
  private int _appsPerRow = 8;

  [Range(0, 90)]
  [SerializeField]
  private float _iconAngle = 40;

  [Header("Move Settings")]
  [SerializeField]
  private SpriteRenderer _slotPreviewPrefab;

  [SerializeField]
  private AppButton _buttonPrefab;

  [MinValue(0)]
  [SerializeField]
  private float _maxDistFromSlot = 0.1f;

  [MinValue(0)]
  [SerializeField]
  private float _slotPreviewTweenTime = 0.5f;

  [Range(0, 1)]
  [SerializeField]
  private float _slotPreviewAlpha = 0.3f;

  [MinValue(0)]
  [SerializeField]
  private float _moveIntoSlotTime = 0.1f;

  private AppButton[,] _buttons;
  private TweenHandle[,] _slotPreviewTweens;
  private int _previewShelf = -1, _previewColumn = -1;

  private bool _isMovingApps = false;
  private AppCollection _apps;

  public float IconAngle {
    get {
      return _iconAngle;
    }
  }

  private IEnumerable<AppButton> allButtons {
    get {
      for (int i = 0; i < _shelves.Length; i++) {
        for (int j = 0; j < _appsPerRow; j++) {
          AppButton button = _buttons[i, j];
          if (button != null) {
            yield return button;
          }
        }
      }
    }
  }

  void Awake() {
    _apps = GetComponent<AppCollection>();

    _appList.OnOpen += updateCollectionEnableStatus;
    _appList.OnClose += updateCollectionEnableStatus;

    _buttons = new AppButton[_shelves.Length, _appsPerRow];
    _slotPreviewTweens = new TweenHandle[_shelves.Length, _appsPerRow];

    for (int i = 0; i < _shelves.Length; i++) {
      for (int j = 0; j < _appsPerRow; j++) {
        var renderer = Instantiate(_slotPreviewPrefab) as SpriteRenderer;
        renderer.transform.SetParent(transform);
        renderer.transform.localPosition = getSlotPositionLocal(i, j);
        renderer.transform.localRotation = getSlotRotationLocal(i, j);
        renderer.gameObject.SetActive(true);
        _slotPreviewTweens[i, j] = Tween.Value(0, _slotPreviewAlpha, a => renderer.color = new Color(1, 1, 1, a)).
                                         OverTime(_slotPreviewTweenTime).
                                         Smooth(TweenType.SMOOTH).
                                         Keep();
      }
    }
  }

  void OnDestroy() {
    if (_appList != null) {
      _appList.OnOpen -= updateCollectionEnableStatus;
      _appList.OnClose -= updateCollectionEnableStatus;
    }
  }

  void Update() {
    updateCollectionEnableStatus();
  }

  public void UpdateMovedAppPosition(Vector3 position) {
    int shelf, column;
    float distance;

    closestSlotToPosition(position, out shelf, out column, out distance);

    if (distance > _maxDistFromSlot) {
      shelf = -1;
      column = -1;
    }

    if (shelf != _previewShelf || column != _previewColumn) {
      if (_previewShelf != -1 && _previewColumn != -1) {
        _slotPreviewTweens[_previewShelf, _previewColumn].Play(TweenDirection.BACKWARD);
      }
      _previewShelf = shelf;
      _previewColumn = column;
      if (_previewShelf != -1 && _previewColumn != -1) {
        _slotPreviewTweens[_previewShelf, _previewColumn].Play(TweenDirection.FORWARD);
      }
    }
  }

  public void BeginMove(AppButton grabbedButton) {
    _isMovingApps = true;
    updateCollectionEnableStatus();

    for (int i = 0; i < _shelves.Length; i++) {
      for (int j = 0; j < _appsPerRow; j++) {
        if (_buttons[i, j] == grabbedButton) {
          _buttons[i, j] = null;
          _apps.Remove(grabbedButton);
          DestroyImmediate(grabbedButton.gameObject);
          return;
        }
      }
    }
  }

  public void EndMove(AppGrabbable grabbable) {
    int shelf, column;
    float distance;

    closestSlotToPosition(grabbable.transform.position, out shelf, out column, out distance);

    if (_previewShelf != -1 && _previewColumn != -1) {
      _slotPreviewTweens[_previewShelf, _previewColumn].Play(TweenDirection.BACKWARD);
      _previewShelf = -1;
      _previewColumn = -1;
    }

    if (distance > _maxDistFromSlot) {
      //TODO: Get rid of grabbable properly
      DestroyImmediate(grabbable.gameObject);
      _isMovingApps = false;
      updateCollectionEnableStatus();
    } else {
      Vector3 slotPos = getSlotPositionLocal(shelf, column);
      Quaternion slotRot = getSlotRotationLocal(shelf, column);

      grabbable.transform.SetParent(transform, worldPositionStays: true);
      Tween.Target(grabbable.transform).ToLocalPosition(slotPos).
            Target(grabbable.transform).ToLocalRotation(slotRot).
            Smooth(TweenType.SMOOTH_END).
            OverTime(_moveIntoSlotTime).
            OnReachEnd(() => {
              AppButton newButton = Instantiate(_buttonPrefab) as AppButton;
              _buttons[shelf, column] = newButton;
              _apps.Add(newButton);

              newButton.InitButton(grabbable.appData);

              Vector2 rectPos;
              float offsetRadius;
              getSlotPositionRect(shelf, column, out rectPos, out offsetRadius);

              newButton.transform.SetParent(transform, worldPositionStays: true);
              newButton.SetRectPos(rectPos, offsetRadius, updateRotation: false);
              newButton.transform.localRotation = slotRot;

              newButton.gameObject.SetActive(true);
              DestroyImmediate(grabbable.gameObject);

              _isMovingApps = false;
              updateCollectionEnableStatus();
            }).
            Play();
    }
  }

  private void closestSlotToPosition(Vector3 position, out int shelf, out int column, out float closestDistance) {
    shelf = -1;
    column = -1;
    closestDistance = float.MaxValue;

    for (int i = 0; i < _shelves.Length; i++) {
      for (int j = 0; j < _appsPerRow; j++) {
        Vector3 slotPos = getSlotPositionWorld(i, j);
        float dist = Vector3.Distance(slotPos, position);
        if (dist < closestDistance) {
          closestDistance = dist;
          shelf = i;
          column = j;
        }
      }
    }
  }

  private void getSlotPositionRect(int shelf, int column, out Vector2 rectPos, out float offsetRadius) {
    CurvedRect shelfRect = _shelves[shelf];
    float percentX = column / (_appsPerRow - 1.0f);
    rectPos.x = (percentX - 0.5f) * shelfRect.Width;
    rectPos.y = shelfRect.transform.localPosition.y;
    offsetRadius = shelfRect.RadiusOffset;
  }

  private Vector3 getSlotPositionLocal(int shelf, int column) {
    Vector2 rectPos;
    float radiusOffset;
    getSlotPositionRect(shelf, column, out rectPos, out radiusOffset);
    return _space.RectToLocal(rectPos, radiusOffset);
  }

  private Vector3 getSlotPositionWorld(int shelf, int column) {
    return _space.transform.TransformPoint(getSlotPositionLocal(shelf, column));
  }

  private Quaternion getSlotRotationLocal(int shelf, int column) {
    Vector2 rectPos;
    float radiusOffset;
    getSlotPositionRect(shelf, column, out rectPos, out radiusOffset);
    return _space.RectToLocal(Quaternion.Euler(_iconAngle, 0, 0), rectPos);
  }

  private void updateCollectionEnableStatus() {
    bool shouldCollectionBeActive = true;

    if (_appList.IsOpen || _appList.IsOpening) {
      shouldCollectionBeActive = false;
    }

    if (_isMovingApps) {
      shouldCollectionBeActive = false;
    }

    _apps.enabled = shouldCollectionBeActive;
  }

  public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    drawer.RelativeTo(transform);

    for (int i = 0; i < _shelves.Length; i++) {
      for (int j = 0; j < _appsPerRow; j++) {
        Vector3 pos = getSlotPositionLocal(i, j);
        drawer.DrawWireCube(pos, Vector3.one * 0.01f);
      }
    }
  }
}
