using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Visualization {

  public class VisualizationBehaviour : MonoBehaviour {

    protected virtual void Update() {
      this.transform.LookAwayFrom(Camera.main.transform);
    }

    public VisualizationBehaviour AtPosition(Vector3 worldPosition) {
      this.transform.position = worldPosition;
      return this;
    }

    private int _maxSamples = 32;
    public int maxSamples { get { return _maxSamples; } }
    public VisualizationBehaviour MaxSamples(int maxSamples) {
      _maxSamples = maxSamples;
      return this;
    }

    private bool _rangeSpecified = false;
    public bool rangeSpecified { get { return _rangeSpecified; } }
    private float _minValue = 0F;
    private float _maxValue = 0F;
    public Vector2 range { get { return new Vector2(_minValue, _maxValue); } }
    public float minValue { get { return _minValue; } }
    public float maxValue { get { return _maxValue; } }
    public VisualizationBehaviour Range(Vector2 range) {
      return Range(range.x, range.y);
    }
    public VisualizationBehaviour Range(float minValue, float maxValue) {
      _rangeSpecified = true;
      _minValue = minValue;
      _maxValue = maxValue;
      return this;
    }

    private UnityEngine.Color _color = UnityEngine.Color.gray;
    public UnityEngine.Color color { get { return _color; } }
    public VisualizationBehaviour Color(UnityEngine.Color color) {
      _color = color;
      return this;
    }

  }

}