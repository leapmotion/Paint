using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

/// <summary>
/// Using scaling, motion, and a common transition color, this class enables visual
/// transformation between an arbitrary pair of UI elements that may differ in position, shape, and size.
/// </summary>
public class UIEvolver : MonoBehaviour {

  #region PUBLIC FIELDS

  public EvolveableUI _A;
  public EvolveableUI _B;

  public Color _transitionColor = Color.white;

  [Header("Transition Curves")]
  public AnimationCurve _movementCurve = AnimationCurve.EaseInOut(0F, 0F, 1F, 1F);
  public AnimationCurve _colorCurve = AnimationCurve.EaseInOut(0F, 0F, 1F, 1F);
  public AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0F, 0F, 1F, 1F);

  #endregion

  #region PRIVATE FIELDS

  private bool _evolving = false;
  private bool _towardsB; // evolution direction
  /// <summary>
  /// 0: Not evolving
  /// 1: Initial white fade out of original (and movement starts)
  /// 2: Original shrinks, target appears and grows
  /// 3: Movement finished, target fades from white, original disappears
  /// </summary>
  private int _evolutionStep = 0;
  private float VERY_SMALL = 0.01F;

  // Evolution step durations. Provided as arguments to Evolveables
  // at each step in the evolution.
  private float _movementDuration = 0.5F;
  private float _fadeToColorDuration = 0.2F;
  private float _sizeTransitionDuration = 0.2F;
  private float _fadeBackToOrigColorDuration = 0.2F;

  // Evolution step timers. Implementers of Evolveable keep
  // track of their own time; these timers are for calling
  // the next evolution steps on Evolveables at the right times.
  private float _movementTimer = 0F;
  private float _fadeToColorTimer = 0F;
  private float _sizeTransitionTimer = 0F;
  private float _fadeBackToOrigColorTimer = 0F;

  #endregion

  #region PROPERTIES

  public bool EvolvingTowardsA {
    get { return !_towardsB;  }
  }
  public bool EvolvingTowardsB {
    get { return _towardsB; }
  }

  private IEvolveable TargetEvolveable {
    get { return (_towardsB ? _B : _A); }
  }
  private Transform TargetTransform {
    get { return (_towardsB ? _B.GetAnchor() : _A.GetAnchor()); }
  }
  private IEvolveable OriginalEvolveable {
    get { return (_towardsB ? _A : _B); }
  }
  private Transform OriginalTransform {
    get { return (_towardsB ? _A.GetAnchor() : _B.GetAnchor()); }
  }

  #endregion

  #region UNITY EVENTS

  public UnityEvent OnFinishedEvolving;
  public UnityEvent OnFinishedEvolvingToA;
  public UnityEvent OnFinishedEvolvingToB;

  #endregion

  #region UNITY CALLBACKS

  protected void Update() {
    UpdateEvolution();

    if (Input.GetKeyDown(KeyCode.Space)) {
      EvolveToB();
    }
  }

  #endregion

  #region PRIVATE METHODS

  private void BeginEvolution() {
    _evolutionStep = 0;
    _evolving = true;
  }

  private void UpdateEvolution() {
    if (_evolving) {
      if (_evolutionStep < 1) {
        // Evolution step one: The original and the target evolveables move along
        // the same path, target invisible, while the original fades
        // to white.
        _evolutionStep = 1;

        // Reset timers
        _movementTimer = 0F;
        _fadeToColorTimer = 0F;
        _sizeTransitionTimer = 0F;
        _fadeBackToOrigColorTimer = 0F;

        // Initiate movement
        TargetEvolveable.MoveFromTo(OriginalTransform, TargetTransform, _movementCurve, _movementDuration);
        OriginalEvolveable.MoveFromTo(OriginalTransform, TargetTransform, _movementCurve, _movementDuration);

        // Initiate transition color fade on original
        OriginalEvolveable.FadeToTransitionColor(_transitionColor, _colorCurve, _fadeToColorDuration);

        // Make sure target is invisible and very small
        TargetEvolveable.SetScale(VERY_SMALL);
        TargetEvolveable.Disappear();
      }
      else {
        // Progress step 1 timers
        _movementTimer += Time.deltaTime;
        _fadeToColorTimer += Time.deltaTime;

        if (_fadeToColorTimer >= _fadeToColorDuration) {
          if (_evolutionStep < 2) {
            // Step two: The target evolveable appears (all white) while
            // it grows from very small to full size as the original
            // shrinks.
            // (Movement may still be happening during this process; that's fine.)
            _evolutionStep = 2;

            // Make target appear and begin growing
            TargetEvolveable.Appear();
            TargetEvolveable.SetToTransitionColor(_transitionColor);
            TargetEvolveable.ChangeToScale(VERY_SMALL, 1F, _scaleCurve, _sizeTransitionDuration);

            // Make original shrink
            OriginalEvolveable.ChangeToScale(1F, VERY_SMALL, _scaleCurve, _sizeTransitionDuration);
          }
          else {
            // Progress step 2 timers
            _sizeTransitionTimer += Time.deltaTime;

            if (_sizeTransitionTimer >= _sizeTransitionDuration) {
              if (_evolutionStep < 3) {
                // Step three: The target evolveable fades in from white
                // and the original disappears completely.
                _evolutionStep = 3;

                // Make original disappear
                OriginalEvolveable.Disappear();

                // Make target fade in
                TargetEvolveable.FadeFromTransitionColor(_transitionColor, _colorCurve, _fadeBackToOrigColorDuration);
              }
              else {
                _fadeBackToOrigColorTimer += Time.deltaTime;

                if (_fadeBackToOrigColorTimer >= _fadeBackToOrigColorDuration) {
                  _evolving = false;
                  OnFinishedEvolving.Invoke();
                  if (_towardsB) {
                    OnFinishedEvolvingToB.Invoke();
                  }
                  else {
                    OnFinishedEvolvingToA.Invoke();
                  }
                }
              }
            }
          }
        }
      }
    }
  }

  #endregion

  #region PUBLIC METHODS

  public void EvolveToA() {
    _towardsB = false;
    BeginEvolution();
  }

  public void EvolveToB() {
    _towardsB = true;
    BeginEvolution();
  }

  #endregion

}
