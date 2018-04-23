using UnityEngine;
using System;
using Leap;
using Leap.Unity;
using Leap.Unity.Animation;
namespace Leap.Unity.LeapPaint_v3 {

  public class DisconnectionHandler : MonoBehaviour {

    public static event Action OnChange;
    public Renderer _disconnectNotifRenderer;

    [SerializeField]
    private LeapServiceProvider _provider;

    private Tween _transition;
    private Controller _controller;
    private bool _lastConnectionReport = false;
    private float _startupDelay = 0.5F;

    void Awake() {
      _transition = Tween.Persistent()
                         .Target(_disconnectNotifRenderer.material).Alpha(0F, 1F, "_Alpha")
                         .OverTime(0.5f)
                         .Smooth(SmoothType.Smooth)
                         .OnLeaveStart(DoOnLeaveStart)
                         .OnReachStart(DoOnReachStart);
    }

    void Start() {
      _controller = _provider.GetLeapController();

      _transition.progress = 0.001F;
      _transition.Play(Direction.Backward);
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

        _transition.Play(_lastConnectionReport ? Direction.Backward: Direction.Forward);
      }
    }

    void DoOnLeaveStart() {
      _disconnectNotifRenderer.enabled = true;
    }

    void DoOnReachStart() {
      _disconnectNotifRenderer.enabled = false;
    }
  }


}
