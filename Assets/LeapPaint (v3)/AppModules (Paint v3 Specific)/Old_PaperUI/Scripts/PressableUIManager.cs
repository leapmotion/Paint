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

    private List<PressableUI> _pressablesBuffer = new List<PressableUI>();

    public class ActivatorProximity {
      public UIActivator activator;
      public float distance = float.PositiveInfinity;
      public void Clear() { activator = null; distance = float.PositiveInfinity; }
    }
    public Dictionary<PressableUI, ActivatorProximity> _proximityData
      = new Dictionary<PressableUI, ActivatorProximity>();

    protected void FixedUpdate() {

      // Clear proximity data for the update.
      _proximityData.Clear();

      try {
        for (int i = 0; i < _activators.Length; i++) {
          var activator = _activators[i];
          if (!activator.IsHandTracked) continue;

          var activatorHandedness = activator._handModel.Handedness;

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
          if (activatorHandedness == Chirality.Left) {
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
          if (activatorHandedness == Chirality.Right) {
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

          // We know the closest pressable to this activator. Remember it, but there might
          // be a closer activator we'll find later, so we don't notify the pressable yet.
          ActivatorProximity proximityData = null;
          if (_proximityData.TryGetValue(absoluteClosestPressable, out proximityData)) {
            // Is this activator closer than the previously-found activator?
            if (absoluteClosestPressableDistance < proximityData.distance) {
              proximityData.activator = activator;
              proximityData.distance = absoluteClosestPressableDistance;
            }
            // Otherwise, the other activator is closer, so no action is necessary.
          }
          else {
            // Create the first proximity entry for this activator->pressable pair.
            // We pool proximity data objects to avoid allocation.
            var newProximityData = Pool<ActivatorProximity>.Spawn();
            newProximityData.activator = activator;
            newProximityData.distance = absoluteClosestPressableDistance;
            _proximityData[absoluteClosestPressable] = newProximityData;
          }
        }

        // Now, scan through each entry in the proximity data and notify each pressable
        // element about the closest activator to it.
        foreach (var pressableProximityDataPair in _proximityData) {
          var pressable = pressableProximityDataPair.Key;
          var proximityData = pressableProximityDataPair.Value;

          // The important bit: Notify each pressable of its closest UIActivator.
          pressable.NotifyClosestActivator(proximityData.activator);
        }
      }
      finally {
        // Finally, return the proximity data objects we spawned back to the pool.
        foreach (var pressableProximityDataPair in _proximityData) {
          var proximityData = pressableProximityDataPair.Value;
          proximityData.Clear();
          Pool<ActivatorProximity>.Recycle(proximityData);
        }

        // Technically repetitive, but just-to-be-safe we also don't want references to
        // pooled objects hanging around in the buffer dictionary.
        _proximityData.Clear();
      }

    }

    #endregion

  }


}