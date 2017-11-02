using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing {
  
  // Pair:
  //   a = List of PolyMesh position indices corresponding to the keyed object's
  //       positions in the PolyMesh
  //   b = List of PolyMesh Polygon indices corresponding to the keyed object's
  //       polygons in the PolyMesh
  using PolygonData = Pair<List<int>, List<int>>;

  public class LivePolyMeshObject : MonoBehaviour {

    #region Inspector

    [SerializeField]
    private MeshFilter _meshFilter;
    public MeshFilter meshFilter {
      get {
        if (_meshFilter == null) {
          _meshFilter = this.gameObject.GetComponent<MeshFilter>();
          if (_meshFilter == null) {
            _meshFilter = this.gameObject.AddComponent<MeshFilter>();
          }
        }
        return _meshFilter;
      }
    }

    [SerializeField]
    private MeshRenderer _meshRenderer;
    public MeshRenderer meshRenderer {
      get {
        if (_meshRenderer == null) {
          _meshRenderer = this.gameObject.GetComponent<MeshRenderer>();
          if (_meshRenderer == null) {
            _meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
          }
        }

        return _meshRenderer;
      }
    }

    #endregion

    private PolyMesh _polyMesh;

    // Two meshes that are swapped when a given mesh is updated, better when modifying
    // a mesh every frame.
    private Mesh _unityMeshA;
    private Mesh _unityMeshB;

    void Awake() {
      _meshFilter = GetComponent<MeshFilter>();

      _polyMesh = Pool<PolyMesh>.Spawn();
      _polyMesh.DisableEdgeData();
      _polyMesh.Clear();

      _unityMeshA = Pool<Mesh>.Spawn();
      _unityMeshA.Clear();
      _unityMeshA.MarkDynamic();
      _unityMeshB = Pool<Mesh>.Spawn();
      _unityMeshB.Clear();
      _unityMeshB.MarkDynamic();

      _notifyPolyMeshModifiedAction = notifyPolyMeshModified;
    }

    void OnDestroy() {
      // In clearing the PolyMesh, its data is pooled. Polygon data in objectPolygonData
      // would also have been pooled and thus will no longer be valid.
      _polyMesh.Clear();
      Pool<PolyMesh>.Recycle(_polyMesh);

      _unityMeshA.Clear();
      Pool<Mesh>.Recycle(_unityMeshA);

      _unityMeshB.Clear();
      Pool<Mesh>.Recycle(_unityMeshB);

      // Pool the polygon data lists we allocated for objectPolygonData.
      foreach (var objPolyDataPair in objectPolygonData) {
        var polygonData = objPolyDataPair.Value;

        var positionIndices = polygonData.a;
        var polygonIndices = polygonData.b;

        positionIndices.Clear();
        Pool<List<int>>.Recycle(positionIndices);

        // The individual polygon data would have been invalidated and returned to the
        // pool when the PolyMesh was cleared; so just recycle the polygons.
        polygonIndices.Clear();
        Pool<List<int>>.Recycle(polygonIndices);
      }
    }

    public int PolygonCount {
      get { return _polyMesh.polygons.Count; }
    }

    private bool bufferState = false;
    private void RefreshUnityMesh() {
      using (new ProfilerSample("LivePolyMeshObject: Refresh Unity Mesh")) {
        var bufferMesh = bufferState ? _unityMeshA : _unityMeshB;
        bufferState = !bufferState;

        _polyMesh.FillUnityMesh(bufferMesh);
        meshFilter.sharedMesh = bufferMesh;
      }
    }

    private Dictionary<object, PolygonData> objectPolygonData
      = new Dictionary<object, PolygonData>();

    /// <summary>
    /// Adds new positions and new polygons to the underlying PolyMesh of this object,
    /// keying their indices using the provided key.
    /// 
    /// The Polygons in newPolygons should index the newPositions directly; they are
    /// both added using the PolyMesh Append() operation, which will modify the polygons
    /// in the newPolygons list (specifically, their underlying vertex indices lists will
    /// be modified to match the indices of the positions that are added to the PolyMesh).
    /// 
    /// If you'd like to retrieve this data to modify it later, provide the same key to
    /// the ModifyDataFor() method, which will return a PolyMesh object and indices into
    /// its data that you can modify freely.
    /// 
    /// Positions are never re-used by LivePolyMeshObjects, so they are less optimal than
    /// manipulating a PolyMesh directly (however, this has no impact on the resulting
    /// Unity mesh representation).
    /// </summary>
    public void AddDataFor(object key, List<Vector3> newPositions,
                                       List<Polygon> newPolygons) {
      PolygonData polygonData;
      if (objectPolygonData.TryGetValue(key, out polygonData)) {
        throw new System.InvalidOperationException(
          "Data for the provided key already exists: " + key.ToString());
      }

      var newPositionIndices = Pool<List<int>>.Spawn();
      newPositionIndices.Clear();
      var newPolygonIndices = Pool<List<int>>.Spawn();
      newPolygonIndices.Clear();
      _polyMesh.Append(newPositions,
                       newPolygons,
                       newPositionIndices,
                       newPolygonIndices);
      
      objectPolygonData[key] = new PolygonData() {
        a = newPositionIndices,
        b = newPolygonIndices
      };

      RefreshUnityMesh();
    }

    private bool _modificationPending = false;
    /// <summary>
    /// Provide the object whose mesh representation you'd like to modify. You will
    /// receive a PolyMesh object, position indices into that PolyMesh, and polygon
    /// indices into that PolyMesh; these indices are the polygon mesh data associated
    /// with the keyed object.
    /// 
    /// Valid modifications currently only _ADD_ position or polygons to the PolyMesh.
    /// You must at least re-use all of the existing positions and polygons for the
    /// keyed object (you can modify the values at the existing indices), but you can
    /// also add new positions and new polygons as long as you report them in using the
    /// callback Action.
    /// 
    /// When you are finished modifying the PolyMesh, call the provided Action, which
    /// will update the Unity mesh representation of the PolyMesh, and allow future
    /// modifications. You must provide the indices of any additional positions or
    /// polygons you added to the PolyMesh. (PolyMesh addition methods include variants
    /// that pass back the added indices of any new positions and polygons.)
    /// </summary>
    public void ModifyDataFor(object key,
                              out PolyMesh polyMesh,
                              List<int> keyedPositionIndices,
                              List<int> keyedPolygonIndices,
                              out Action<object, List<int>, List<int>> 
                                callWhenDoneModifyingPolyMesh) {

      if (_modificationPending) {
        throw new InvalidOperationException(
          "A PolyMesh modification is already in progress for this LivePolyMeshObject. "
          + "(Did you forget to call the Action when the modification was finished?)");
      }

      callWhenDoneModifyingPolyMesh = _notifyPolyMeshModifiedAction;
      polyMesh = _polyMesh;

      PolygonData polyData;
      if (!objectPolygonData.TryGetValue(key, out polyData)) {
        throw new InvalidOperationException(
          "No polygon data was found for key: " + key.ToString() + "; "
          + "Did you add data for this key first?");
      }

      keyedPositionIndices.AddRange(polyData.a);
      keyedPolygonIndices.AddRange(polyData.b);
    }

    /// <summary>
    /// This Action is passed to objects that request to be able to directly modify a
    /// the PolyMesh of a LivePolyMeshObject. They call it when they are done modifying
    /// the PolyMesh of this PolyMeshObject; they must remember and provide the indices
    /// of any positions or polygons that they added to the PolyMesh.
    /// 
    /// You can provide null for the newPositionIndices or newPolygonIndices if no new
    /// positions or polygons were added.
    /// </summary>
    private Action<object, List<int>, List<int>> _notifyPolyMeshModifiedAction;
    private void notifyPolyMeshModified(object key,
                                        List<int> newPositionIndices,
                                        List<int> newPolygonIndices) {
      _modificationPending = false;

      PolygonData polyData;
      if (!objectPolygonData.TryGetValue(key, out polyData)) {
        throw new InvalidOperationException(
          "No polygon data was found for key: " + key.ToString());
      }
      if (newPositionIndices != null) {
        polyData.a.AddRange(newPositionIndices);
      }
      if (newPolygonIndices != null) {
        polyData.b.AddRange(newPolygonIndices);
      }

      RefreshUnityMesh();
    }

  }
  
}
