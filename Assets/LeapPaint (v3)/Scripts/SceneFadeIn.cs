using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Leap.Unity.Animation;

public class SceneFadeIn : MonoBehaviour {

  public PostProcessVolume volume;
  public float fadeTime;

  private void Start() {
    Tween.Single().Value(0, 1, w => volume.weight = w).
                   OverTime(fadeTime).
                   Play();
  }
}
