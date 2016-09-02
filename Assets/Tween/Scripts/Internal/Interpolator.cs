using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TweenInternal {

  public class Interpolator {
    public enum InterpolatorType {
      ValueFloat,
      ValueColor,
      ValueGradient,
      ValueVec2,
      ValueVec3,
      ValueVec4,
      ValueRot,
      
      TransformGlobal,
      TransformGlobalPosition,
      TransformGlobalRotation,
      TransformLocal,
      TransformLocalPosition,
      TransformLocalRotation,
      TransformLocalScale,

      MaterialColor,
      MaterialGradient,
      MaterialAlpha,
      MaterialValue,

      CanvasGroupAlpha,
    }

    private float _floatA, _floatB;
    private Color _colorA, _colorB;
    private Gradient _gradient;
    private Vector2 _vec2A, _vec2B;
    private Vector3 _vec3A, _vec3B;
    private Vector3 _vec4A, _vec4B;
    private Quaternion _rotA, _rotB;

    //Callback interpolators
    private Action<float> _actionFloat;
    private Action<Color> _actionColor;
    private Action<Vector2> _actionVec2;
    private Action<Vector3> _actionVec3;
    private Action<Vector4> _actionVec4;
    private Action<Quaternion> _actionRot;

    //Object interpolators
    private Transform _targetTransform;

    private Material _targetMaterial;
    private int _materialId;

    private CanvasGroup _targetCanvasGroup;

    private float _length;
    private InterpolatorType _interpolatorType;

    static Interpolator() {
      Pool<Interpolator>.RegisterInstance(factory, onSpawn, onReturn);
    }

    private static Interpolator factory() {
      return new Interpolator();
    }

    private static void onSpawn(Interpolator interpolator) {
    }

    private static void onReturn(Interpolator interpolator) {
      interpolator._targetTransform = null;
      interpolator._targetMaterial = null;
      interpolator._targetCanvasGroup = null;
      interpolator._gradient = null;
    }

    private static Interpolator GetInstance(InterpolatorType type, float length) {
      Interpolator interpolator = Pool<Interpolator>.Spawn();
      interpolator._interpolatorType = type;
      interpolator._length = Mathf.Abs(length);
      return interpolator;
    }

    #region Value Interpolators
    public static Interpolator Value(float from, float to, Action<float> action) {
      Interpolator interpolator = GetInstance(InterpolatorType.ValueFloat, to - from);
      interpolator._floatA = from;
      interpolator._floatB = to - from;
      interpolator._actionFloat = action;
      return interpolator;
    }

    public static Interpolator Value(Color from, Color to, Action<Color> action) {
      Interpolator interpolator = GetInstance(InterpolatorType.ValueColor, ((Vector4)(from - to)).magnitude);
      interpolator._colorA = from;
      interpolator._colorB = to - from;
      interpolator._actionColor = action;
      return interpolator;
    }

    public static Interpolator Value(Gradient gradient, Action<Color> action) {
      Interpolator interpolator = GetInstance(InterpolatorType.ValueGradient, 1);
      interpolator._gradient = gradient;
      interpolator._actionColor = action;
      return interpolator;
    }

    public static Interpolator Value(Vector2 from, Vector2 to, Action<Vector2> action) {
      Interpolator interpolator = GetInstance(InterpolatorType.ValueFloat, (to - from).magnitude);
      interpolator._vec2A = from;
      interpolator._vec2B = to - from;
      interpolator._actionVec2 = action;
      return interpolator;
    }

    public static Interpolator Value(Vector3 from, Vector3 to, Action<Vector3> action) {
      Interpolator interpolator = GetInstance(InterpolatorType.ValueFloat, (to - from).magnitude);
      interpolator._vec3A = from;
      interpolator._vec3B = to - from;
      interpolator._actionVec3 = action;
      return interpolator;
    }

    public static Interpolator Value(Vector4 from, Vector4 to, Action<Vector4> action) {
      Interpolator interpolator = GetInstance(InterpolatorType.ValueFloat, (to - from).magnitude);
      interpolator._vec4A = from;
      interpolator._vec4B = to - from;
      interpolator._actionVec4 = action;
      return interpolator;
    }

    public static Interpolator Value(Quaternion from, Quaternion to, Action<Quaternion> action) {
      Interpolator interpolator = GetInstance(InterpolatorType.ValueFloat, Quaternion.Angle(from, to));
      interpolator._rotA = from;
      interpolator._rotB = to;
      interpolator._actionRot = action;
      return interpolator;
    }
    #endregion

    #region Transform Interpolators
    public static Interpolator TransformGlobal(Transform target, Transform from, Transform to) {
      Interpolator interpolator = GetInstance(InterpolatorType.TransformGlobal, (from.position - to.position).magnitude);
      interpolator._targetTransform = target;
      interpolator._vec3A = from.position;
      interpolator._vec3B = to.position - from.position;

      interpolator._rotA = from.rotation;
      interpolator._rotB = to.rotation;

      return interpolator;
    }

    public static Interpolator TransformGlobal(Transform target, Transform to) {
      return TransformGlobal(target, target, to);
    }

    public static Interpolator TransformGlobalPosition(Transform target, Vector3 from, Vector3 to) {
      Interpolator interpolator = GetInstance(InterpolatorType.TransformGlobalPosition, (from - to).magnitude);
      interpolator._targetTransform = target;
      interpolator._vec3A = from;
      interpolator._vec3B = to - from;
      return interpolator;
    }

    public static Interpolator TransformGlobalPosition(Transform target, Vector3 to) {
      return TransformGlobalPosition(target, target.position, to);
    }

    public static Interpolator TransformGlobalRotation(Transform target, Quaternion from, Quaternion to) {
      Interpolator interpolator = GetInstance(InterpolatorType.TransformGlobalRotation, Quaternion.Angle(from, to));
      interpolator._targetTransform = target;
      interpolator._rotA = from;
      interpolator._rotB = to;
      return interpolator;
    }

    public static Interpolator TransformGlobalRotation(Transform target, Quaternion to) {
      return TransformGlobalRotation(target, target.rotation, to);
    }

    public static Interpolator TransformLocal(Transform target, Transform from, Transform to) {
      Interpolator interpolator = GetInstance(InterpolatorType.TransformLocal, (from.localPosition - to.localPosition).magnitude);
      interpolator._targetTransform = target;
      interpolator._vec3A = from.localPosition;
      interpolator._vec3B = to.localPosition - from.localPosition;

      interpolator._rotA = from.localRotation;
      interpolator._rotB = to.localRotation;

      return interpolator;
    }

    public static Interpolator TransformLocal(Transform target, Transform to) {
      return TransformLocal(target, target, to);
    }

    public static Interpolator TransformLocalPosition(Transform target, Vector3 from, Vector3 to) {
      Interpolator interpolator = GetInstance(InterpolatorType.TransformLocalPosition, (from - to).magnitude);
      interpolator._targetTransform = target;
      interpolator._vec3A = from;
      interpolator._vec3B = to - from;
      return interpolator;
    }

    public static Interpolator TransformLocalPosition(Transform target, Vector3 to) {
      return TransformLocalPosition(target, target.localPosition, to);
    }

    public static Interpolator TransformLocalRotation(Transform target, Quaternion from, Quaternion to) {
      Interpolator interpolator = GetInstance(InterpolatorType.TransformLocalRotation, Quaternion.Angle(from, to));
      interpolator._targetTransform = target;
      interpolator._rotA = from;
      interpolator._rotB = to;
      return interpolator;
    }

    public static Interpolator TransformLocalRotation(Transform target, Quaternion to) {
      return TransformLocalRotation(target, target.localRotation, to);
    }

    public static Interpolator TransformLocalScale(Transform target, Vector3 from, Vector3 to) {
      Interpolator interpolator = GetInstance(InterpolatorType.TransformLocalScale, (from - to).magnitude);
      interpolator._targetTransform = target;
      interpolator._vec3A = from;
      interpolator._vec3B = to - from;
      return interpolator;
    }

    public static Interpolator TransformLocalScale(Transform target, Vector3 to) {
      return TransformLocalScale(target, target.localScale, to);
    }
    #endregion

    #region Material Interpolators
    public static Interpolator MaterialColor(Material target, Color from, Color to, string propertyName) {
      Interpolator interpolator = GetInstance(InterpolatorType.MaterialColor, ((Vector4)(from - to)).magnitude);
      interpolator._materialId = Shader.PropertyToID(propertyName);
      interpolator._targetMaterial = target;
      interpolator._colorA = from;
      interpolator._colorB = to - from;
      return interpolator;
    }

    public static Interpolator MaterialColor(Material target, Color to, string propertyName) {
      return MaterialColor(target, target.GetColor(propertyName), to, propertyName);
    }

    public static Interpolator MaterialGradient(Material target, Gradient gradient, string propertyName) {
      Interpolator interpolator = GetInstance(InterpolatorType.MaterialGradient, 1);
      interpolator._targetMaterial = target;
      interpolator._materialId = Shader.PropertyToID(propertyName);
      interpolator._gradient = gradient;
      return interpolator;
    }

    public static Interpolator MaterialAlpha(Material target, float from, float to, string propertyName) {
      Interpolator interpolator = GetInstance(InterpolatorType.MaterialAlpha, to - from);
      interpolator._materialId = Shader.PropertyToID(propertyName);
      interpolator._targetMaterial = target;
      interpolator._floatA = from;
      interpolator._floatB = to - from;
      return interpolator;
    }

    public static Interpolator MaterialAlpha(Material target, float to, string propertyName) {
      return MaterialAlpha(target, target.GetColor(propertyName).a, to, propertyName);
    }

    public static Interpolator MaterialValue(Material target, float from, float to, string propertyName) {
      Interpolator interpolator = GetInstance(InterpolatorType.MaterialValue, to - from);
      interpolator._materialId = Shader.PropertyToID(propertyName);
      interpolator._targetMaterial = target;
      interpolator._floatA = from;
      interpolator._floatB = to - from;
      return interpolator;
    }

    public static Interpolator MaterialValue(Material target, float to, string propertyName) {
      return MaterialValue(target, target.GetFloat(propertyName), to, propertyName);
    }
    #endregion

    #region CanvasGroup Interpolators
    public static Interpolator CanvasGroupAlpha(CanvasGroup target, float from, float to) {
      Interpolator interpolator = GetInstance(InterpolatorType.CanvasGroupAlpha, to - from);
      interpolator._targetCanvasGroup = target;
      interpolator._floatA = from;
      interpolator._floatB = to - from;
      return interpolator;
    }

    public static Interpolator CanvasGroupAlpha(CanvasGroup target, float to) {
      return CanvasGroupAlpha(target, target.alpha, to);
    }
    #endregion

    public void Interpolate(float progress) {
      switch (_interpolatorType) {
        case InterpolatorType.ValueFloat:
          _actionFloat(_floatA + _floatB * progress);
          break;
        case InterpolatorType.ValueColor:
          _actionColor(_colorA + _colorB * progress);
          break;
        case InterpolatorType.ValueGradient:
          _actionColor(_gradient.Evaluate(progress));
          break;
        case InterpolatorType.ValueVec2:
          _actionVec2(_vec2A + _vec2B * progress);
          break;
        case InterpolatorType.ValueVec3:
          _actionVec3(_vec3A + _vec3B * progress);
          break;
        case InterpolatorType.ValueVec4:
          _actionVec4(_vec4A + _vec4B * progress);
          break;
        case InterpolatorType.ValueRot:
          _actionRot(Quaternion.Slerp(_rotA, _rotB, progress));
          break;

        case InterpolatorType.TransformGlobal:
          _targetTransform.position = _vec3A + _vec3B * progress;
          _targetTransform.rotation = Quaternion.Slerp(_rotA, _rotB, progress);
          break;
        case InterpolatorType.TransformGlobalPosition:
          _targetTransform.position = _vec3A + _vec3B * progress;
          break;
        case InterpolatorType.TransformGlobalRotation:
          _targetTransform.rotation = Quaternion.Slerp(_rotA, _rotB, progress);
          break;

        case InterpolatorType.TransformLocal:
          _targetTransform.localPosition = _vec3A + _vec3B * progress;
          _targetTransform.localRotation = Quaternion.Slerp(_rotA, _rotB, progress);
          break;
        case InterpolatorType.TransformLocalPosition:
          _targetTransform.localPosition = _vec3A + _vec3B * progress;
          break;
        case InterpolatorType.TransformLocalRotation:
          _targetTransform.localRotation = Quaternion.Slerp(_rotA, _rotB, progress);
          break;
        case InterpolatorType.TransformLocalScale:
          _targetTransform.localScale = _vec3A + _vec3B * progress;
          break;

        case InterpolatorType.MaterialColor:
          _targetMaterial.SetColor(_materialId, _colorA + _colorB * progress);
          break;
        case InterpolatorType.MaterialGradient:
          _targetMaterial.SetColor(_materialId, _gradient.Evaluate(progress));
          break;
        case InterpolatorType.MaterialAlpha:
          //Color curr = _targetMaterial.GetColor(_materialId);
          //_targetMaterial.SetColor(_materialId, curr.atAlpha(_floatA + _floatB * progress));
          break;
        case InterpolatorType.MaterialValue:
          _targetMaterial.SetFloat(_materialId, _floatA + _floatB * progress);
          break;

        case InterpolatorType.CanvasGroupAlpha:
          _targetCanvasGroup.alpha = _floatA + _floatB * progress;
          break;

        default:
          throw new Exception("Unexpected interpolator type " + _interpolatorType);
      }
    }

    public float GetLength() {
      return _length;
    }

    public bool IsValid() {
      switch (_interpolatorType) {
        case InterpolatorType.TransformGlobal:
        case InterpolatorType.TransformGlobalPosition:
        case InterpolatorType.TransformGlobalRotation:
        case InterpolatorType.TransformLocal:
        case InterpolatorType.TransformLocalPosition:
        case InterpolatorType.TransformLocalRotation:
        case InterpolatorType.TransformLocalScale:
          return _targetTransform != null;
        case InterpolatorType.MaterialAlpha:
        case InterpolatorType.MaterialColor:
        case InterpolatorType.MaterialValue:
          return _targetMaterial != null;
        case InterpolatorType.CanvasGroupAlpha:
          return _targetCanvasGroup != null;
        default:
          return true;
      }
    }
  }

}