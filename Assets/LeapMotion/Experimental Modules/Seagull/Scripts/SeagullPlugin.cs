using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Leap.Unity.Seagull {

  /// <summary>
  /// Interop layer for the Seagull DLL.
  /// </summary>
  public static class SeagullPlugin {

    [RuntimeInitializeOnLoadMethod]
    public static void RuntimeInitializeOnLoad() {
      Initialize();
    }

    /// <summary>
    /// Initializes the Seagull DLL. Expected to be called once at initialization time.
    /// </summary>s
    [DllImport("Seagull")]
    public static extern void Initialize();

    #region Mesh Loading

    /// <summary>
    /// Loads the mesh data into a representation Seagull can use to perform e.g. set
    /// operations. Mesh data is loaded into a slot identified by the integer index. This
    /// index must be less than GetNumMeshSlots(). If there is already a mesh at the slot,
    /// it will be overwritten.
    /// 
    /// To simply load a new mesh without overwriting anything and receive a fresh slot
    /// index, use LoadNewMesh.
    /// </summary>
    [DllImport("Seagull")]
    public static extern void LoadMesh(
      Int32 slot,
      IntPtr f32Arr_xyzSeqPositions, Int32 xyzSeqArrLength,
      IntPtr i32Arr_triIndices,      Int32 triIndicesLength);

    /// <summary>
    /// Loads the mesh data into a representation Seagull can use to perform e.g. set
    /// operations. Mesh data is loaded into a slot identified by the integer index. This
    /// index must be less than GetNumMeshSlots(). If there is already a mesh at the slot,
    /// it will be overwritten.
    /// 
    /// This function will fail and return -1 if there are no slots left. 
    /// </summary>
    [DllImport("Seagull")]
    public static extern Int32 LoadNewMesh(
      IntPtr f32Arr_xyzSeqPositions, Int32 xyzSeqArrLength,
      IntPtr i32Arr_triIndices,      Int32 triIndicesLength);

    /// <summary>
    /// Retrieves the mesh at the argument slot and copies its data as vertex positions
    /// and triangle position indices via memcpy to the corresponding argument pointers.
    /// You must specify the correct XYZ sequence array length and the correct triangle
    /// indices array length as well. 
    /// 
    /// If you don't know what these values are for the mesh at the slot index, see the
    /// RetrieveMeshPositionSequenceLength and RetrieveMeshTriIndicesLength functions.
    /// </summary>
    [DllImport("Seagull")]
    public static extern Int32 RetrieveMeshData(
      Int32 slot,
      IntPtr f32Arr_outXYZSeqPositions, Int32 xyzSeqArrLength,
      IntPtr i32Arr_outTriIndices,      Int32 triIndicesLength);

    ///<summary>
    /// Retrieves the number of positions in the mesh stored at the slot * 3, i.e., the
    /// length of a sequence of numbers describing the X, Y, and Z coordinates of each
    /// position.
    /// </summary>
    [DllImport("Seagull")]
    public static extern Int32 RetrieveMeshPositionSequenceLength(Int32 slot);

    /// <summary>
    /// Retrieves the number of triangle indices in the mesh stored at the argument slot.
    /// This corresponds to the number of triangle faces in the mesh * 3; each element is
    /// a vertex index.
    /// </summary>
    [DllImport("Seagull")]
    public static extern Int32 RetrieveMeshTriIndicesLength(Int32 slot);

    #endregion

    #region Mesh Operations

    /// <summary>
    /// Computes the union of the mesh at slot A and the mesh at slot B.
    /// Outputs the result into the mesh at outSlot.
    /// </summary>
    [DllImport("Seagull")]
    public static extern void Union(Int32 aSlot, Int32 bSlot, Int32 outSlot);

    /// <summary>
    /// Computes the difference of the mesh at slot A and the mesh at slot B.
    /// Outputs the result in the mesh at outSlot.
    /// </summary>
    [DllImport("Seagull")]
    public static extern void Difference(Int32 aSlot, Int32 bSlot, Int32 outSlot);

    #endregion

  }

}