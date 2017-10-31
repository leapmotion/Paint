using UnityEngine;
using System.Collections;

namespace Leap.Unity.LeapPaint_v3 {

  public class SimpleRotator : MonoBehaviour {

    public Vector3 _localRotationAxis = Vector3.right;

    public float _period = 3600F;

    protected virtual void OnValidate() {
      _localRotationAxis = _localRotationAxis.normalized;
    }

    protected virtual void Update() {
      this.transform.Rotate(_localRotationAxis, (360F / _period) * Time.deltaTime, UnityEngine.Space.Self);
    }

  }


}
