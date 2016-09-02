using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TweenInternal;

public static class Tween {
  public static TweenHandle Value(float from, float to, Action<float> onValue) {
    return NewTween().Value(from, to, onValue);
  }

  public static TweenHandle Value(Color from, Color to, Action<Color> onValue) {
    return NewTween().Value(from, to, onValue);
  }

  public static TweenHandle Value(Vector2 from, Vector2 to, Action<Vector2> onValue) {
    return NewTween().Value(from, to, onValue);
  }

  public static TweenHandle Value(Vector3 from, Vector3 to, Action<Vector3> onValue) {
    return NewTween().Value(from, to, onValue);
  }

  public static TweenHandle Value(Vector4 from, Vector4 to, Action<Vector4> onValue) {
    return NewTween().Value(from, to, onValue);
  }

  public static TweenHandle Value(Quaternion from, Quaternion to, Action<Quaternion> onValue) {
    return NewTween().Value(from, to, onValue);
  }

  public static MaterialInterpolatorSelector Target(Material material) {
    return NewTween().Target(material);
  }

  public static TransformInterpolatorSelector Target(Transform transform) {
    return NewTween().Target(transform);
  }

  public static CanvasGroupInterpolatorSelector Target(CanvasGroup canvasGroup) {
    return NewTween().Target(canvasGroup);
  }

  public static TweenHandle AfterDelay(float delay, Action action) {
    var newTween = NewTween().OverTime(delay).OnReachEnd(action);
    newTween.Play();
    return newTween;
  }

  private static TweenHandle NewTween() {
    TweenInstance.EnsureRegisteredWithPool();
    TweenInstance instance = Pool<TweenInstance>.Spawn();
    return new TweenHandle(instance);
  }
}
