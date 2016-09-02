using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TweenInternal {

  public class MaterialInterpolatorSelector {
    private Material _target;
    private TweenHandle _handle;

    public MaterialInterpolatorSelector(TweenHandle handle, Material target) {
      _target = target;
      _handle = handle;
    }

    public TweenHandle Color(Color from, Color to, string propertyName = "_Color") {
      _handle.Instance.AddInterpolator(Interpolator.MaterialColor(_target, from, to, propertyName));
      return _handle;
    }

    public TweenHandle ToColor(Color to, string propertyName = "_Color") {
      _handle.Instance.AddInterpolator(Interpolator.MaterialColor(_target, to, propertyName));
      return _handle;
    }

    public TweenHandle Gradient(Gradient gradient, string propertyName = "_Color") {
      _handle.Instance.AddInterpolator(Interpolator.MaterialGradient(_target, gradient, propertyName));
      return _handle;
    }

    public TweenHandle Alpha(float from, float to, string propertyName = "_Color") {
      _handle.Instance.AddInterpolator(Interpolator.MaterialAlpha(_target, from, to, propertyName));
      return _handle;
    }

    public TweenHandle ToAlpha(float to, string propertyName = "_Color") {
      _handle.Instance.AddInterpolator(Interpolator.MaterialAlpha(_target, to, propertyName));
      return _handle;
    }

    public TweenHandle Value(float from, float to, string propertyName) {
      _handle.Instance.AddInterpolator(Interpolator.MaterialValue(_target, from, to, propertyName));
      return _handle;
    }

    public TweenHandle ToValue(float to, string propertyName) {
      _handle.Instance.AddInterpolator(Interpolator.MaterialValue(_target, to, propertyName));
      return _handle;
    }
  }

}