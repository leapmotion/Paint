using UnityEngine;
using System.Collections;
namespace Leap.Unity.LeapPaint_v3 {

  public class EnableGameObjectsOnStartup : MonoBehaviour {

    public GameObject[] _gameObjectsToEnable;

    void Start() {
      for (int i = 0; i < _gameObjectsToEnable.Length; i++) {
        if (_gameObjectsToEnable[i] != null) {
          _gameObjectsToEnable[i].SetActive(true);
        }
      }
    }

  }


}
