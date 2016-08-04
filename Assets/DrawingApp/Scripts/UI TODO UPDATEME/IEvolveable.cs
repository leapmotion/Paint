using UnityEngine;

/// <summary>
/// Evolveable objects can visually transform into one another using a general
/// animation scheme.
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
  /// should be entirely white.
  /// </summary>
  void FadeToWhite(float duration);

  /// <summary>
  /// Immediately after this method is called, the implementing class should assume
  /// (fromScale) as a fraction of its full size. After (duration) seconds from the
  /// time this method is called, the implementing class should reach (toScale) as
  /// a fraction of its full size.
  /// </summary>
  void ChangeToSize(float fromScale, float toScale, float duration);

  /// <summary>
  /// After (duration) seconds from the time this method is called, the implementing class
  /// should display its original colors and texture.
  /// </summary>
  /// <param name="duration"></param>
  void FadeFromWhite(float duration);

  /// <summary>
  /// Immediately after this method is called, the implementing class should be visible
  /// if it was previously invisible. If (asWhite), the object should appear all white.
  /// (It will later be told to fade in from white.)
  /// </summary>
  void Appear(bool asWhite);

  /// <summary>
  /// Immediately after this method is called, the implementing class should be invisible
  /// if it was previously visible.
  /// </summary>
  void Disappear();

}
