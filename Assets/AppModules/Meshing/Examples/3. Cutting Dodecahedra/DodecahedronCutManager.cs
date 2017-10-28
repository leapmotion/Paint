using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing.Examples {

  public class DodecahedronCutManager : MonoBehaviour {

    public DodecahedronExample d0;
    public DodecahedronExample d1;

    void Awake() {
      d0.cutManager = this;
      d1.cutManager = this;
    }

    void Update() {
      d0.InitMesh();
      d1.InitMesh();

      PolyMesh.Ops.Cut(d0.polyMesh, d1.polyMesh);

      d0.UpdateMesh();
      d1.UpdateMesh();
    }

  }

}
