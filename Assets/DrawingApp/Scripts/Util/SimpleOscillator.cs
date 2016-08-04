using UnityEngine;
using System.Collections;

public class SimpleOscillator : MonoBehaviour {

  public Transform anchor;
  public Vector3 localAxisOfOscillation = Vector3.up;
  public bool oscillateInWorldSpace = false;
  public Vector3 worldAxisOfOscillation = Vector3.up;
  public float amplitude = 1F;
  public float period = 1F;

  protected void OnValidate() {
    localAxisOfOscillation = localAxisOfOscillation.normalized;
    worldAxisOfOscillation = worldAxisOfOscillation.normalized;
  }

  protected void Update() {
    this.transform.position = anchor.position
      + (oscillateInWorldSpace ? worldAxisOfOscillation : this.transform.TransformDirection(localAxisOfOscillation)) * Mathf.Sin(Time.time * 2F * Mathf.PI / period) * amplitude;
  }

}
