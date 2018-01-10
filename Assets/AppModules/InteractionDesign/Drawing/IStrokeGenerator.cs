using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Drawing {

  public interface IStrokeGenerator {

    void Initialize();

    void AddPoint(Vector3 position,
                  Vector3 normal,
                  Color color,
                  float size = 1f);

    void Finish();

  }

  public static class IStrokeGeneratorExtensions {

    public static void AddPoint(this IStrokeGenerator strokeGenerator,
                                Vector3 position,
                                Vector3 normal,
                                float size = 1f) {
      strokeGenerator.AddPoint(position, normal, Color.white, size);
    }

  }

}
