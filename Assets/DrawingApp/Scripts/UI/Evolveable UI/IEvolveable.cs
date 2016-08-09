using UnityEngine;

/// <summary>
/// Evolveable objects can visually transform into one another using a general
/// animation scheme implemented by UIEvolver (or other custom code).
/// </summary>
public interface IEvolveable {

  /// <summary>
  /// Returns a Transform describing the anchor point that the Evolveable element follows when not evolving.
  /// (Not just self.transform!)
  /// </summary>
  /// <returns></returns>
  Transform GetAnchor();

  /// <summary>
  /// Immediately after this method is called, the implement class should be located at
  /// (from). The implementing class should then use (translationCurve) to reach (to),
  /// (duration) seconds after the method is called.
  /// </summary>
  void MoveFromTo(Transform from, Transform to, AnimationCurve translationCurve, float duration);

  /// <summary>
  /// After (duration) seconds from the time this method is called, the implementing class
  /// should have faded entirely to the transition color.
  /// </summary>
  void FadeToTransitionColor(Color transitionColor, AnimationCurve curve, float duration);

  /// <summary>
  /// After (duration) seconds from the time this method is called, the implementing class
  /// should have faded from the transitionColor back to its original color(s).
  /// </summary>
  void FadeFromTransitionColor(Color transitionColor, AnimationCurve curve, float duration);

  /// <summary>
  /// Immediately after this method is called, the implementing class should assume
  /// (fromScale) as a fraction of its full size. After (duration) seconds from the
  /// time this method is called, the implementing class should reach (toScale) as
  /// a fraction of its full size.
  /// </summary>
  void ChangeToScale(float fromScale, float toScale, AnimationCurve curve, float duration);

  /// <summary>
  /// Immediately after this method is called, the implementing class should be visible
  /// if it was previously invisible. If (asWhite), the object should appear all white.
  /// (It will later be told to fade in from white.)
  /// </summary>
  void Appear();

  /// <summary>
  /// Immediately after this method is called, the implementing class should be invisible
  /// if it was previously visible.
  /// </summary>
  void Disappear();

}

public static class IEvolveableExtensions {

  private static AnimationCurve s_dummyCurve = AnimationCurve.Linear(0F, 0F, 1F, 1F);

  public static void MoveTo(this IEvolveable evolveable, Transform transform) {
    evolveable.MoveFromTo(transform, transform, s_dummyCurve, 0F);
  }

  public static void SetToTransitionColor(this IEvolveable evolveable, Color transitionColor) {
    evolveable.FadeToTransitionColor(transitionColor, s_dummyCurve, 0F);
  }

  public static void SetScale(this IEvolveable evolveable, float scale) {
    evolveable.ChangeToScale(scale, scale, s_dummyCurve, 0F);
  }

  public static void SetToOriginalColor(this IEvolveable evolveable) {
    evolveable.FadeFromTransitionColor(Color.white, s_dummyCurve, 0F);
  }

}
