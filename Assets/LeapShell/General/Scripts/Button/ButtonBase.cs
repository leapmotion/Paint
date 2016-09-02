using UnityEngine;
using Leap;
using Leap.Unity.Attributes;

public abstract class ButtonBase : MonoBehaviour {

  public enum State {
    None,
    Hover,
    Pressing,
    Selected
  }

  [Header("Gesture Settings")]
  [SerializeField]
  private float _selectDepth = 0.0f;

  [SerializeField]
  private float _pressDepth = 0.02f;

  [SerializeField]
  private float _hoverDepth = 0.1f;

  [SerializeField]
  private float _maxHoverRange = 0f;

  [SerializeField]
  private float _maxPressRange = 0f;

  [MinValue(0)]
  [SerializeField]
  private float _returnToRestTime = 0.1f;

  [Header("Sounds")]
  [SerializeField]
  private AudioClip _hoverSound;

  [Range(0, 1)]
  [SerializeField]
  private float _hoverSoundVolume = 1;

  [SerializeField]
  private AudioClip _pressSound;

  [Range(0, 1)]
  [SerializeField]
  private float _pressSoundVolume = 1;

  [SerializeField]
  private AudioClip _selectSound;

  [Range(0, 1)]
  [SerializeField]
  private float _selectSoundVolume = 1;

  [SerializeField]
  private AudioClip _releaseSound;

  [Range(0, 1)]
  [SerializeField]
  private float _releaseSoundVolume = 1;

  public float SelectDepth {
    get {
      return _selectDepth;
    }
  }

  public float PressDepth {
    get {
      return _pressDepth;
    }
  }

  public float HoverDepth {
    get {
      return _hoverDepth;
    }
  }

  public float MaxHoverRange {
    get {
      return _maxHoverRange;
    }
  }

  public float MaxPressRange {
    get {
      return _maxPressRange;
    }
  }

  public float CurrentDepth {
    get {
      return _currentDepth;
    }
  }

  public State CurrentState {
    get {
      return _currentState;
    }
  }

  private State _currentState = State.None;
  private float _currentDepth = 0;
  private TweenHandle _hoverTween;
  private TweenHandle _positionTween;

  protected virtual void Awake() {
    _hoverTween = buildHoverTween();
    SetDepth(_pressDepth);
  }

  protected virtual void OnDestroy() {
    if (_hoverTween.IsValid) _hoverTween.Stop();
    if (_positionTween.IsValid) _positionTween.Stop();
  }

  public abstract float GetHandDistance(Hand hand);
  public abstract float GetHandDepth(Hand hand);

  public virtual void SetDepth(float depth) {
    _currentDepth = depth;
  }

  public virtual void ChangeState(State newState) {
    if (_positionTween.IsValid) {
      _positionTween.Stop();
    }

    if (newState == State.Hover) {
      if (_currentState == State.None) {
        if (_hoverSound != null) {
          AudioSource.PlayClipAtPoint(_hoverSound, transform.position, _hoverSoundVolume);
        }
      } else if (_currentState == State.Selected) {
        if (_releaseSound != null) {
          AudioSource.PlayClipAtPoint(_releaseSound, transform.position, _releaseSoundVolume);
        }
      }
    }

    switch (newState) {
      case State.None:
        _hoverTween.Play(TweenDirection.BACKWARD);
        _positionTween = Tween.Value(_currentDepth, _pressDepth, d => SetDepth(d)).
                               OverTime(_returnToRestTime).
                               Smooth(TweenType.SMOOTH);
        _positionTween.Play();
        break;
      case State.Hover:
        _hoverTween.Play(TweenDirection.FORWARD);
        break;
      case State.Pressing:
        if (_pressSound != null) {
          AudioSource.PlayClipAtPoint(_pressSound, transform.position, _pressSoundVolume);
        }
        _hoverTween.Play(TweenDirection.FORWARD);
        break;
      case State.Selected:
        if (_selectSound != null) {
          AudioSource.PlayClipAtPoint(_selectSound, transform.position, _selectSoundVolume);
        }
        _hoverTween.Play(TweenDirection.FORWARD);
        break;
    }

    _currentState = newState;
  }

  protected abstract TweenHandle buildHoverTween();

}
