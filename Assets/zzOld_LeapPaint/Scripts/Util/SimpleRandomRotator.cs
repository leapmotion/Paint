using UnityEngine;
using System.Collections;

public class SimpleRandomRotator : MonoBehaviour {

  public Vector3 _localRotationAxis = Vector3.right;

  public float _minPeriod = 60F;
  public float _maxPeriod = 60F*15F;

  private float _chosenPeriod = 0F;

  protected virtual void Start() {
    _chosenPeriod = Mathf.Lerp(_minPeriod, _maxPeriod, Random.value);
  }

  protected virtual void Update() {
    this.transform.Rotate(_localRotationAxis, (360F / _chosenPeriod) * Time.deltaTime, Space.Self);
  }

}
