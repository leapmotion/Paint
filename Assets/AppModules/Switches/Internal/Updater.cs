using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Animation {

  public class Updater : MonoBehaviour {

    private static Updater _singleton = null;
    public static Updater singleton {
      get {
        if (_singleton == null) {
          GameObject updaterObj = new GameObject("__Updater Singleton__");
          _singleton = updaterObj.AddComponent<Updater>();
        }
        return _singleton;
      }
    }

    public event Action OnUpdate;

    void Update() {
      OnUpdate();
    }

  }

}