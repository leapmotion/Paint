using UnityEngine;
using System.Collections;

using zzOld_MeshGeneration_LeapPaint_v3;

public class GenerationExample : MonoBehaviour {

  [SerializeField]
  private Material _mat;

  [SerializeField]
  private int _ribbonCount = 500;

  [SerializeField]
  private int _segments = 500;

  IEnumerator Start() {
    ShapeCombiner c = new ShapeCombiner(65536, shouldOptimize: true, shouldUpload: true, infiniteBounds: false);
    MeshObjectCreator.CreateObjectsRealtime(c, _mat, transform);

    Tube tube = new Tube(8);
    for (int i = 0; i < _ribbonCount; i++) {
      tube.Clear();
      
      float offset = Random.Range(0, 360);
      float radius = Random.Range(0.5f, 1.5f);

      for (int j = 0; j < _segments; j++) {
        tube.Add(new MeshPoint(new Vector3(j * 0.04f, 
                                             radius * Mathf.Sin(j * 0.1f + offset), 
                                             radius * Mathf.Cos(j * 0.1f + offset))),
                                 0.03f);
      }

      yield return null;
      c.AddShape(tube);
    }

    c.FinalizeCurrentMesh();
  }
}
