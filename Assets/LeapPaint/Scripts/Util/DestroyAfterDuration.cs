using UnityEngine;
using System.Collections;

public class DestroyAfterDuration : MonoBehaviour {

  public float _timer = 5F;

  void Start() {
    StartCoroutine(DestroyWhenTimerReachesZero());
  }

  public void SetTimer(float newTimer) {
    _timer = newTimer;
  }

  private IEnumerator DestroyWhenTimerReachesZero() {
    while (true) {
      yield return new WaitForSecondsRealtime(1F);
      _timer -= 1F;
      if (_timer <= 0F) break;
    }
    Destroy(this.gameObject);
  }

}
