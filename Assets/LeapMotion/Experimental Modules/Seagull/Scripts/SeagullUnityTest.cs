using Leap.Unity.Attributes;
using Leap.Unity.Meshing;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Leap.Unity.Seagull {

  public class SeagullUnityTest : MonoBehaviour {
    
    public int slotA = 0;
    public PolyMeshObject A;
    public int slotB = 1;
    public PolyMeshObject B;
    public int slotC = 2;
    public PolyMeshObject C;

    // Managed -> Native buffers
    private float[] _meshXYZPosSeqA;
    private float[] _meshXYZPosSeqB;
    private int[]   _triIndicesSeq;

    // Native -> Managed buffers
    private float[] _meshResultXYZPosSeqBuffer = new float[65536 * 3];
    private int _meshResultXYZPosSeqLength = -1;
    private int[]   _meshResultTriIdxSeqBuffer = new int[65536 * 3];
    private int _meshResultTriIdxSeqLength = -1;

    private void Start() {
      Dodecahedron.FillPolyMesh(A.polyMesh, Dodecahedron.PolygonMode.Triangles);
      Dodecahedron.FillPolyMesh(B.polyMesh, Dodecahedron.PolygonMode.Triangles);

      A.RefreshUnityMesh();
      B.RefreshUnityMesh();

      _meshXYZPosSeqA = new float[A.polyMesh.positions.Count * 3];
      _meshXYZPosSeqB = new float[B.polyMesh.positions.Count * 3];

      int trisIdx = 0;
      _triIndicesSeq = new int[A.polyMesh.polygons.Count * 3];
      for (int i = 0; i < A.polyMesh.polygons.Count; i++) {
        Polygon p = A.polyMesh.polygons[i];
        _triIndicesSeq[trisIdx++] = p[0];
        _triIndicesSeq[trisIdx++] = p[1];
        _triIndicesSeq[trisIdx++] = p[2];
      }

      Debug.Log("meshXYZPosSeq initialized. Length: " + _meshXYZPosSeqA.Length);
      Debug.Log("triIndicesSeq initialized. Length: " + _triIndicesSeq.Length);

      string posPtrString = "";
      string idxPtrString = "";
      unsafe {
        fixed (float* f32Arr_xyzPosSeq = _meshXYZPosSeqA) {
          fixed (int* i32Arr_triIdxSeq = _triIndicesSeq) {
            posPtrString = ((IntPtr)f32Arr_xyzPosSeq).ToString();
            idxPtrString = ((IntPtr)i32Arr_triIdxSeq).ToString();
          }
        }
      }

      Debug.Log("meshXYZPosSeq was able to be fixed. Pointer: " + posPtrString);
      Debug.Log("triIndicesSeq was able to be fixed. Pointer: " + idxPtrString);
    }

    private void Update() {
      if (Input.GetKeyDown(KeyCode.Alpha1)) {
        LoadMeshToSlotForA();
      }
      if (Input.GetKeyDown(KeyCode.Alpha2)) {
        LoadMeshToSlotForB();
      }
      if (Input.GetKeyDown(KeyCode.Alpha3)) {
        ComputeUnionToSlotC();
      }
      if (Input.GetKeyDown(KeyCode.Alpha4)) {
        ComputeDifferenceToSlotC();
      }
      if (Input.GetKeyDown(KeyCode.D)) {
        int count = C.polyMesh.positions.Count;
        int randIdx = UnityEngine.Random.Range(0, count);
        Vector3 position = C.polyMesh.positions[randIdx];
        Debug.Log("Here's a random position from C.polyMesh: " + position.ToString("R"));
      }
    }

    public void LoadMeshToSlotForA() {
      int seqIdx = 0;
      for (int i = 0; i < A.polyMesh.positions.Count; i++) {
        Vector3 pos = A.transform.TransformPoint(A.polyMesh.positions[i]);
        _meshXYZPosSeqA[seqIdx++] = pos.x;
        _meshXYZPosSeqA[seqIdx++] = pos.y;
        _meshXYZPosSeqA[seqIdx++] = pos.z;
      }

      unsafe {
        fixed (float* f32Arr_xyzPosSeq = _meshXYZPosSeqA) {
          fixed (int* i32Arr_triIdxSeq = _triIndicesSeq) {
            SeagullPlugin.LoadMesh(
              slotA,
              (IntPtr)f32Arr_xyzPosSeq, _meshXYZPosSeqA.Length,
              (IntPtr)i32Arr_triIdxSeq, _triIndicesSeq.Length);
          }
        }
      }

      Debug.Log("Mesh loaded to slot A");
    }

    public void LoadMeshToSlotForB() {
      int seqIdx = 0;
      for (int i = 0; i < B.polyMesh.positions.Count; i++) {
        Vector3 pos = B.transform.TransformPoint(B.polyMesh.positions[i]);
        _meshXYZPosSeqB[seqIdx++] = pos.x;
        _meshXYZPosSeqB[seqIdx++] = pos.y;
        _meshXYZPosSeqB[seqIdx++] = pos.z;
      }

      unsafe {
        fixed (float* f32Arr_xyzPosSeq = _meshXYZPosSeqB) {
          fixed (int* i32Arr_triIdxSeq = _triIndicesSeq) {
            SeagullPlugin.LoadMesh(
              slotB,
              (IntPtr)f32Arr_xyzPosSeq, _meshXYZPosSeqB.Length,
              (IntPtr)i32Arr_triIdxSeq, _triIndicesSeq.Length);
          }
        }
      }

      Debug.Log("Mesh loaded to slot B");
    }

    public void ComputeUnionToSlotC() {
      SeagullPlugin.Union(slotA, slotB, slotC);

      Debug.Log("Union A + B computed to slot C");

      LoadResultFromSlotC();
    }

    public void ComputeDifferenceToSlotC() {
      SeagullPlugin.Difference(slotA, slotB, slotC);

      Debug.Log("Difference A - B computed to slot C");

      LoadResultFromSlotC();
    }

    public void LoadResultFromSlotC() {
      _meshResultXYZPosSeqLength = SeagullPlugin.RetrieveMeshPositionSequenceLength(slotC);
      _meshResultTriIdxSeqLength = SeagullPlugin.RetrieveMeshTriIndicesLength(slotC);

      int resultCode = -1;
      unsafe {
        float* f32Arr_xyzPosSeq = stackalloc float[2048 * 3];
        int*   i32Arr_triIdxSeq = stackalloc int  [2048 * 3];

        resultCode = SeagullPlugin.RetrieveMeshData(
          slotC,
          (IntPtr)f32Arr_xyzPosSeq, _meshResultXYZPosSeqLength,
          (IntPtr)i32Arr_triIdxSeq, _meshResultTriIdxSeqLength);

        Marshal.Copy((IntPtr)f32Arr_xyzPosSeq,
                     _meshResultXYZPosSeqBuffer,
                     0, _meshResultXYZPosSeqLength);
        Marshal.Copy((IntPtr)i32Arr_triIdxSeq,
                     _meshResultTriIdxSeqBuffer,
                     0, _meshResultTriIdxSeqLength);
      }
      if (resultCode == -1) {
        Debug.Log("Couldn't get a result code from mesh retrieval attempt.");
      }
      if (resultCode != 0) {
        Debug.Log("Retrieval result error code: " + resultCode);
      }
      else {
        Debug.Log("No native-side errors detected from mesh retrieval.");
      }

      C.polyMesh.Clear();
      var positions = Pool<List<Vector3>>.Spawn(); positions.Clear();
      var triIndices = Pool<List<int>>.Spawn(); triIndices.Clear();
      try {
        for (int pc = 0; pc < _meshResultXYZPosSeqLength; pc += 3) {
          var v = new Vector3(_meshResultXYZPosSeqBuffer[pc + 0],
                              _meshResultXYZPosSeqBuffer[pc + 1],
                              _meshResultXYZPosSeqBuffer[pc + 2]);
          positions.Add(v);
        }
        for (int tv = 0; tv < _meshResultTriIdxSeqLength; tv++) {
          triIndices.Add(_meshResultTriIdxSeqBuffer[tv]);
        }

        C.polyMesh.AddPositions(positions);
        C.polyMesh.AddTriangles(triIndices);
      }
      finally {
        positions.Clear(); Pool<List<Vector3>>.Recycle(positions);
        triIndices.Clear();  Pool<List<int>>.Recycle(triIndices);
      }

      C.RefreshUnityMesh();

      Debug.Log("C polymesh loaded from Slot C.");
    }

  }

}