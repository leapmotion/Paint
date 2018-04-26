using UnityEngine;
using System.Collections.Generic;

namespace Leap.Unity.LeapPaint_v3 {

  using ActivatorFilter = PressableUI.ActivatorFilter;

  public class PressableUIManager : MonoBehaviour {

    #region Setup

    public UIActivator[] _activators;
    
    private Dictionary<ActivatorFilter, List<PressableUI>> _pressablesByFilter
      = new Dictionary<ActivatorFilter, List<PressableUI>>();

    public void RegisterPressable(PressableUI pressable, ActivatorFilter filter) {
      ensureKeyExists(_pressablesByFilter, filter, new List<PressableUI>());
      _pressablesByFilter[filter].Add(pressable);
    }

    private static void ensureKeyExists<K, V>(Dictionary<K, V> dict, K key, V defaultValue) {
      V value;
      if (!dict.TryGetValue(key, out value)) {
        dict[key] = defaultValue;
      }
    }

    #endregion

    #region Unity Callbacks

    public class PressableUIData {

    }

    //Dictionary<PressableUI, float> _pressableToDistance
    //  = new Dictionary<PressableUI, float>();
    //Dictionary<PressableUI, UIActivator> _pressableToActivator
    //    = new Dictionary<PressableUI, UIActivator>();

    private List<PressableUI> _pressablesBuffer = new List<PressableUI>();

    protected void FixedUpdate() {

      for (int i = 0; i < _activators.Length; i++) {
        var activator = _activators[i];
        if (!activator.IsHandTracked) continue;

        var activatorKind = activator._handModel.Handedness;

        _pressablesBuffer.Clear();

        // Get closest pressable that filters for either hand.
        ensureKeyExists(_pressablesByFilter, ActivatorFilter.EitherHand,
                        new List<PressableUI>());
        PressableUI closestPressable_eitherHand = null;
        float closestPressableDistance_eitherHand = float.PositiveInfinity;
        closestPressable_eitherHand = activator.gameObject.FindClosest<PressableUI>(
          _pressablesByFilter[ActivatorFilter.EitherHand],
          out closestPressableDistance_eitherHand);

        if (closestPressable_eitherHand != null) {
          _pressablesBuffer.Add(closestPressable_eitherHand);
        }

        // Get closest pressable that filters accepts left-hand only input if applicable.
        ensureKeyExists(_pressablesByFilter, ActivatorFilter.LeftHandOnly,
                        new List<PressableUI>());
        PressableUI closestPressable_leftOnly = null;
        float closestPressableDistance_leftOnly = float.PositiveInfinity;
        if (activatorKind == Chirality.Left) {
          closestPressable_leftOnly = activator.gameObject.FindClosest(
            _pressablesByFilter[ActivatorFilter.LeftHandOnly],
            out closestPressableDistance_leftOnly);

          if (closestPressable_leftOnly != null) {
            _pressablesBuffer.Add(closestPressable_leftOnly);
          }
        }

        // Get closest pressable that filters accepts right-hand only input if applicable.
        ensureKeyExists(_pressablesByFilter, ActivatorFilter.RightHandOnly,
                        new List<PressableUI>());
        PressableUI closestPressable_rightOnly = null;
        float closestPressableDistance_rightOnly = float.PositiveInfinity;
        if (activatorKind == Chirality.Right) {
          closestPressable_rightOnly = activator.gameObject.FindClosest(
            _pressablesByFilter[ActivatorFilter.RightHandOnly],
            out closestPressableDistance_rightOnly);

          if (closestPressable_rightOnly != null) {
            _pressablesBuffer.Add(closestPressable_rightOnly);
          }
        }

        // Get absolute closest pressable to this activator.
        float absoluteClosestPressableDistance = float.PositiveInfinity;
        var absoluteClosestPressable = activator.gameObject.FindClosest(
          _pressablesBuffer,
          out absoluteClosestPressableDistance);

        // Notify the closest pressable that its closest activator is this one.
        if (absoluteClosestPressable != null) {
          absoluteClosestPressable.NotifyClosestActivator(activator);
        }
      }

      //for (int i = 0; i < _activators.Length; i++) {
      //  if (!_activators[i].IsHandTracked) continue;

      //  float pressableDistance;
      //  PressableUI closestPressable = _activators[i].gameObject.FindClosest<PressableUI>(_pressables, out pressableDistance);
      //  if (closestPressable == null) {
      //    continue;
      //  }

      //  _cachedClosestPressables[i] = closestPressable;

      //  if (pressableToDistance.ContainsKey(closestPressable)) {
      //    if (pressableDistance < pressableToDistance[closestPressable]) {
      //      pressableToDistance[closestPressable] = pressableDistance;
      //      pressableToActivator[closestPressable] = _activators[i];
      //    }
      //  }
      //  else {
      //    pressableToDistance[closestPressable] = pressableDistance;
      //    pressableToActivator[closestPressable] = _activators[i];
      //  }
      //}

      //for (int i = 0; i < _cachedClosestPressables.Length; i++) {
      //  PressableUI pressable = _cachedClosestPressables[i];
      //  if (pressable != null
      //    && pressableToActivator.ContainsKey(pressable)
      //    && _cachedClosestPressables[i].activator != pressableToActivator[pressable]) {
      //    pressable.NotifyClosestActivator(pressableToActivator[pressable]);
      //  }
      //}
    }

    #endregion

  }


}