using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Meshing.Examples {

  public class DodecahedronCutManager : MonoBehaviour {

    public DodecahedronExample dA;
    public DodecahedronExample dB;

    void Awake() {
      dA.cutManager = this;
      dB.cutManager = this;
    }

    void Update() {
      dA.InitMesh();
      dB.InitMesh();

      PolyMesh.Ops.DualCut(dA.polyMesh, dB.polyMesh);

      dA.UpdateMesh();
      dB.UpdateMesh();
    }

  }

}
