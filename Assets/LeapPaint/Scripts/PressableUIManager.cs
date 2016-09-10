using UnityEngine;
using System.Collections.Generic;

public class PressableUIManager : MonoBehaviour {

  #region Setup

  public UIActivator[] _activators;

  private List<PressableUI> _pressables = new List<PressableUI>();
  private PressableUI[] _cachedClosestPressables; // equal in length to num _activators

  protected void Start() {
    _cachedClosestPressables = new PressableUI[_activators.Length];
  }

  public void RegisterPressable(PressableUI pressable) {
    _pressables.Add(pressable);
  }

  #endregion

  #region Unity Callbacks

  protected void FixedUpdate() {
    Dictionary<PressableUI, float> pressableToDistance = new Dictionary<PressableUI, float>();
    Dictionary<PressableUI, UIActivator> pressableToActivator = new Dictionary<PressableUI, UIActivator>();
    for (int i = 0; i < _activators.Length; i++) {
      if (!_activators[i].IsHandTracked) continue;

      float pressableDistance;
      PressableUI closestPressable = _activators[i].gameObject.FindClosest<PressableUI>(_pressables, out pressableDistance);
      _cachedClosestPressables[i] = closestPressable;

      if (pressableToDistance.ContainsKey(closestPressable)) {
        if (pressableDistance < pressableToDistance[closestPressable]) {
          pressableToDistance[closestPressable] = pressableDistance;
          pressableToActivator[closestPressable] = _activators[i];
        }
      }
      else {
        pressableToDistance[closestPressable] = pressableDistance;
        pressableToActivator[closestPressable] = _activators[i];
      }
    }

    for (int i = 0; i < _cachedClosestPressables.Length; i++) {
      PressableUI pressable = _cachedClosestPressables[i];
      if (pressable != null && pressableToActivator.ContainsKey(pressable) && _cachedClosestPressables[i].Activator != pressableToActivator[pressable]) {
        pressable.NotifyClosestActivator(pressableToActivator[pressable]);
      }
    }
  }

  #endregion

}
