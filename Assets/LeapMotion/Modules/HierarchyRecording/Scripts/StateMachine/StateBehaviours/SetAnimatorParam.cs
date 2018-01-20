using UnityEngine;

public class SetAnimatorParam : MonoBehaviour {

  [SerializeField]
  private Animator _animator;

  [SerializeField]
  private bool _revertOnDisable = true;

  [SerializeField]
  private AnimatorControllerParameterType _type = AnimatorControllerParameterType.Float;

  [SerializeField]
  private string _paramName;

  [SerializeField]
  private bool _boolValue;

  [SerializeField]
  private int _intValue;

  [SerializeField]
  private float _floatValue;

  private object _defaultValue;

  private void Awake() {
    switch (_type) {
      case AnimatorControllerParameterType.Bool:
        _defaultValue = _animator.GetBool(_paramName);
        break;
      case AnimatorControllerParameterType.Int:
        _defaultValue = _animator.GetInteger(_paramName);
        break;
      case AnimatorControllerParameterType.Float:
        _defaultValue = _animator.GetFloat(_paramName);
        break;
    }
  }

  private void OnEnable() {
    switch (_type) {
      case AnimatorControllerParameterType.Bool:
        _animator.SetBool(_paramName, _boolValue);
        break;
      case AnimatorControllerParameterType.Int:
        _animator.SetInteger(_paramName, _intValue);
        break;
      case AnimatorControllerParameterType.Float:
        _animator.SetFloat(_paramName, _floatValue);
        break;
      case AnimatorControllerParameterType.Trigger:
        _animator.SetTrigger(_paramName);
        break;
    }
  }

  private void OnDisable() {
    if (_revertOnDisable) {
      switch (_type) {
        case AnimatorControllerParameterType.Bool:
          _animator.SetBool(_paramName, (bool)_defaultValue);
          break;
        case AnimatorControllerParameterType.Int:
          _animator.SetInteger(_paramName, (int)_defaultValue);
          break;
        case AnimatorControllerParameterType.Float:
          _animator.SetFloat(_paramName, (float)_defaultValue);
          break;
        case AnimatorControllerParameterType.Trigger:
          _animator.ResetTrigger(_paramName);
          break;
      }
    }
  }
}
