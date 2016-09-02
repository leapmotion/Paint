using UnityEngine;
using System;
using TweenInternal;

public struct TweenHandle {

  private TweenInstance _instance;
  private int _instanceId;

  public TweenInstance Instance {
    get {
      if (!IsValid) {
        throw new Exception("Invalid TweenHandle!  If you want to keep a tween around after it finishes, make sure to call Keep()!");
      }
      return _instance;
    }
  }

  public TweenHandle(TweenInstance instance) {
    _instance = instance;
    _instanceId = _instance.InstanceId;
  }

  public bool IsValid {
    get {
      return _instance != null && _instance.InstanceId == _instanceId;
    }
  }

  public TweenHandle Value(float from, float to, Action<float> onValue) {
    Instance.AddInterpolator(Interpolator.Value(from, to, onValue));
    return this;
  }

  public TweenHandle Value(Color from, Color to, Action<Color> onValue) {
    Instance.AddInterpolator(Interpolator.Value(from, to, onValue));
    return this;
  }

  public TweenHandle Value(Vector2 from, Vector2 to, Action<Vector2> onValue) {
    Instance.AddInterpolator(Interpolator.Value(from, to, onValue));
    return this;
  }

  public TweenHandle Value(Vector3 from, Vector3 to, Action<Vector3> onValue) {
    Instance.AddInterpolator(Interpolator.Value(from, to, onValue));
    return this;
  }

  public TweenHandle Value(Vector4 from, Vector4 to, Action<Vector4> onValue) {
    Instance.AddInterpolator(Interpolator.Value(from, to, onValue));
    return this;
  }

  public TweenHandle Value(Quaternion from, Quaternion to, Action<Quaternion> onValue) {
    Instance.AddInterpolator(Interpolator.Value(from, to, onValue));
    return this;
  }

  public MaterialInterpolatorSelector Target(Material material) {
    return new MaterialInterpolatorSelector(this, material);
  }

  public TransformInterpolatorSelector Target(Transform transform) {
    return new TransformInterpolatorSelector(this, transform);
  }

  public CanvasGroupInterpolatorSelector Target(CanvasGroup canvasGroup) {
    return new CanvasGroupInterpolatorSelector(this, canvasGroup);
  }

  /// <summary>
  /// Tweens destroy themselves by default when they complete.  Call Keep() to have the tween stay around
  /// so that you can do more things with it like start it again or change parameters in real time.
  /// </summary>
  public TweenHandle Keep() {
    _instance.Keep();
    return this;
  }

  /// <summary>
  /// Immediately releases the tween and allows it's resources to be reused.  This TweenHandle becomes invalid
  /// right away, and can no longer be used.  
  /// </summary>
  public void Release() {
    _instance.Release();
  }

  /// <summary>
  /// Gets or sets whether or not the Tween is currently animating or not.
  /// </summary>
  public bool IsRunning {
    get {
      return Instance.IsRunning;
    }
    set {
      Instance.IsRunning = value;
    }
  }

  /// <summary>
  /// Gets or sets whether or not the Tween is animating forward or backwards.
  /// 
  /// If the Tween is not currently playing, the result represents the direction the tween will
  /// animate if it is started by the argument-less Play() method.
  /// </summary>
  public TweenDirection Direction {
    get {
      return Instance.Direction;
    }
    set {
      Instance.Direction = value;
    }
  }

  /// <summary>
  /// Returns the time in seconds left before this tween will complete animation. 
  /// </summary>
  public float TimeLeft {
    get {
      return Instance.TimeLeft;
    }
  }

  /// <summary>
  /// Returns the percent through the animation the tween is currently in.
  /// </summary>
  public float Progress {
    get {
      return Instance.Progress;
    }
    set {
      Instance.Progress = value;
    }
  }

  /// <summary>
  /// Sets the time this tween will take to complete.  Takes effect the next 
  /// time this Tween is started. (If this tween is looping, this is the time 
  /// per loop)
  /// </summary>
  public TweenHandle OverTime(float forTime) {
    Instance.OverTime(forTime);
    return this;
  }

  /// <summary>
  /// Sets the rate at which this Tween will move at.  Takes effect the next 
  /// time this Tween is started.  This rate is always measured relative to 
  /// the first interpolator specified.
  /// </summary>
  public TweenHandle AtRate(float rate) {
    Instance.OverTime(rate);
    return this;
  }

  /// <summary>
  /// Sets the smoothing type used for this tween.
  /// </summary>
  public TweenHandle Smooth(TweenType tweenType) {
    Instance.Smooth(tweenType);
    return this;
  }

  /// <summary>
  /// Specifies a custom smoothing function for the tween.  The function should 
  /// return 0 for an input of 0 and should return 1 for an input of 1.
  /// </summary>
  public TweenHandle Smooth(Func<float, float> tweenFunc) {
    Instance.Smooth(tweenFunc);
    return this;
  }

  /// <summary>
  /// Specifies a custom smoothing curve for the tween.  The curve should return
  /// 0 for an input of 0, and should return 1 for an input of 1.
  /// </summary>
  public TweenHandle Smooth(AnimationCurve tweenCurve) {
    Instance.Smooth(tweenCurve);
    return this;
  }

  /// <summary>
  /// Specifies a callback to be called whenever this tween updates its progress.
  /// </summary>
  public TweenHandle OnProgress(Action<float> onProgress) {
    Instance.OnProgress += onProgress;
    return this;
  }

  /// <summary>
  /// Specifies a callback to be called when this tween reaches the start position.
  /// </summary>
  public TweenHandle OnReachStart(Action onReachStart) {
    Instance.OnReachStart += onReachStart;
    return this;
  }

  /// <summary>
  /// Specifies a callback to be called when this tween leaves the start position.
  /// </summary>
  public TweenHandle OnLeaveStart(Action onLeaveStart) {
    Instance.OnLeaveStart += onLeaveStart;
    return this;
  }

  /// <summary>
  /// Specifies a callback to be called when this tween reaches the end position.
  /// </summary>
  public TweenHandle OnReachEnd(Action onReachEnd) {
    Instance.OnReachEnd += onReachEnd;
    return this;
  }

  /// <summary>
  /// Specifies a callback to be called when this tween leaves the end position.
  /// </summary>
  public TweenHandle OnLeaveEnd(Action onLeaveEnd) {
    Instance.OnLeaveEnd += onLeaveEnd;
    return this;
  }

  /// <summary>
  /// Starts the Tween.
  /// 
  /// The return value is a WaitForSeconds object specifying how much longer the tween
  /// will animate if it does not have it's direction changed.
  /// starting the Tween.
  /// </summary>
  public WaitForSeconds Play() {
    return Instance.Play();
  }

  /// <summary>
  /// Identical to Play(), but also sets the direction before starting the Tween.
  /// </summary>
  public WaitForSeconds Play(TweenDirection direction) {
    return Instance.Play(direction);
  }

  public WaitForSeconds Play(float goalPercent) {
    return Instance.Play(goalPercent);
  }

  /// <summary>
  /// Pauses the Tween.  When started again it will resume from it's current position. 
  /// </summary>
  public void Pause() {
    Instance.Pause();
  }

  /// <summary>
  /// Stops the Tween and resets it's state back to the begining.
  /// </summary>
  public void Stop() {
    Instance.Stop();
  }
}

