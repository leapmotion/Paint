using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TweenInternal {

  public class TransformInterpolatorSelector {
    private Transform _target;
    private TweenHandle _handle;

    public TransformInterpolatorSelector(TweenHandle handle, Transform target) {
      _handle = handle;
      _target = target;
    }

    #region GLOBAL
    //GLOBAL TRANSFORM
    public TweenHandle Transform(Transform from, Transform to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformGlobal(_target, from, to));
      return _handle;
    }

    public TweenHandle To(Transform to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformGlobal(_target, to));
      return _handle;
    }

    public TweenHandle To(Vector3 toPosition, Quaternion toRotation, Vector3 toLocalScale) {
      _handle.Instance.AddInterpolator(Interpolator.TransformGlobalPosition(_target, toPosition));
      _handle.Instance.AddInterpolator(Interpolator.TransformGlobalRotation(_target, toRotation));
      _handle.Instance.AddInterpolator(Interpolator.TransformLocalScale(_target, toLocalScale));
      return _handle;
    }

    //GLOBAL POSITION
    public TweenHandle Position(Transform from, Transform to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformGlobalPosition(_target, from.position, to.position));
      return _handle;
    }

    public TweenHandle Position(Vector3 from, Vector3 to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformGlobalPosition(_target, from, to));
      return _handle;
    }

    public TweenHandle ToPosition(Transform to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformGlobalPosition(_target, to.position));
      return _handle;
    }

    public TweenHandle ToPosition(Vector3 to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformGlobalPosition(_target, to));
      return _handle;
    }

    //GLOBAL ROTATION
    public TweenHandle Rotation(Transform from, Transform to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformGlobalRotation(_target, from.rotation, to.rotation));
      return _handle;
    }

    public TweenHandle Rotation(Quaternion from, Quaternion to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformGlobalRotation(_target, from, to));
      return _handle;
    }

    public TweenHandle ToRotation(Transform to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformGlobalRotation(_target, to.rotation));
      return _handle;
    }

    public TweenHandle ToRotation(Quaternion to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformGlobalRotation(_target, to));
      return _handle;
    }

    #endregion
    //LOCAL TRANSFORM
    public TweenHandle LocalTransform(Transform from, Transform to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformLocal(_target, from, to));
      return _handle;
    }

    public TweenHandle ToLocal(Transform to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformLocal(_target, to));
      return _handle;
    }

    public TweenHandle ToLocal(Vector3 toLocalPosition, Quaternion toLocalRotation, Vector3 toLocalScale) {
      _handle.Instance.AddInterpolator(Interpolator.TransformLocalPosition(_target, _target.localPosition, toLocalPosition));
      _handle.Instance.AddInterpolator(Interpolator.TransformLocalRotation(_target, toLocalRotation));
      _handle.Instance.AddInterpolator(Interpolator.TransformLocalScale(_target, toLocalScale));
      return _handle;
    }

    //LOCAL POSITION
    public TweenHandle LocalPosition(Transform from, Transform to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformLocalPosition(_target, from.localPosition, to.localPosition));
      return _handle;
    }

    public TweenHandle LocalPosition(Vector3 from, Vector3 to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformLocalPosition(_target, from, to));
      return _handle;
    }

    public TweenHandle ToLocalPosition(Transform to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformLocalPosition(_target, to.localPosition));
      return _handle;
    }

    public TweenHandle ToLocalPosition(Vector3 to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformLocalPosition(_target, to));
      return _handle;
    }

    //LOCAL ROTATION
    public TweenHandle LocalRotation(Transform from, Transform to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformLocalRotation(_target, from.localRotation, to.localRotation));
      return _handle;
    }

    public TweenHandle LocalRotation(Quaternion from, Quaternion to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformLocalRotation(_target, from, to));
      return _handle;
    }

    public TweenHandle ToLocalRotation(Transform to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformLocalRotation(_target, to.localRotation));
      return _handle;
    }

    public TweenHandle ToLocalRotation(Quaternion to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformLocalRotation(_target, to));
      return _handle;
    }

    //LOCAL SCALE
    public TweenHandle LocalScale(Transform from, Transform to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformLocalScale(_target, from.localScale, to.localScale));
      return _handle;
    }

    public TweenHandle LocalScale(Vector3 from, Vector3 to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformLocalScale(_target, from, to));
      return _handle;
    }

    public TweenHandle ToLocalScale(Transform to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformLocalScale(_target, to.localScale));
      return _handle;
    }

    public TweenHandle ToLocalScale(Vector3 to) {
      _handle.Instance.AddInterpolator(Interpolator.TransformLocalScale(_target, to));
      return _handle;
    }
  }

}