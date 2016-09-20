using UnityEngine;
using System;
using Leap;
using Leap.Unity;

public class DisconnectionHandler : MonoBehaviour {

  public static event Action OnChange;
  public Renderer _disconnectNotifRenderer;

  [SerializeField]
  private LeapServiceProvider _provider;

  private TweenHandle _transition;
  private Controller _controller;
  private bool _lastConnectionReport = false;
  private float _startupDelay = 0.5F;

  void Awake() {
    _transition = Tween.Target(_disconnectNotifRenderer.material).Value(0F, 1F, "_Alpha").
                        OverTime(0.5f).
                        Smooth(TweenType.SMOOTH).
                        OnLeaveStart(DoOnLeaveStart).
                        OnReachStart(DoOnReachStart).
                        Keep();
  }

  void Start() {
    _controller = _provider.GetLeapController();

    _transition.Progress = 0.001F;
    _transition.Play(TweenDirection.BACKWARD);
  }

  void Update() {
    if (_startupDelay > 0F) {
      _startupDelay -= Time.deltaTime;
    }
    else {
      if (_lastConnectionReport != _controller.IsConnected) {
        _lastConnectionReport = _controller.IsConnected;
        if (OnChange != null) {
          OnChange();
        }
      }

      _transition.Play(_lastConnectionReport ? TweenDirection.BACKWARD : TweenDirection.FORWARD);
    }
  }

  void DoOnLeaveStart() {
    _disconnectNotifRenderer.enabled = true;
  }

  void DoOnReachStart() {
    _disconnectNotifRenderer.enabled = false;
  }
}
