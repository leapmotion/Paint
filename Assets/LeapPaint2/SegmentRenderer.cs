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

  private List<SegmentNode> _segmentNodes;
  private Mesh _mesh;
  private MeshFilter _meshFilter;
  private MeshRenderer _meshRenderer;
  private bool _needsInit = true;

  void Start() {
    if (_needsInit) Initialize();
  }

  /// <summary>
  /// Initializes, or resets if already initialized.
  /// </summary>
  public void Initialize() {
    if (_segmentNodes == null) _segmentNodes = new List<SegmentNode>();
    else _segmentNodes.Clear();
    if (_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
    if (_mesh == null) _mesh = _meshFilter.mesh = new Mesh();
    else _mesh.Clear();
    if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();
    _needsInit = false;
  }

  public void AddPoint(StrokePoint strokePoint) {
    SegmentNode node = new SegmentNode(strokePoint);
    _segmentNodes.Add(node);
    if (_segmentNodes.Count > 1) {
      node.prevNode = _segmentNodes[_segmentNodes.Count - 2];
      node.prevNode.nextNode = node;
      AddMeshSegment(node.prevNode, node);
    }
  }

  public void AddStartCap(int strokePointIndex) {
    SegmentNode node = _segmentNodes[strokePointIndex];
    node.hasStartCap = true;
    AddMeshStartCap(node);
  }

  public void AddEndCap(int strokePointIndex) {
    SegmentNode node = _segmentNodes[strokePointIndex];
    node.hasEndCap = true;
    AddMeshEndCap(node);
  }

  public void RemoveEndCapAtEnd() {
    SegmentNode node = _segmentNodes[_segmentNodes.Count - 1];
    node.hasEndCap = false;
    RemoveMeshEndCap(node);
  }

  private class SegmentNode {
    public StrokePoint strokePoint;
    public SegmentNode(StrokePoint point) {
      strokePoint = point;
    }
    public bool hasStartCap = false;
    public bool hasEndCap = false;

    public SegmentNode prevNode;
    public SegmentNode nextNode;
  }

  private void AddMeshSegment(SegmentNode node1, SegmentNode node2) {
    throw new System.NotImplementedException();
  }

  private void AddMeshStartCap(SegmentNode node) {
    throw new System.NotImplementedException();
  }

  private void AddMeshEndCap(SegmentNode node) {
    throw new System.NotImplementedException();
  }

  private void RemoveMeshEndCap(SegmentNode node) {
    throw new System.NotImplementedException();
  }

}
