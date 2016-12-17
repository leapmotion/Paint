using Leap.Paint2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SegmentRenderer : MonoBehaviour {

  [Header("Segment Definitions")]
  [Tooltip("Must begin with a cyclical loop of the same length as the cross section mesh.")]
  public Mesh startCapMesh;
  [Tooltip("Must contain a cyclical loop of vertices, with no tris.")]
  public Mesh crossSectionMesh;
  [Tooltip("Must begin with a cyclical loop of the same length as the cross section mesh.")]
  public Mesh endCapMesh;

  private List<Segment> _segments;
  private Mesh _mesh;
  private MeshFilter _meshFilter;
  private MeshRenderer _meshRenderer;
  private bool _needsInit = true;

  void Start() {
    if (_needsInit) Initialize();
  }

  /// <summary>
  /// Initializes or clears the segments if already initialized.
  /// </summary>
  public void Initialize() {
    if (_segments == null) _segments = new List<Segment>();
    else _segments.Clear();
    if (_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
    if (_mesh == null) _mesh = _meshFilter.mesh = new Mesh();
    else _mesh.Clear();
    if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();
    _needsInit = false;
  }

  // TODO: This may not be the best way to represent segments.
  /// <summary>
  /// Segments consist of two connected cross-sections aligned to outward-facing normals.
  /// These segments may or may not have startCap and endCap meshes attached.
  /// </summary>
  public class Segment {
    public StrokePoint startPoint;
    public StrokePoint endPoint;
    public Segment prevSegment;
    public Segment nextSegment;
  }

}
