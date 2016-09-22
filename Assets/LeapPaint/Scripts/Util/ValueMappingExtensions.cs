using UnityEngine;

public static class ValueMappingExtensions {

  /// <summary>
  /// Maps the value between valueMin and valueMax to its linearly proportional equivalent between resultMin and resultMax.
  /// </summary>
  public static float Map(this float value, float valueMin, float valueMax, float resultMin, float resultMax) {
    return Mathf.Lerp(resultMin, resultMax, ((value - valueMin) / (valueMax - valueMin)));
  }

}