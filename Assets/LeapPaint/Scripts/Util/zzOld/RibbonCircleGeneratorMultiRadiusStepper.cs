using UnityEngine;
using System.Collections;

public class RibbonCircleGeneratorMultiRadiusStepper : MonoBehaviour {

  public RibbonCircleGenerator[] _ribbonCircleGenerators;

  public float _initRadius = 15F;
  public float _radiusStep = 0.1F;

  protected void OnValidate() {
    _ribbonCircleGenerators = GetComponentsInChildren<RibbonCircleGenerator>();
    for (int i = 0; i < _ribbonCircleGenerators.Length; i++) {
      _ribbonCircleGenerators[i]._initRadius = _initRadius + (i * _radiusStep);
      _ribbonCircleGenerators[i].SendMessage("OnValidate");
    }
  }

}
