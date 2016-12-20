using UnityEngine;
using System.Collections;

namespace MeshGeneration {

  public struct MeshPoint {
    private Vector3 _position;
    private Vector3 _normal;
    private bool _hasNormal;
    private Vector2 _uv;
    private Color _color;

    public MeshPoint(Vector3 position) {
      _position = position;
      _normal = Vector3.zero;
      _hasNormal = false;
      _uv = Vector3.zero;
      _color = Color.white;
    }

    public Vector3 Position {
      get {
        return _position;
      }
      set {
        _position = value;
      }
    }

    public bool HasNormal {
      get {
        return _hasNormal;
      }
    }

    public Vector3 Normal {
      get {
        return _normal;
      }
      set {
        _normal = value;
        _hasNormal = true;
      }
    }

    public Vector3 Uv {
      get {
        return _uv;
      }
      set {
        _uv = value;
      }
    }

    public Color Color {
      get {
        return _color;
      }
      set {
        _color = value;
      }
    }
  }

}