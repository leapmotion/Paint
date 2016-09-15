using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TweenInternal {

  public class TweenInstance {
    private static float linearTween(float value) {
      return value;
    }

    private static float smoothTween(float value) {
      return Mathf.SmoothStep(0.0f, 1.0f, value);
    }

    private static float smoothInTween(float value) {
      return 1.0f - Mathf.Pow(1.0f - value, 2.0f);
    }

    private static float smoothOutTween(float value) {
      return Mathf.Pow(value, 2.0f);
    }

    #region SETTING VARIABLES
    private Interpolator[] _interpolators = new Interpolator[2];
    private int _interpolatorCount = 0;

    private Func<float, float> _smoothFunc = linearTween;
    #endregion

    #region RUNTIME VARIABLES
    private float _percent = 0.0f;
    private float _delta = 1.0f;
    private TweenDirection _direction = TweenDirection.FORWARD;
    private float _goalPercent = 1.0f;

    private int _runnerIndex = -1;


    #endregion

    #region Pooling 
    //Starts at index 1, not 0.  0 Is an invalid index!
    private static int _nextInstanceId = 1;
    private int _instanceId = -1;

    //All tweens return to the pool upon complete by default
    private bool _releaseUponComplete = true;

    public int InstanceId {
      get {
        return _instanceId;
      }
    }

    public static void EnsureRegisteredWithPool() {
    }

    static TweenInstance() {
      Pool<TweenInstance>.RegisterInstance(factory, spawnFunc, returnFunc);
    }

    private static TweenInstance factory() {
      return new TweenInstance();
    }

    private static void spawnFunc(TweenInstance toSpawn) {
      toSpawn._instanceId = _nextInstanceId;
      toSpawn._releaseUponComplete = true;

      toSpawn._percent = 0;
      toSpawn._delta = 1.0f;
      toSpawn._direction = TweenDirection.FORWARD;
      toSpawn._goalPercent = 1.0f;
      toSpawn._smoothFunc = linearTween;

      _nextInstanceId++;
    }

    private static void returnFunc(TweenInstance toReturn) {
      for (int i = 0; i < toReturn._interpolatorCount; i++) {
        Pool<Interpolator>.Return(toReturn._interpolators[i]);
        toReturn._interpolators[i] = null;
      }
      toReturn._interpolatorCount = 0;
      toReturn.OnReachStart = null;
      toReturn.OnReachEnd = null;
      toReturn.OnLeaveEnd = null;
      toReturn.OnLeaveStart = null;
      toReturn.OnProgress = null;



      toReturn._instanceId = -1;
    }

    private TweenInstance() { }
    #endregion

    #region Events
    public Action<float> OnProgress = null;
    public Action OnReachStart = null;
    public Action OnLeaveStart = null;
    public Action OnReachEnd = null;
    public Action OnLeaveEnd = null;
    #endregion

    #region Getters and Setters
    public bool IsRunning {
      get {
        return _runnerIndex != -1;
      }
      set {
        if (IsRunning != value) {
          if (IsRunning) {
            Pause();
          } else {
            Play();
          }
        }
      }
    }

    public TweenDirection Direction {
      get {
        return _direction;
      }
      set {
        _direction = value;
        _delta = Mathf.Abs(_delta) * (int)_direction;
        _goalPercent = _direction == TweenDirection.FORWARD ? 1.0f : 0.0f;
      }
    }

    public float TimeLeft {
      get {
        return Mathf.Abs((_percent - _goalPercent) / _delta);
      }
    }

    public float Progress {
      get {
        return _percent;
      }
      set {
        if (_percent == value) {
          return;
        }

        if (_percent == 0.0f) {
          if (OnLeaveStart != null) {
            OnLeaveStart();
          }
        } else if (_percent == 1.0f) {
          if (OnLeaveEnd != null) {
            OnLeaveEnd();
          }
        }

        _percent = value;

        if (_percent == 0.0f) {
          if (OnReachStart != null) {
            OnReachStart();
          }
        } else if (_percent == 1.0f) {
          if (OnReachEnd != null) {
            OnReachEnd();
          }
        }

        if (!IsRunning) {
          float progress = _smoothFunc(_percent);
          interpolatePercent(progress);
        }
      }
    }
    #endregion

    #region Tween Commands
    public void OverTime(float forTime) {
      _delta = (int)_direction / forTime;
    }

    public void AtRate(float rate) {
      OverTime(_interpolators[0].GetLength() / rate);
    }

    public void Smooth(TweenType TweenType) {
      switch (TweenType) {
        case TweenType.LINEAR:
          _smoothFunc = linearTween;
          break;
        case TweenType.SMOOTH:
          _smoothFunc = smoothTween;
          break;
        case TweenType.SMOOTH_END:
          _smoothFunc = smoothInTween;
          break;
        case TweenType.SMOOTH_START:
          _smoothFunc = smoothOutTween;
          break;
        default:
          throw new Exception("Unecpected Tween type " + TweenType);
      }
    }

    public void Smooth(Func<float, float> TweenFunc) {
      _smoothFunc = TweenFunc;
    }

    public void Smooth(AnimationCurve TweenCurve) {
      _smoothFunc = TweenCurve.Evaluate;
    }

    public WaitForSeconds Play(bool restart = false) {
      //If we are already at our destination, no time needs to pass!
      if (_percent == _goalPercent) {
        return null;
      }

      if (_percent == 0.0f) {
        if (OnLeaveStart != null) {
          OnLeaveStart();
        }
      } else if (_percent == 1.0f) {
        if (OnLeaveEnd != null) {
          OnLeaveEnd();
        }
      }

      //If we are already running no need to restart the tween.
      if (!restart) {
        if (IsRunning) {
          return new WaitForSeconds(TimeLeft);
        }
      }

      TweenRunner.Instance.AddTween(this);

      return new WaitForSeconds(TimeLeft);
    }

    public WaitForSeconds Play(TweenDirection direction) {
      Direction = direction;
      return Play(true);
    }

    public WaitForSeconds Play(float destinationPercent) {
      Direction = destinationPercent >= _percent ? TweenDirection.FORWARD : TweenDirection.BACKWARD;
      _goalPercent = destinationPercent;
      return Play(true);
    }

    public void Pause() {
      if (_runnerIndex != -1) {
        TweenRunner.Instance.RemoveTween(this);
      }
    }

    public void Stop() {
      _percent = 0.0f;
      Direction = TweenDirection.FORWARD;
      Pause();
    }

    public void SetRunnerIndex(int index) {
      _runnerIndex = index;
    }

    public int GetRunnerIndex() {
      return _runnerIndex;
    }
    #endregion

    public void AddInterpolator(Interpolator interpolator) {
      if (_interpolators.Length <= _interpolatorCount) {
        Interpolator[] newArray = new Interpolator[_interpolators.Length * 2];
        _interpolators.CopyTo(newArray, 0);
        _interpolators = newArray;
      }

      _interpolators[_interpolatorCount++] = interpolator;
    }

    public void Keep() {
      _releaseUponComplete = false;
    }

    public void Release() {
      if (_runnerIndex != -1) {
        TweenRunner.Instance.RemoveTween(this);
      }

      if (_instanceId != -1) {
        Pool<TweenInstance>.Return(this);
      }
    }

    public bool StepProgress() {
      _percent += _delta * Time.deltaTime;
      if (_percent * (int)_direction >= _goalPercent) {
        _percent = _goalPercent;
      }

      float progress = _smoothFunc(_percent);

      if (OnProgress != null) {
        OnProgress(progress);
      }

      interpolatePercent(progress);

      if (_percent == _goalPercent) {

        if (_percent == 1.0f) {
          if (OnReachEnd != null) {
            OnReachEnd();
          }
        } else if (_percent == 0.0f) {
          if (OnReachStart != null) {
            OnReachStart();
          }
        }

        if (_releaseUponComplete && _instanceId != -1) {
          Release();
        }

        return true;
      }

      return false;
    }

    private void interpolatePercent(float percent) {
      for (int i = _interpolatorCount - 1; i >= 0; --i) {
        Interpolator interpolator = _interpolators[i];
        if (interpolator.IsValid()) {
          _interpolators[i].Interpolate(percent);
        } else {
          _interpolators[i] = _interpolators[--_interpolatorCount];
        }
      }
    }
  }

}