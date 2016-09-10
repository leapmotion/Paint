using UnityEngine;

public enum NormalBasis {
  UPWARD,
  DOWNWARD,
  FORWARD,
  BACKWARD
}

public static class NormalBasisExtensions {
  public static Vector3 GetDirection(this NormalBasis basis) {
    switch (basis) {
      case NormalBasis.UPWARD:
        return Vector3.up;
      case NormalBasis.DOWNWARD:
        return Vector3.down;
      case NormalBasis.FORWARD:
        return Vector3.forward;
      case NormalBasis.BACKWARD:
        return Vector3.back;
      default: return Vector3.up;
    }
  }
}