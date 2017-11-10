using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Geometry {

  [System.Serializable]
  public struct Point {
    public Transform transform;

    [SerializeField]
    private Vector3 _position;
    public Vector3 position {
      get {
        if (transform == null) return _position;
        else return transform.TransformPoint(_position);
      }
      set {
        if (transform == null) _position = value;
        else _position = transform.InverseTransformPoint(value);
      }
    }
    /// <summary> Short-hand for position. </summary>
    public Vector3 pos { get { return position; } set { position = value; } }

    public Point(Component transformSource = null)
      : this(default(Vector3), transformSource) { }

    public Point(Vector3 position = default(Vector3), Component transformSource = null) {
      this.transform = transformSource.transform;
      _position = Vector3.zero;
    }

  }

  public static class PointExtensions {



  }

}
