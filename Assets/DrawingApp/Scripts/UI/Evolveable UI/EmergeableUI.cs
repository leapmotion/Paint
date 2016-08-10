using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

public class EmergeableUI : MonoBehaviour {

  #region PUBLIC FIELDS

  public EvolveableUI _evolveableUI;
  public Color _transitionColor;
  public bool _startAppeared = false;

  [Header("Optional: Non-null allows movement during transitions.")]
  public Transform _emergeBeginAnchor;

  [Header("Transition Curves")]
  public AnimationCurve _movementCurve = AnimationCurve.EaseInOut(0F, 0F, 1F, 1F);
  public AnimationCurve _colorCurve = AnimationCurve.EaseInOut(0F, 0F, 1F, 1F);
  public AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0F, 0F, 1F, 1F);

  #endregion

  #region PRIVATE FIELDS

  private bool _evolving = false;
  private int _evolutionStep = 0;
  private bool _emerging = false;

  private const float VERY_SMALL = 0.01F;

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

  private bool _locked = false;

  #endregion

  #region PROPERTIES

  public bool IsEmerged {
    get;
    private set;
  }

  public bool IsVanished {
    get;
    private set;
  }

  #endregion

  #region EVENTS

  [Header("Callbacks")]
  public UnityEvent OnBegunEmerging;
  public UnityEvent OnFinishedEmerging;
  public UnityEvent OnBegunVanishing;
  public UnityEvent OnFinishedVanishing;

  #endregion

  #region UNITY CALLBACKS

  protected void Start() {
    if (!_startAppeared) {
      InstantDisappear();
    }
    else {
      InstantAppear();
    }
  }

  protected void Update() {
    if (_evolving) {
      if (_emerging) {
        UpdateEmerging();
      }
      else {
        UpdateVanishing();
      }
    }
  }

  #endregion

  #region PRIVATE METHODS

  private void BeginEvolution() {
    _evolutionStep = 0;
    _evolving = true;
  }

  private void UpdateEmerging() {
    if (_evolving) {
      if (_evolutionStep < 1) {
        // Emerging step one: The evolveable UI appears (very small, transition color)
        // optionally begins moving from the emerge anchor to its own anchor position,
        // and begins scaling from VERY_SMALL to full size.
        _evolutionStep = 1;

        // Reset timers
        _movementTimer = 0F;
        _fadeToColorTimer = 0F;
        _sizeTransitionTimer = 0F;
        _fadeBackToOrigColorTimer = 0F;

        // Initiate movement
        if (_emergeBeginAnchor != null) {
          _evolveableUI.MoveFromTo(_emergeBeginAnchor, _evolveableUI.GetAnchor(), _movementCurve, _movementDuration);
        }

        // Initiate scaling
        _evolveableUI.ChangeToScale(VERY_SMALL, 1F, _scaleCurve, _sizeTransitionDuration);

        // Make sure the UI emerging is visible and starting in the transition color
        _evolveableUI.Appear();
        _evolveableUI.SetToTransitionColor(_transitionColor);
      }
      else {
        // Progress step 1 timers
        _movementTimer += Time.deltaTime;
        _sizeTransitionTimer += Time.deltaTime;

        if (_sizeTransitionTimer >= _sizeTransitionDuration) {
          if (_evolutionStep < 2) {
            // Step two: The target evolveable begins fading from the
            // transition color to its original color(s).
            // (Movement may still be happening during this process; that's fine.)
            _evolutionStep = 2;

            // Make target fade in from the transition color
            _evolveableUI.FadeFromTransitionColor(_transitionColor, _colorCurve, _fadeBackToOrigColorDuration);
          }
          else {
            // Progress step 2 timers
            _fadeBackToOrigColorTimer += Time.deltaTime;

            if (_fadeBackToOrigColorTimer >= _fadeBackToOrigColorDuration) {
              // Once the evolveable is back to its original color, the emergence is done.
              _evolving = false;
              OnFinishedEmerging.Invoke();
            }
          }
        }
      }
    }
  }

  private void UpdateVanishing() {
    if (_evolving) {
      if (_evolutionStep < 1) {
        // Vanishing step one: The EvolveableUI begins fading back to its transition color.
        _evolutionStep = 1;

        // Reset timers
        _movementTimer = 0F;
        _fadeToColorTimer = 0F;
        _sizeTransitionTimer = 0F;
        _fadeBackToOrigColorTimer = 0F;

        // Make sure the UI emerging is visible and starting in original colors
        _evolveableUI.Appear();
        _evolveableUI.SetToOriginalColor();

        // Begin fade to transition color
        _evolveableUI.FadeToTransitionColor(_transitionColor, _colorCurve, _fadeToColorDuration);
      }
      else {
        // Progress step 1 timers
        _fadeToColorTimer += Time.deltaTime;

        if (_fadeToColorTimer >= _fadeToColorDuration) {
          if (_evolutionStep < 2) {
            // Step two: The Evolveable begins moving back to the emerge anchor position
            // (if it has one) and starts to shrink down to nothing.
            _evolutionStep = 2;

            // Optionally initiate movement
            if (_emergeBeginAnchor != null) {
              _evolveableUI.MoveFromTo(_evolveableUI.GetAnchor(), _emergeBeginAnchor, _movementCurve, _movementDuration);
            }

            // Initiate scaling down
            _evolveableUI.ChangeToScale(1F, VERY_SMALL, _scaleCurve, _sizeTransitionDuration);
          }
          else {
            // Progress step 2 timers
            _movementTimer += Time.deltaTime;
            _sizeTransitionTimer += Time.deltaTime;

            if (_sizeTransitionTimer >= _sizeTransitionDuration) {
              // Once the evolveable has reached a VERY_SMALL size, the vanishing is done.
              _evolving = false;
              _evolveableUI.Disappear();
              OnFinishedVanishing.Invoke();
            }
          }
        }
      }
    }
  }

  #endregion

  #region PUBLIC METHODS

  public void Emerge() {
    if (!_locked) {
      OnBegunEmerging.Invoke();
      BeginEvolution();
      _evolving = true;
      _emerging = true;
      IsEmerged = true;
    }
  }

  public void EnsureEmerged() {
    if (!_emerging) {
      Emerge();
    }
  }

  public void Vanish() {
    if (!_locked) {
      OnBegunVanishing.Invoke();
      BeginEvolution();
      _evolving = true;
      _emerging = false;
      IsVanished = true;
    }
  }

  public void EnsureVanished() {
    if (_emerging) {
      Vanish();
    }
  }

  public void Lock() {
    _locked = true;
  }

  public void Unlock() {
    _locked = false;
  }

  public void Toggle() {
    if (_emerging) {
      Vanish();
    }
    else {
      Emerge();
    }
  }

  public void InstantAppear() {
    _evolveableUI.SetScale(1F);
    _evolveableUI.SetToOriginalColor();
    _evolveableUI.Appear();
  }

  public void InstantDisappear() {
    _evolveableUI.SetScale(VERY_SMALL);
    _evolveableUI.SetToTransitionColor(_transitionColor);
    _evolveableUI.Disappear();
  }

  #endregion

}
