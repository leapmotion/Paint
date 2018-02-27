using System;
using UnityEngine;

namespace Leap.Unity.LemurUI {

  /// <summary>
  /// Static API for LemurUI. Use this to, e.g., spawn new UI elements at runtime.
  /// </summary>
  public static class Lemur {
    
    public static Label Spawn<T>() where T : Label {
      return new Label<TextMesh>();
    }

    // need to use reflection and cache results probably
    //private static class Default<Label> {
    //  public static Type textRendererType = typeof(TextMesh);
    //  public static Type driverType = typeof(TextMeshLabelDriver);
    //}

  }

}
