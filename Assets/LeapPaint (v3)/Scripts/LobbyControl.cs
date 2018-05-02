using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Leap.Unity.Animation;
using Leap.Unity.LeapPaint_v3;

public class LobbyControl : MonoBehaviour {

  private const string HAS_EXPERIENCED_TUTORIAL_KEY = "LeapPaintUserHasExperiencedTutorial";
  private const int FALSE_VALUE = 0;
  private const int TRUE_VALUE = 1;

  public static LobbySelectionState selectionState = LobbySelectionState.None;

  public static bool hasExperiencedTutorial {
    get {
      return PlayerPrefs.GetInt(HAS_EXPERIENCED_TUTORIAL_KEY, defaultValue: FALSE_VALUE) == TRUE_VALUE;
    }
    set {
      int prefValue = value ? TRUE_VALUE : FALSE_VALUE;
      PlayerPrefs.SetInt(HAS_EXPERIENCED_TUTORIAL_KEY, prefValue);
    }
  }

  public string sceneToLoad;
  public PressableUI tutorialButton;
  public PressableUI sandboxButton;
  public float transitionTime;
  public AnimationCurve transitionCurve;

  private Tween _buttonTween;

  void OnEnable() {
    if (!hasExperiencedTutorial) {
      transitionWithoutButtons();
      return;
    }

    _buttonTween = Tween.Persistent().
                   Target(tutorialButton.transform).LocalScale(Vector3.zero, tutorialButton.transform.localScale).
                   Target(sandboxButton.transform).LocalScale(Vector3.zero, sandboxButton.transform.localScale).
                   Smooth(transitionCurve).
                   OverTime(transitionTime).
                   OnReachEnd(() => {
                     tutorialButton.enabled = true;
                     sandboxButton.enabled = true;
                   });

    _buttonTween.Play();
  }

  private void OnDisable() {
    _buttonTween.Release();
  }

  public void OnSelectTutorial() {
    shouldPlayTutorial = true;
    StartCoroutine(transitionMinimizeButtons());
  }

  public void OnSelectSandbox() {
    shouldPlayTutorial = false;
    StartCoroutine(transitionMinimizeButtons());
  }

  private IEnumerator transitionMinimizeButtons() {
    var asyncOp = SceneManager.LoadSceneAsync(sceneToLoad);
    asyncOp.allowSceneActivation = false;

    tutorialButton.enabled = false;
    sandboxButton.enabled = false;

    yield return _buttonTween.Play(Direction.Backward).Yield();
    yield return new WaitUntil(() => asyncOp.isDone);

    asyncOp.allowSceneActivation = true;
  }

  private void transitionWithoutButtons() {
    var asyncOp = SceneManager.LoadSceneAsync(sceneToLoad);
    asyncOp.allowSceneActivation = true;
  }

  public enum LobbySelectionState {
    None,
    Tutorial,
    Sandbox
  }
}
