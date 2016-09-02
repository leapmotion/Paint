using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TweenInternal;

public interface ITweenObj {
  ITweenObj Value(float from, float to, Action<float> onValue);
  ITweenObj Value(Color from, Color to, Action<Color> onValue);
  ITweenObj Value(Color32 from, Color32 to, Action<Color32> onValue);
  ITweenObj Value(Vector2 from, Vector2 to, Action<Vector2> onValue);
  ITweenObj Value(Vector3 from, Vector3 to, Action<Vector3> onValue);
  ITweenObj Value(Vector4 from, Vector4 to, Action<Vector4> onValue);
  ITweenObj Value(Quaternion from, Quaternion to, Action<Quaternion> onValue);
  MaterialInterpolatorSelector Target(Material material);
  TransformInterpolatorSelector Target(Transform transform);
  CanvasGroupInterpolatorSelector Target(CanvasGroup canvasGroup);

  bool IsRunning { get; set; }
  TweenDirection Direction { get; set; }
  float TimeLeft { get; }
  float Progress { get; set; }

  /// <summary>
  /// Sets the time this tween will take to complete.  Takes effect the next 
  /// time this Tween is started. (If this tween is looping, this is the time 
  /// per loop)
  /// </summary>
  /// <param name="forTime"></param>
  /// <returns></returns>
  ITweenObj OverTime(float forTime);

  /// <summary>
  /// Sets the rate at which this Tween will move at.  Takes effect the next 
  /// time this Tween is started.  This rate is always measured relative to 
  /// the first interpolator specified.
  /// </summary>
  /// <param name="rate"></param>
  /// <returns></returns>
  ITweenObj AtRate(float rate);

  /// <summary>
  /// Sets the smoothing type used for this tween.
  /// </summary>
  /// <param name="TweenType"></param>
  /// <returns></returns>
  ITweenObj Smooth(TweenType TweenType);

  /// <summary>
  /// Specifies a custom smoothing function for the tween.  The function should 
  /// return 0 for an input of 0 and should return 1 for an input of 1.
  /// </summary>
  /// <param name="TweenFunc"></param>
  /// <returns></returns>
  ITweenObj Smooth(Func<float, float> TweenFunc);

  /// <summary>
  /// Specifies a custom smoothing curve for the tween.  The curve should return
  /// 0 for an input of 0, and should return 1 for an input of 1.
  /// </summary>
  /// <param name="TweenCurve"></param>
  /// <returns></returns>
  ITweenObj Smooth(AnimationCurve TweenCurve);

  /// <summary>
  /// Specifies a callback to be called whenever this tween updates its progress.
  /// </summary>
  /// <param name="onProgress"></param>
  /// <returns></returns>
  ITweenObj OnProgress(Action<float> onProgress);

  /// <summary>
  /// Specifies a callback to be called when this tween completes.
  /// </summary>
  /// <param name="onComplete"></param>
  /// <returns></returns>
  ITweenObj OnComplete(Action onComplete);

  /// <summary>
  /// Starts the Tween.
  /// 
  /// The return value is a WaitForSeconds object specifying how much longer the tween
  /// will animate if it does not have it's direction changed.
  /// starting the Tween.
  /// </summary>
  /// <returns></returns>
  WaitForSeconds Play();

  /// <summary>
  /// Identical to Play(), but also sets the direction before starting the Tween.
  /// </summary>
  /// <param name="direction"></param>
  /// <returns></returns>
  WaitForSeconds Play(TweenDirection direction);

  /// <summary>
  /// Pauses the Tween.  When started again it will resume from it's current position. 
  /// </summary>
  void Pause();

  /// <summary>
  /// Stops the Tween and resets it's state back to the begining.
  /// </summary>
  void Stop();
}