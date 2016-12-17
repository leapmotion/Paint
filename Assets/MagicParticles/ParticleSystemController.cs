using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemController : MonoBehaviour {

  [Header("Automatically Configured on Start")]
  [SerializeField]
  private ParticleSystem[] _particleSystems;

  void Start() {
    _particleSystems = GetComponentsInChildren<ParticleSystem>();
  }

  public void EnsureEmittingEnabled() {
    for (int i = 0; i < _particleSystems.Length; i++) {
      ParticleSystem ps = _particleSystems[i];
      if (!ps.isEmitting) {
        ps.Play();
      }
    }
  }

  public void EnsureEmittingDisabled() {
    for (int i = 0; i < _particleSystems.Length; i++) {
      ParticleSystem ps = _particleSystems[i];
      if (ps.isEmitting) {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
      }
    }
  }

}
