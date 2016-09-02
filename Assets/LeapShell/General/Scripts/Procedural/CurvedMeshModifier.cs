using UnityEngine;
using System;
using Procedural.DynamicMesh;

public class CurvedMeshModifier : ModifierBehaviour<CurvedMeshMod> { }

[Serializable]
public struct CurvedMeshMod : IMeshMod {
  public CurvedSpace space;

  public void Modify(ref RawMesh input) {
    if (space == null) return;

    for (int i = input.verts.Count; i-- != 0;) {
      Vector3 pos = input.verts[i];
      input.verts[i] = space.RectToLocal(pos, pos.z);
    }

    input.normals = null;
  }
}
