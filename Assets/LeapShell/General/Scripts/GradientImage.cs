using UnityEngine;
using UnityEngine.UI;

public class GradientImage : MonoBehaviour {

  [SerializeField]
  private float _length = 0.2f;

  [SerializeField]
  private float _power = 1;

  private Renderer _renderer;

  void Awake() {
    _renderer = GetComponent<Renderer>();
  }

  public void SetGradient(bool isLeft, float position, float fade) {
    Vector4 gradient = new Vector4();
    gradient.x = Mathf.Lerp(0, 1, isLeft ? position : 1 - position);
    gradient.y = isLeft ? -_length : _length;
    gradient.z = _power;
    gradient.w = fade;

    _renderer.material.SetVector("_Gradient", gradient);
  }

}
