using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class ExpandableUI : MonoBehaviour {

  //public EvolveableBehaviour _staticElement;
  public EvolveableUI _expansionElement;

  public bool startExpanded = false;

  public UnityEvent OnBehaviourStart;
  public UnityEvent OnExpansionBegin;
  public UnityEvent OnExpansionComplete;
  public UnityEvent OnRetractionBegin;
  public UnityEvent OnRetractionComplete;

  private const float VERY_SMALL = 0.01F;

  private bool _expanding = false;
  private bool _retracting = false;
  private int _expansionStep = 0;

  private float _scaleDuration = 0.125F;
  private float _scaleTimer = 0F;

  private float _fadeDuration = 0.125F;
  private float _fadeTimer = 0F;

  private bool _lastActionWasExpand = false;


  protected void Start() {
    if (startExpanded) {
      _expansionElement.ChangeToSize(1F, 1F, 0F);
      _expansionElement.FadeFromWhite(0F);
      _expansionElement.Appear(false);
      _lastActionWasExpand = true;
    }
    else {
      _expansionElement.ChangeToSize(VERY_SMALL, VERY_SMALL, 0F);
      _expansionElement.Disappear();
      _lastActionWasExpand = false;
    }
    OnBehaviourStart.Invoke();
  }


  protected void Update() {
    if (_expanding) {
      if (_expansionStep < 1) {
        _expansionElement.Appear(true);
        _expansionElement.ChangeToSize(VERY_SMALL, 1F, _scaleDuration);
        _expansionStep = 1;
        OnExpansionBegin.Invoke();
      }
      else {
        _scaleTimer += Time.deltaTime;

        if (_scaleTimer >= _scaleDuration) {
          if (_expansionStep < 2) {
            // scaling done, fade in
            _expansionElement.FadeFromWhite(_fadeDuration);
            _expansionStep = 2;
          }
          else {
            _fadeTimer += Time.deltaTime;

            if (_fadeTimer >= _fadeDuration) {
              _expanding = false;
              OnExpansionComplete.Invoke();
            }
          }
        }
      }
    }
    else if (_retracting) {
      if (_expansionStep < 1) {
        _expansionElement.FadeToWhite(_fadeDuration);
        _expansionStep = 1;
        OnRetractionBegin.Invoke();
      }
      else {
        _fadeTimer += Time.deltaTime;

        if (_fadeTimer >= _fadeDuration) {
          if (_expansionStep < 2) {
            // fading done, scale down
            _expansionElement.ChangeToSize(1F, VERY_SMALL, _scaleDuration);
            _expansionStep = 2;
          }
          else {
            _scaleTimer += Time.deltaTime;

            if (_scaleTimer >= _scaleDuration) {
              _expansionElement.Disappear();
              _retracting = false;
              OnRetractionComplete.Invoke();
            }
          }
        }
      }
    }
  }


  public void Expand() {
    _expanding = true;
    _retracting = false;
    _expansionStep = 0;
    _scaleTimer = 0F;
    _fadeTimer = 0F;
    _lastActionWasExpand = true;
  }

  public void Retract() {
    _expanding = false;
    _retracting = true;
    _expansionStep = 0;
    _scaleTimer = 0F;
    _fadeTimer = 0F;
    _lastActionWasExpand = false;
  }

  public void Toggle() {
    if (_lastActionWasExpand) {
      Retract();
    }
    else {
      Expand();
    }
  }

}
