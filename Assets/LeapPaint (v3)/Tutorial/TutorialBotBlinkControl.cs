using System.Collections;
using UnityEngine;
using Leap.Unity.Animation;
using Leap.Unity.Attributes;

public class TutorialBotBlinkControl : MonoBehaviour {

  public GameObject eyesOpenAnchor;
  public GameObject eyesClosedAnchor;

  [Header("Settings")]
  [MinValue(0)]
  public float eyesClosedTime;

  [MinValue(0)]
  public float maxBlinkDelay;

  [UnitCurve]
  public AnimationCurve blinkDistribution;

  [Range(0, 1)]
  public float doubleBlinkChance;

  [MinValue(0)]
  public float maxDoubleBlinkTime;

  [UnitCurve]
  public AnimationCurve doubleBlinkDistribution;

  private IEnumerator Start() {
    float delay, endTime;
    while (true) {

      delay = blinkDistribution.Evaluate(Random.value) * maxBlinkDelay;
      endTime = Time.time + delay;
      while (Time.time < endTime) {
        yield return null;
      }

      blink();

      if (Random.value < doubleBlinkChance) {
        delay = doubleBlinkDistribution.Evaluate(Random.value) * maxDoubleBlinkTime;
        endTime = Time.time + delay;
        while (Time.time < endTime) {
          yield return null;
        }

        blink();
      }
    }
  }

  private void blink() {
    eyesOpenAnchor.SetActive(false);
    eyesClosedAnchor.SetActive(true);

    Tween.AfterDelay(eyesClosedTime, () => {
      eyesOpenAnchor.SetActive(true);
      eyesClosedAnchor.SetActive(false);
    });
  }
}
