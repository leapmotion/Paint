using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TweenInternal {

  public struct CanvasGroupInterpolatorSelector {
    private TweenHandle _handle;
    private CanvasGroup _target;

    public CanvasGroupInterpolatorSelector(TweenHandle handle, CanvasGroup target) {
      _handle = handle;
      _target = target;
    }

    public TweenHandle Alpha(float from, float to) {
      _handle.Instance.AddInterpolator(Interpolator.CanvasGroupAlpha(_target, from, to));
      return _handle;
    }

    public TweenHandle ToAlpha(float to) {
      _handle.Instance.AddInterpolator(Interpolator.CanvasGroupAlpha(_target, _target.alpha, to));
      return _handle;
    }
  }

}