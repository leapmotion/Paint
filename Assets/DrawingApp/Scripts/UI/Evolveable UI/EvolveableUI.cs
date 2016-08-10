using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class EvolveableUI : MonoBehaviour, IEvolveable {

  #region PUBLIC FIELDS

  /// <summary>
  /// The anchor point to be used as this behaviour's position when not evolving.
  /// Used to tween between one Evolveable's position and another, so it can't be
  /// the Evolveable's own transform.
  /// </summary>
  public Transform _anchorTransform;

  #endregion

  #region PRIVATE FIELDS

  private Vector3 _originalScale = Vector3.zero;

  private bool _isVisible = true;

  #endregion

  #region PROPERTIES

  public bool IsVisible {
    get { return _isVisible; }
  }

  #endregion

  #region UNITY CALLBACKS

  protected virtual void Start() {
    if (_originalScale == Vector3.zero) {
      _originalScale = this.transform.localScale;
    }
  }

  protected virtual void Update() {
    UpdateMovement();
    UpdateFadeToTransitionColor();
    UpdateSize();
    UpdateFadeFromTransitionColor();
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

      if (_movementDuration > 0F) {
        this.transform.position = Vector3.LerpUnclamped(_moveFrom.position, _moveTo.position, _movementCurve.Evaluate(_movementTimer / _movementDuration));
        this.transform.rotation = Quaternion.SlerpUnclamped(_moveFrom.rotation, _moveTo.rotation, _movementCurve.Evaluate(_movementTimer / _movementDuration));
      }

      if (_movementTimer >= _movementDuration) {
        _moving = false;

        this.transform.position = _moveTo.position;
        this.transform.rotation = _moveTo.rotation;
      }
    }
  }


  private bool _fadingToColor = false;
  private float _fadeDuration = 0F;
  private float _fadeTimer = 0F;
  private AnimationCurve _colorCurve;
  private Color _transitionColor;
  public void FadeToTransitionColor(Color transitionColor, AnimationCurve curve, float duration) {
    _fadingToColor = true;
    _fadeDuration = duration;
    _fadeTimer = 0F;
    _colorCurve = curve;
    _transitionColor = transitionColor;
  }
  protected virtual void UpdateFadeToTransitionColor() {
    if (_fadingToColor) {
      _fadeTimer += Time.deltaTime;

      if (_fadeDuration > 0f) {
        // TODO: Fix color transition
        //for (int i = 0; i < _origGraphics.Count; i++) {
        //  _mapOrigToClone[_origGraphics[i]].color = Color.Lerp(_origGraphics[i].color, _transitionColor, _colorCurve.Evaluate(_fadeTimer / _fadeDuration));
        //}
      }

      if (_fadeTimer >= _fadeDuration) {
        _fadingToColor = false;

        // TODO: Fix color transition
        //for (int i = 0; i < _origGraphics.Count; i++) {
        //  _mapOrigToClone[_origGraphics[i]].color = _transitionColor;
        //}
      }
    }
  }

  private bool _changingSize = false;
  private float _sizeChangeDuration = 0F;
  private float _sizeChangeTimer = 0F;
  private AnimationCurve _sizeCurve;
  private float _fromScale = 1F;
  private float _toScale = 1F;
  public void ChangeToScale(float fromScale, float toScale, AnimationCurve curve, float duration) {
    _changingSize = true;
    _sizeChangeDuration = duration;
    _sizeChangeTimer = 0F;
    _sizeCurve = curve;
    _fromScale = fromScale;
    _toScale = toScale;
  }
  protected virtual void UpdateSize() {
    if (_changingSize) {
      _sizeChangeTimer += Time.deltaTime;

      if (_sizeChangeDuration != 0F) {
        this.transform.localScale = Vector3.Lerp(_originalScale * _fromScale, _originalScale * _toScale, _sizeCurve.Evaluate(_sizeChangeTimer / _sizeChangeDuration));
      }

      if (_sizeChangeTimer >= _sizeChangeDuration) {
        _changingSize = false;

        this.transform.localScale = _originalScale * _toScale;
      }
    }
  }

  private bool _fadingFromTransitionColor = false;
  private float _fadeInDuration = 0F;
  private float _fadeInTimer = 0F;
  public void FadeFromTransitionColor(Color transitionColor, AnimationCurve curve, float duration) {
    _fadingFromTransitionColor = true;
    _fadeInDuration = duration;
    _fadeInTimer = 0F;
    _colorCurve = curve;
    _transitionColor = transitionColor;
  }
  protected virtual void UpdateFadeFromTransitionColor() {
    if (_fadingFromTransitionColor) {
      _fadeInTimer += Time.deltaTime;

      if (_fadeInDuration > 0F) {
        // TODO: Fix color transition
        //for (int i = 0; i < _origGraphics.Count; i++) {
        //  _mapOrigToClone[_origGraphics[i]].color = Color.Lerp(_transitionColor, _origGraphics[i].color, _colorCurve.Evaluate(_fadeInTimer / _fadeInDuration));
        //}
      }

      if (_fadeInTimer >= _fadeInDuration) {
        _fadingFromTransitionColor = false;

        // TODO: Fix color transition
        //for (int i = 0; i < _origGraphics.Count; i++) {
        //  _mapOrigToClone[_origGraphics[i]].color = _origGraphics[i].color;
        //}
      }
    }
  }

  public virtual void Appear() {
    _isVisible = true;
    var graphics = GetComponentsInChildren<Graphic>();
    for (int i = 0; i < graphics.Length; i++) {
      graphics[i].enabled = true;
    }
  }

  public virtual void Disappear() {
    _isVisible = false;
    var graphics = GetComponentsInChildren<Graphic>();
    for (int i = 0; i < graphics.Length; i++) {
      graphics[i].enabled = false;
    }
  }

  #endregion
}
