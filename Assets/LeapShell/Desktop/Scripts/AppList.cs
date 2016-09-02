using UnityEngine;
using System;
using System.Collections.Generic;
using Leap;
using Leap.Unity;
using Leap.Unity.Attributes;

public class AppList : MonoBehaviour {

  [AutoFind]
  [SerializeField]
  private LeapProvider _provider;

  [AutoFind]
  [SerializeField]
  private CurvedSpace _space;

  [SerializeField]
  private Sprite[] _appSprites;

  [SerializeField]
  private AppListPage _pagePrefab;

  [SerializeField]
  private AppButton _buttonPrefab;

  [Header("Gesture Settings")]
  [SerializeField]
  private MomentumSlider _slider;

  [SerializeField]
  private bool _canSwipeOnCenter = true;

  [SerializeField]
  private AnimationCurve _decimalBias;

  [Header("Transition Settings")]
  [SerializeField]
  private Transform _inAnchor;

  [MinValue(0)]
  [SerializeField]
  private float _transitionTime = 0.5f;

  [SerializeField]
  private AnimationCurve _transitionCurve;

  [Header("Anchors")]
  [SerializeField]
  private CurvedRect _interactionSpaceAnchor;

  [SerializeField]
  private CurvedRect _centerPageAnchor;

  [SerializeField]
  private CurvedRect _leftPageAnchor;

  [SerializeField]
  private CurvedRect _rightPageAnchor;

  [SerializeField]
  private CurvedRect _leftBackPagelAnchor;

  [SerializeField]
  private CurvedRect _rightBackPageAnchor;

  [SerializeField]
  private AnimationCurve _backToFrontCurve;

  [Header("Sounds")]
  [SerializeField]
  private AudioClip _connectSound;

  [Range(0, 1)]
  [SerializeField]
  private float _connectSoundVolume = 1;

  [SerializeField]
  private AudioClip _disconnectSound;

  [SerializeField]
  private DisconnectSoundTime _disconnectSoundTime;

  [Range(0, 1)]
  [SerializeField]
  private float _disconnectSoundVolume = 1;

  [SerializeField]
  private AudioSource _scrollSource;

  [SerializeField]
  private float _scrollSoundSmooth = 0.05f;

  [SerializeField]
  private float _scrollSoundSensitivity = 1f;

  [MinMax(0, 2)]
  [SerializeField]
  private Vector2 _scrollPitchRange = new Vector2(0.9f, 1.1f);

  [MinMax(0, 1)]
  [SerializeField]
  private Vector2 _scrollVolumeRange = new Vector2(0, 1);

  public event Action OnOpen;
  public event Action OnClose;

  private Vector2 _pageSize;

  private List<AppListPage> _pages;
  private bool _isSliderLocked = false;
  private float _constrainSign = 1;

  //Transition settings
  private TweenHandle _transitionTween;

  //Sound vars
  private float _prevPosition = 0;
  private SmoothedFloat _positionVel;

  public bool IsOpen {
    get {
      return !_transitionTween.IsRunning && _transitionTween.Progress == 1.0f;
    }
  }

  public bool IsOpening {
    get {
      return _transitionTween.IsRunning && _transitionTween.Direction == TweenDirection.FORWARD;
    }
  }

  public void Open() {
    _transitionTween.Play(TweenDirection.FORWARD);
  }

  public void Close() {
    _transitionTween.Play(TweenDirection.BACKWARD);
  }

  void Awake() {
    _transitionTween = Tween.Target(transform).ToLocalPosition(_inAnchor).
                             OverTime(_transitionTime).
                             Smooth(_transitionCurve).
                             OnReachEnd(() => {
                               //Activate the first page when the transition completes
                               setActivePage(0);
                               if (OnOpen != null) {
                                 OnOpen();
                               }
                             }).
                             OnLeaveEnd(() => {
                               //Deactivate all pages right away when we start the transition out
                               setActivePage(-1);
                               if (OnClose != null) {
                                 OnClose();
                               }
                             }).
                             OnReachStart(() => {
                               //Reset the position to the first page when the transition out finished
                               _slider.ResetSlider(0);
                             }).
                             OnProgress(p => {
                               //Update the position so the fade can be updated too
                               updatePosition(_prevPosition);
                             }).
                             Keep();

    _positionVel = new SmoothedFloat();
    _positionVel.delay = _scrollSoundSmooth;

    _slider.HandToDistance = getDistance;
    _slider.HandToPosition = getPosition;
    _slider.CanConnect = canConnect;
    _slider.CanHandInteract = canInteract;
    _slider.GetRestPosition = f => Mathf.Clamp(Mathf.RoundToInt(f), 0, _pages.Count - 1);
    _slider.ConstrainOffset = constrainOffset;

    _slider.OnPosition += updatePosition;
    _slider.OnConnect += onConnect;
    _slider.OnDisconnect += onDisconnect;
    _slider.OnStationary += onStationary;
  }

  void Start() {
    _pages = new List<AppListPage>();
    AppListPage currPage = null;
    for (int i = 0; i < _appSprites.Length; i++) {
      if (currPage == null || !currPage.CanAddButton()) {
        currPage = Instantiate(_pagePrefab, transform) as AppListPage;
        _pages.Add(currPage);
      }

      var button = Instantiate(_buttonPrefab) as AppButton;
      var appData = new AppData(_appSprites[i]);
      button.InitButton(appData);

      currPage.AddButton(button);
    }

    for (int i = 0; i < _pages.Count; i++) {
      _pages[i].gameObject.SetActive(true);
    }

    _transitionTween.Progress = 0;
    setActivePage(-1);
    updatePosition(0);
  }

  void Update() {
    if (Input.GetKeyDown(KeyCode.F1)) {
      Open();
    }
    if (Input.GetKeyDown(KeyCode.F2)) {
      Close();
    }

    if (IsOpen) {
      List<Hand> hands = _provider.CurrentFrame.Hands;

      _slider.Update(hands);

      float smallestZDepth = float.MaxValue;
      for (int i = 0; i < hands.Count; i++) {
        smallestZDepth = Mathf.Min(smallestZDepth, getDistance(hands[i]));
      }
      if (_isSliderLocked) {
        if (smallestZDepth > 0) {
          _isSliderLocked = false;
        }
      } else {
        if (smallestZDepth < 0) {
          _isSliderLocked = true;
        }
      }

      if (_slider.IsStationary) {
        int enabledPage = Mathf.RoundToInt(_slider.Position);
        setActivePage(enabledPage);
      } else {
        setActivePage(-1);
      }
    }
  }

  private void setActivePage(int pageIndex) {
    for (int i = 0; i < _pages.Count; i++) {
      _pages[i].enabled = i == pageIndex;
    }
  }

  private void onConnect(Hand hand, float position) {
    _constrainSign = Mathf.Sign(position);

    if (_connectSound != null) {
      AudioSource.PlayClipAtPoint(_connectSound, transform.position, _connectSoundVolume);
    }

    if (_scrollSource.enabled) {
      _scrollSource.Play();
    }
  }

  private void onDisconnect() {
    if (_disconnectSoundTime == DisconnectSoundTime.OnDisconnect) {
      if (_disconnectSound != null) {
        AudioSource.PlayClipAtPoint(_disconnectSound, transform.position, _disconnectSoundVolume);
      }
    }
  }

  private void onStationary() {
    if (_disconnectSoundTime == DisconnectSoundTime.OnStationary) {
      if (_disconnectSound != null) {
        AudioSource.PlayClipAtPoint(_disconnectSound, transform.position, _disconnectSoundVolume);
      }
    }

    if (_scrollSource.enabled) {
      _scrollSource.Stop();
    }
  }

  private float constrainOffset(float offset) {
    if (_constrainSign < 0) {
      return Mathf.Max(0, offset);
    } else {
      return Mathf.Min(0, offset);
    }
  }

  private bool canInteract(Hand hand) {
    bool anyInside = false;
    for (int i = 0; i < 5; i++) {
      Finger finger = hand.Fingers[i];
      Vector3 tip = finger.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3();
      Vector2 rectTip = _space.WorldToRect(tip);

      if (_interactionSpaceAnchor.IsPointInside(rectTip)) {
        anyInside = true;
        break;
      }
    }

    return anyInside;
  }

  private bool canConnect(Hand hand) {
    if (_isSliderLocked) {
      return false;
    }

    for (int i = 0; i < 5; i++) {
      Finger finger = hand.Fingers[i];
      Vector3 tip = finger.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3();
      Vector2 rectTip = _space.WorldToRect(tip);
      if (_leftPageAnchor.IsPointInside(rectTip) || _rightPageAnchor.IsPointInside(rectTip) ||
          (_canSwipeOnCenter && _centerPageAnchor.IsPointInside(rectTip))) {
        return true;
      }
    }
    return false;
  }

  private float getDistance(Hand hand) {
    float dist = float.MaxValue;
    for (int i = 0; i < 5; i++) {
      Finger finger = hand.Fingers[i];
      Vector3 tip = finger.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3();
      dist = Mathf.Min(dist, _space.WorldDistance(tip));
    }
    return dist;
  }

  private float getPosition(Hand hand) {
    Vector3 averageTip = Vector3.zero;
    for (int i = 0; i < 5; i++) {
      Finger finger = hand.Fingers[i];
      averageTip += finger.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3();
    }

    return _space.WorldToRect(averageTip / 5).x / (_centerPageAnchor.transform.position.x - _rightPageAnchor.transform.position.x);
  }

  private void updatePosition(float newPosition) {
    _positionVel.Update((newPosition - _prevPosition) / Time.deltaTime, Time.deltaTime);
    _prevPosition = newPosition;

    if (_scrollSource.enabled) {
      float p = Mathf.Abs(_positionVel.value) * _scrollSoundSensitivity;
      _scrollSource.volume = Mathf.Lerp(_scrollVolumeRange.x, _scrollVolumeRange.y, p);
      _scrollSource.pitch = Mathf.Lerp(_scrollPitchRange.x, _scrollPitchRange.y, p);
    }

    float floor = Mathf.Floor(newPosition);
    float dec = newPosition - floor;
    float fade = _transitionTween.Progress;
    newPosition = floor + _decimalBias.Evaluate(dec);

    for (int i = 0; i < _pages.Count; i++) {
      var page = _pages[i];
      float position = i - newPosition;

      float percent = Mathf.Abs(position) - Mathf.Floor(Mathf.Abs(position));

      if (Mathf.Abs(position) >= 2) {
        percent = 1;
      }

      if (Mathf.Abs(position) >= 1) {
        percent = _backToFrontCurve.Evaluate(percent);
      }

      if (position <= -1) {
        page.InterpolatePage(_leftPageAnchor.transform.localPosition, _leftBackPagelAnchor.transform.localPosition,
                             1, 1,
                             fade, 0,
                             true, percent);
      } else if (position < 0) {
        page.InterpolatePage(_centerPageAnchor.transform.localPosition, _leftPageAnchor.transform.localPosition,
                             0, 1,
                             fade, fade,
                             true, percent);
      } else if (position >= 1) {
        page.InterpolatePage(_rightPageAnchor.transform.localPosition, _rightBackPageAnchor.transform.localPosition,
                             1, 1,
                             fade, 0,
                             false, percent);
      } else {
        page.InterpolatePage(_centerPageAnchor.transform.localPosition, _rightPageAnchor.transform.localPosition,
                             0, 1,
                             fade, fade,
                             false, percent);
      }
    }
  }

  public enum DisconnectSoundTime {
    OnDisconnect,
    OnStationary
  }

}
