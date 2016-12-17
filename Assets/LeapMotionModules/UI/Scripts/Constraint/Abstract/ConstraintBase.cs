using Leap.Unity.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.UI.Constraint {

  public enum Axis { X, Y, Z }

  public static class ConstraintExtensions {
    public static Vector3 ToUnitVector3(this Axis axis) {
      switch (axis) {
        case Axis.X:
          return Vector3.right;
        case Axis.Y:
          return Vector3.up;
        case Axis.Z: default:
          return Vector3.forward;
      }
    }
  }

  [ExecuteInEditMode]
  /// <summary>
  /// Base class for Constraints, which enforce boundaries on the local-space positions of Transforms.
  /// </summary>
  public abstract class ConstraintBase : MonoBehaviour {

    public static Color DefaultGizmoColor       { get { return new Color(0.7F, 0.2F, 0.5F,   1F); } }
    public static Color DefaultGizmoSubtleColor { get { return new Color(0.7F, 0.2F, 0.5F, 0.3F); } }

    [Header("Constraint Basis")]
    [Disable] // (Read-only)
    public Vector3 constraintLocalPosition = Vector3.zero;

    [Header("Editor Constraint Debugging")]
    [SerializeField]
    public bool debugConstraint;

    protected bool _currentlyDebugEnforcingConstraint;

    private void InitLocalPosition() {
      constraintLocalPosition = this.transform.localPosition;
    }

    protected virtual void OnEnable() {
      InitLocalPosition();
    }

    protected virtual void Start() {
      InitLocalPosition();
    }

    protected virtual void Update() {
      if (Application.isPlaying) {
        EnforceConstraint();
      }

#if UNITY_EDITOR
      if (!Application.isPlaying) {
        if (debugConstraint && !_currentlyDebugEnforcingConstraint) {
          InitLocalPosition();
          InitializeDebuggingConstraint();
          _currentlyDebugEnforcingConstraint = true;
        }

        if (_currentlyDebugEnforcingConstraint) {
          EnforceConstraint();
        }
        else {
          InitLocalPosition();
        }

        if (!debugConstraint && _currentlyDebugEnforcingConstraint) {
          this.transform.localPosition = constraintLocalPosition;
          _currentlyDebugEnforcingConstraint = false;
        }
      }
#endif
    }

    /// <summary>
    /// Editor-only: Called once right when the user checks "Debug Constraint".
    /// Desired behavior: Set the constrained Transform to match the current
    /// debug constraint properties, such as linear progress along the a linear constraint.
    /// </summary>
    public abstract void InitializeDebuggingConstraint();

    /// <summary>
    /// After calling this method, the Transform's local space should satisfy the limits
    /// defined by this constraint.
    /// </summary>
    public abstract void EnforceConstraint();

  }

  public class ConstraintDebuggingAttribute : System.Attribute { }

}