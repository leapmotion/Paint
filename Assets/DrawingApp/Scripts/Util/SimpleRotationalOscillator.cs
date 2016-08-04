using UnityEngine;
using System.Collections;

public class SimpleRotationalOscillator : MonoBehaviour {

  public Transform anchor;
  public Vector3 localAxisOfOscillation = Vector3.up;
  public bool oscillateInWorldSpace = false;
  public Vector3 worldAxisOfOscillation = Vector3.up;
  public float amplitudeInDegrees = 1F;
  public float period = 1F;

  protected void OnValidate() {
    localAxisOfOscillation = localAxisOfOscillation.normalized;
    worldAxisOfOscillation = worldAxisOfOscillation.normalized;
  }

  protected void Update() {
    this.transform.rotation = Quaternion.AngleAxis(Mathf.Sin(Time.time * 2F * Mathf.PI / period) * amplitudeInDegrees,
      (oscillateInWorldSpace ? worldAxisOfOscillation : this.transform.TransformDirection(localAxisOfOscillation))) * anchor.rotation;
  }

}
