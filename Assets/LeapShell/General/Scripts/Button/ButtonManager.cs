using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;
using Leap;
using Leap.Unity;
using Leap.Unity.Attributes;

public class ButtonManager : MonoBehaviour {

  [AutoFind]
  [SerializeField]
  private LeapProvider _provider;

  private List<ButtonBase> _buttons = new List<ButtonBase>();

  private Dictionary<int, ButtonAction> _actions = new Dictionary<int, ButtonAction>();

  public void AddButton(ButtonBase button) {
    _buttons.Add(button);
  }

  public void AddButtons(IEnumerable<ButtonBase> buttons) {
    _buttons.AddRange(buttons);
  }

  public void RemoveButton(ButtonBase button) {
    _buttons.Remove(button);

    var pairs = _actions.Where(p => p.Value.button == button).ToList();
    foreach (var pair in pairs) {
      pair.Value.button.ChangeState(ButtonBase.State.None);
      _actions.Remove(pair.Key);
    }
  }

  public void RemoveButtons(IEnumerable<ButtonBase> button) {
    foreach (var toRemove in button) {
      RemoveButton(toRemove);
    }
  }

  [System.Serializable]
  public class ButtonEvent : UnityEvent<ButtonBase> { }
  public ButtonEvent onSelect;

  void Update() {
    List<Hand> hands = _provider.CurrentFrame.Hands;

    List<int> activeIds = new List<int>();
    for (int i = 0; i < hands.Count; i++) {
      Hand hand = hands[i];
      activeIds.Add(hand.Id);

      ButtonAction action;
      if (!_actions.TryGetValue(hand.Id, out action)) {
        if (tryStartAction(hand, out action)) {
          _actions[hand.Id] = action;
        }
      }

      if (action != null) {
        bool isFinished;
        updateAction(hand, action, out isFinished);

        if (isFinished) {
          endAction(action);
          _actions.Remove(hand.Id);
        }
      }
    }

    List<int> staleIds = new List<int>();
    foreach (var pair in _actions) {
      if (!activeIds.Contains(pair.Key)) {
        staleIds.Add(pair.Key);
        endAction(pair.Value);
      }
    }
  }

  private ButtonBase getClosestButton(Hand hand) {
    ButtonBase closestButton = _buttons[0];
    float minDist = closestButton.GetHandDistance(hand);
    for (int j = 1; j < _buttons.Count; j++) {
      ButtonBase button = _buttons[j];
      float dist = button.GetHandDistance(hand);
      if (dist < minDist) {
        closestButton = button;
        minDist = dist;
      }
    }
    return closestButton;
  }

  private bool tryStartAction(Hand hand, out ButtonAction action) {
    action = null;

    ButtonBase closestButton = null;
    float closestDist = float.MaxValue;
    for (int i = 0; i < _buttons.Count; i++) {
      //Don't consider buttons that already have actions
      if (_actions.Values.Any(a => a.button == _buttons[i])) {
        continue;
      }

      float dist = _buttons[i].GetHandDistance(hand);
      if (dist < closestDist) {
        closestButton = _buttons[i];
        closestDist = dist;
      }
    }

    if (closestButton == null) {
      return false;
    }

    if (closestDist > closestButton.MaxHoverRange) {
      return false;
    }

    float depth = closestButton.GetHandDepth(hand);
    if (depth > closestButton.HoverDepth || depth < closestButton.PressDepth) {
      return false;
    }

    action = new ButtonAction(closestButton, ButtonBase.State.Hover);
    return true;
  }

  private void updateAction(Hand hand, ButtonAction action, out bool isFinished) {
    isFinished = false;

    if (action.button.CurrentState == ButtonBase.State.Hover) {
      ButtonBase closestButton = null;
      float closestDist = float.MaxValue;
      for (int i = 0; i < _buttons.Count; i++) {
        //Dont consider buttons held by any other action
        if (_actions.Values.Where(a => a != action).Any(a => a.button == _buttons[i])) {
          continue;
        }

        float dist = _buttons[i].GetHandDistance(hand);
        if (dist < closestDist) {
          closestButton = _buttons[i];
          closestDist = dist;
        }
      }

      if (closestButton == null) {
        isFinished = true;
        return;
      }

      if (closestDist > closestButton.MaxHoverRange) {
        isFinished = true;
        return;
      }

      //Change hover target
      if (closestButton != action.button) {
        action.SetButton(closestButton);
      }
    } else {
      float distToButton = action.button.GetHandDistance(hand);

      if (distToButton > action.button.MaxPressRange) {
        isFinished = true;
        return;
      }
    }

    float zDepth = action.button.GetHandDepth(hand);

    if (action.button.CurrentState == ButtonBase.State.Hover) {
      if (zDepth < action.button.PressDepth) {
        action.button.ChangeState(ButtonBase.State.Pressing);
      }
    }

    if (action.button.CurrentState == ButtonBase.State.Pressing) {
      if (zDepth > action.button.PressDepth) {
        action.button.ChangeState(ButtonBase.State.Hover);
      }
      if (zDepth <= action.button.SelectDepth) {
        action.button.ChangeState(ButtonBase.State.Selected);
        onSelect.Invoke(action.button);
      }
    }

    if (action.button.CurrentState == ButtonBase.State.Selected) {
      if (zDepth > action.button.PressDepth) {
        action.button.ChangeState(ButtonBase.State.Hover);
      }
    }

    if (zDepth > action.button.HoverDepth) {
      isFinished = true;
    }

    action.button.SetDepth(Mathf.Min(action.button.HoverDepth, zDepth));
  }

  private void endAction(ButtonAction action) {
    action.SetButton(null);
  }

  public class ButtonAction {
    private ButtonBase _button;
    public ButtonBase button {
      get {
        return _button;
      }
    }

    public ButtonAction(ButtonBase button, ButtonBase.State state) {
      _button = button;
      _button.ChangeState(state);
    }

    public void SetButton(ButtonBase button) {
      if (_button != null) {
        _button.ChangeState(ButtonBase.State.None);
      }

      if (button != null) {
        button.ChangeState(ButtonBase.State.Hover);
      }

      _button = button;
    }
  }
}
