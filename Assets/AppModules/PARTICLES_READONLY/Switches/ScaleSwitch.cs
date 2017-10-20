using Leap.Unity.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Animation {

  public class ScaleSwitch : TweenSwitch, IPropertySwitch {

    public const float NEAR_ZERO = 0.0001f;

    #region Inspector

    [Header("Scale Control")]

    /// <summary> The target to animate. </summary>
    public Transform localScaleTarget;

    /// <summary>
    /// The desired local scale of the scale target when the switch is on.
    /// </summary>
    public Vector3 onLocalScale = Vector3.one;

    /// <summary>
    /// The desired local scale of the scale target when the switch is off.
    /// </summary>
    public Vector3 offLocalScale = Vector3.zero;

    /// <summary>
    /// Enforces a minimum value of 0.0001f for each localScale axis.
    /// </summary>
    public bool enforceNonzeroScale = true;

    /// <summary>
    /// Deactivates THIS OBJECT when its target localScale is zero or very near zero.
    /// </summary>
    public bool deactivateSelfWhenZero = true;

    [Header("Animation Curves")]
    [UnitCurve]
    [DisableIf("nonUniformScale", isEqualTo: true)]
    public AnimationCurve scaleCurve = DefaultCurve.SigmoidUp;

    public bool nonUniformScale = false;

    [UnitCurve]
    [DisableIf("nonUniformScale", isEqualTo: false)]
    public AnimationCurve xScaleCurve = DefaultCurve.SigmoidUp;
    [UnitCurve]
    [DisableIf("nonUniformScale", isEqualTo: false)]
    public AnimationCurve yScaleCurve = DefaultCurve.SigmoidUp;
    [UnitCurve]
    [DisableIf("nonUniformScale", isEqualTo: false)]
    public AnimationCurve zScaleCurve = DefaultCurve.SigmoidUp;

    #endregion

    #region Unity Events

    protected virtual void Reset() {
      if (localScaleTarget == null) localScaleTarget = this.transform;
    }

    #endregion

    #region Switch Implementation

    protected override void updateSwitch(float time, bool immediately = false) {
#if UNITY_EDITOR
      if (!Application.isPlaying) {
        if (localScaleTarget != null) {
          UnityEditor.Undo.RecordObject(localScaleTarget.transform, "Update Scale Switch");
        }
      }
#endif

      Vector3 targetScale = getTargetScale(time);

      if (enforceNonzeroScale) {
        targetScale = Vector3.Max(targetScale, Vector3.one * NEAR_ZERO);
      }

      if (localScaleTarget != null) {
        localScaleTarget.localScale = targetScale;
      }

      if (deactivateSelfWhenZero) {
        this.gameObject.SetActive(!(targetScale.CompMin() <= NEAR_ZERO));
      }
    }

    private Vector3 getTargetScale(float time) {
      if (!nonUniformScale) {
        return Vector3.Lerp(offLocalScale, onLocalScale, scaleCurve.Evaluate(time));
      }
      else {
        return new Vector3(Mathf.Lerp(offLocalScale.x, onLocalScale.x, xScaleCurve.Evaluate(time)),
                           Mathf.Lerp(offLocalScale.y, onLocalScale.y, yScaleCurve.Evaluate(time)),
                           Mathf.Lerp(offLocalScale.z, onLocalScale.z, zScaleCurve.Evaluate(time)));
      }
    }

    #endregion

  }

}