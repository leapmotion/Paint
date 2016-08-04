using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class EvolveableUI : MonoBehaviour, IEvolveable {

  /// <summary>
  /// The anchor point to be used as this behaviour's position when not evolving.
  /// Used to tween between one Evolveable's position and another, so it can't be
  /// the Evolveable's own transform.
  /// </summary>
  public Transform _anchorTransform;

  #region PRIVATE ATTRIBUTES

  protected Graphic[] _graphics;
  private Color[] _originalColors;
  private Vector3 _originalScale = Vector3.zero;

  #endregion

  #region UNITY CALLBACKS

  protected virtual void Start() {
    if (_graphics == null) {
      _graphics = GetComponentsInChildren<Graphic>();
    }

    _originalColors = new Color[_graphics.Length];
    for (int i = 0; i < _graphics.Length; i++) {
      _originalColors[i] = _graphics[i].color;
    }

    if (_originalScale == Vector3.zero) {
      _originalScale = this.transform.localScale;
    }
  }

  protected virtual void Update() {
    UpdateMovement();
    UpdateFadeToWhite();
    UpdateSize();
    UpdateFadeFromWhite();
  }

  #endregion

  #region EVOLVEABLE IMPLEMENTATION

  public virtual Transform GetAnchor() {
    return _anchorTransform;
  }

  protected bool _moving = false;
  protected float _movementDuration = 0F;
  protected float _movementTimer = 0F;
  protected AnimationCurve _movementCurve;
  protected Transform _moveFrom;
  protected Transform _moveTo;
  public void MoveFromTo(Transform from, Transform to, AnimationCurve translationCurve, float duration) {
    _moving = true;
    _movementDuration = duration;
    _movementTimer = 0F;
    _movementCurve = translationCurve;
    _moveFrom = from;
    _moveTo = to;
  }
  protected virtual void UpdateMovement() {
    if (_moving) {
      _movementTimer += Time.deltaTime;

      this.transform.position = Vector3.Lerp(_moveFrom.position, _moveTo.position, _movementCurve.Evaluate(_movementTimer / _movementDuration));
      this.transform.rotation = Quaternion.Slerp(_moveFrom.rotation, _moveTo.rotation, _movementCurve.Evaluate(_movementTimer / _movementDuration));

      if (_movementTimer >= _movementDuration) {
        _moving = false;

        this.transform.position = _moveTo.position;
        this.transform.rotation = _moveTo.rotation;
      }
    }
  }

  private bool _fadingToWhite = false;
  private float _fadeOutDuration = 0F;
  private float _fadeOutTimer = 0F;
  public virtual void FadeToWhite(float duration) {
    _fadingToWhite = true;
    _fadeOutDuration = duration;
    _fadeOutTimer = 0F;
  }
  protected virtual void UpdateFadeToWhite() {
    if (_fadingToWhite) {
      _fadeOutTimer += Time.deltaTime;

      _graphics = GetComponentsInChildren<Graphic>();
      for (int i = 0; i < _graphics.Length; i++) {
        _graphics[i].color = Color.Lerp(_originalColors[i], Color.white, _fadeOutTimer / _fadeOutDuration);
      }

      if (_fadeOutTimer >= _fadeOutDuration) {
        _fadingToWhite = false;

        for (int i = 0; i < _graphics.Length; i++) {
          _graphics[i].color = Color.white;
        }
      }
    }
  }

  private bool _changingSize = false;
  private float _sizeChangeDuration = 0F;
  private float _sizeChangeTimer = 0F;
  private float _fromScale = 1F;
  private float _toScale = 1F;
  public void ChangeToSize(float fromScale, float toScale, float duration) {
    _changingSize = true;
    _sizeChangeDuration = duration;
    _sizeChangeTimer = 0F;
    _fromScale = fromScale;
    _toScale = toScale;
  }
  protected virtual void UpdateSize() {
    if (_changingSize) {
      _sizeChangeTimer += Time.deltaTime;

      this.transform.localScale = Vector3.Lerp(_originalScale * _fromScale, _originalScale * _toScale, _sizeChangeTimer / _sizeChangeDuration);

      if (_sizeChangeTimer >= _sizeChangeDuration) {
        _changingSize = false;

        this.transform.localScale = _originalScale * _toScale;
      }
    }
  }

  private bool _fadingFromWhite = false;
  private float _fadeInDuration = 0F;
  private float _fadeInTimer = 0F;
  public void FadeFromWhite(float duration) {
    _fadingFromWhite = true;
    _fadeInDuration = duration;
    _fadeInTimer = 0F;
  }
  protected virtual void UpdateFadeFromWhite() {
    if (_fadingFromWhite) {
      _fadeInTimer += Time.deltaTime;

      _graphics = GetComponentsInChildren<Graphic>();
      for (int i = 0; i < _graphics.Length; i++) {
        _graphics[i].color = Color.Lerp(Color.white, _originalColors[i], _fadeInTimer / _fadeInDuration);
      }

      if (_fadeInTimer >= _fadeInDuration) {
        _fadingFromWhite = false;

        for (int i = 0; i < _graphics.Length; i++) {
          _graphics[i].color = _originalColors[i];
        }
      }
    }
  }

  public virtual void Appear(bool asWhite) {
    _graphics = GetComponentsInChildren<Graphic>();
    if (_graphics != null) {
      for (int i = 0; i < _graphics.Length; i++) {
        if (asWhite) {
          _graphics[i].color = Color.white;
        }
        _graphics[i].enabled = true;
      }
    }
  }

  public virtual void Disappear() {
    _graphics = GetComponentsInChildren<Graphic>();
    if (_graphics != null) {
      for (int i = 0; i < _graphics.Length; i++) {
        _graphics[i].enabled = false;
      }
    }
  }

  #endregion

}
