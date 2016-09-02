using UnityEngine;
using System.Collections.Generic;

public class LauncherBackground : MonoBehaviour {

  [SerializeField]
  private CurvedSpace _space;

  [SerializeField]
  private PoissonDisc _generator;

  [SerializeField]
  private AnimationCurve _heightBias;

  [SerializeField]
  private float _radiusMultiplier;

  [SerializeField]
  private Material _material;

  [ContextMenu("Try It")]
  private void generateQuads() {
    foreach (var t in GetComponentsInChildren<Transform>()) {
      if (t != transform) {
        DestroyImmediate(t.gameObject);
      }
    }

    List<PoissonDisc.Disc> discs = _generator.Generate();

    for (int i = 0; i < discs.Count; i++) {
      var disc = discs[i];

      Vector2 rectPos = disc.position - _generator.area * 0.5f;

      rectPos.y *= _heightBias.Evaluate(Mathf.Abs(rectPos.y));

      GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
      obj.transform.parent = transform;
      obj.transform.position = _space.RectToWorld(rectPos, _radiusMultiplier * disc.radius);
      obj.transform.rotation = Quaternion.LookRotation(obj.transform.position);
      obj.transform.localScale = disc.radius * Vector3.one * 1.5f;

      Material mat = new Material(_material);
      mat.color = Color.white * Random.value * 0.03f;
      obj.GetComponent<Renderer>().sharedMaterial = mat;
    }
  }

  private void generateTexture() {

  }


}
