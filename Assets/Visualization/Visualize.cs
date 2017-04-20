using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Visualization {

  public static class Visualize {

    public static Dictionary<string, FloatVisualization> s_floatViz = new Dictionary<string, FloatVisualization>();

    public static FloatVisualization Float(string visualizationName, float sample) {
      FloatVisualization floatViz;
      if (!s_floatViz.ContainsKey(visualizationName)) {
        s_floatViz[visualizationName] = CreateNewFloatVisualization(visualizationName);
      }
      floatViz = s_floatViz[visualizationName];
      floatViz.Add(sample);
      return floatViz;
    }

    private static FloatVisualization CreateNewFloatVisualization(string visualizationName) {
      GameObject obj = new GameObject(visualizationName);
      obj.name = visualizationName + " Visualization";
      FloatVisualization visualization = obj.AddComponent<FloatVisualization>();
      return visualization;
    }

  }

}