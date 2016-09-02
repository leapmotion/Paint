using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Leap.Unity.Attributes;

public class AppCollection : MonoBehaviour, IEnumerable<AppButton> {
  private List<AppButton> _buttons = new List<AppButton>();

  [AutoFind]
  [SerializeField]
  private ButtonManager _buttonManager;

  [AutoFind]
  [SerializeField]
  private GraspManager _graspManager;

  public void Add(AppButton button) {
    _buttons.Add(button);
    if (isActiveAndEnabled) {
      _buttonManager.AddButton(button);
      _graspManager.AddButton(button);
    }
  }

  public void Remove(AppButton button) {
    _buttons.Remove(button);
    if (isActiveAndEnabled) {
      _buttonManager.RemoveButton(button);
      _graspManager.RemoveButton(button);
    }
  }

  void OnEnable() {
    _buttonManager.AddButtons(_buttons.Cast<ButtonBase>());
    _graspManager.AddButtons(_buttons);
  }

  void OnDisable() {
    _buttonManager.RemoveButtons(_buttons.Cast<ButtonBase>());
    _graspManager.RemoveButtons(_buttons);
  }

  public int Count {
    get {
      return _buttons.Count;
    }
  }

  public AppButton this[int index] {
    get {
      return _buttons[index];
    }
  }

  public IEnumerator<AppButton> GetEnumerator() {
    return _buttons.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() {
    return _buttons.GetEnumerator();
  }
}
