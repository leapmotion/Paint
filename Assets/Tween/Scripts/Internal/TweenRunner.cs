using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TweenInternal {

  public class TweenRunner : MonoBehaviour {
    private TweenInstance[] _runningTweens = new TweenInstance[16];
    private int _runningCount = 0;

    private static TweenRunner _instance = null;
    public static TweenRunner Instance {
      get {
        if (_instance == null) {
          _instance = FindObjectOfType<TweenRunner>();
          if (_instance == null) {
            _instance = new GameObject("Tween Runner").AddComponent<TweenRunner>();
            _instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
          }
        }
        return _instance;
      }
    }

    void Update() {
      for (int i = _runningCount - 1; i >= 0; --i) {
        var instance = _runningTweens[i];
        try {
          if (instance.StepProgress()) {
            RemoveTween(instance);
          }
        } catch (Exception e) {
          Debug.LogError("Error occured inside of tween!  Tween has been terminated");
          Debug.LogException(e);
          if (_runningTweens[i].IsRunning) {
            RemoveTween(instance);
          }
        }
      }
    }

    public void AddTween(TweenInstance instance) {
      if (_runningTweens.Length <= _runningCount) {
        TweenInstance[] newArray = new TweenInstance[_runningTweens.Length * 2];
        _runningTweens.CopyTo(newArray, 0);
        _runningTweens = newArray;
      }

      instance.SetRunnerIndex(_runningCount);
      _runningTweens[_runningCount++] = instance;
    }

    public void RemoveTween(TweenInstance instance) {
      if (instance.GetRunnerIndex() == -1) {
        return;
      }

      --_runningCount;
      if (_runningCount < 0) {
        throw new Exception("Removed more tweens than were started!");
      }

      int index = instance.GetRunnerIndex();

      _runningTweens[_runningCount].SetRunnerIndex(index);
      _runningTweens[index] = _runningTweens[_runningCount];

      instance.SetRunnerIndex(-1);
    }
  }

}