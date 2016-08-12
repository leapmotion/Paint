using UnityEngine;
using System.Collections;

public class SimpleFaceCamera : MonoBehaviour {

  public bool _invertZ = false;

  [Header("Optional")]
  public Transform _faceFromPoint;

  private bool _activelyFaceCamera = false;

  public void SetFaceFromPoint(Transform faceFrom) {
    _faceFromPoint = faceFrom;
  }

  public void ActivateFacing() {
    _activelyFaceCamera = true;
  }

  public void DeactivateFacing() {
    _activelyFaceCamera = false;

    Rigidbody body = GetComponent<Rigidbody>();
    if (body != null) {
      body.angularVelocity = Vector3.zero;
    }
  }

  protected void Update() {
    if (_activelyFaceCamera) {
      FaceCameraInstantly();
    }
  }

  public void FaceCameraInstantly() {
    Transform faceFrom;
    if (_faceFromPoint != null) {
      faceFrom = _faceFromPoint;
    }
    else {
      faceFrom = this.transform;
    }

    Quaternion desiredRotation = Quaternion.LookRotation(Camera.main.transform.position - faceFrom.position);

    this.transform.rotation = desiredRotation;
    if (_invertZ) {
      this.transform.rotation = Quaternion.AngleAxis(180F, this.transform.up) * this.transform.rotation;
    }
  }

  public void FaceCameraForSeconds(float seconds) {
    StartCoroutine(DoFaceCameraForSeconds(seconds));
  }

  private IEnumerator DoFaceCameraForSeconds(float seconds) {
    float time = 0;
    while (time < seconds) {
      FaceCameraInstantly();

      yield return new WaitForFixedUpdate();
      time += Time.fixedDeltaTime;
    }

    Rigidbody body = GetComponent<Rigidbody>();
    if (body != null) {
      body.angularVelocity = Vector3.zero;
    }
  }

}
