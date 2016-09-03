using UnityEngine;
using System.Collections;

public class EnableGameObjectsOnStartup : MonoBehaviour {

  public GameObject[] _gameObjectsToEnable;

  void Start() {
    for (int i = 0; i < _gameObjectsToEnable.Length; i++) {
      _gameObjectsToEnable[i].SetActive(true);
    }
  }

}
