using Leap.Unity.Attributes;
using System;
using UnityEngine;

namespace Leap.Unity.MeshGen {

  public class TorusGenerator : MeshGenerator {

    [MinValue(0)]
    public float majorRadius = 1F;

    [MinValue(3)]
    public int numMajorSegments = 16;

    [MinValue(0)]
    public float minorRadius = 0.25F;

    [MinValue(3)]
    public int numMinorSegments = 16;

    public override void Generate(Mesh mesh) {
      Generators.GenerateTorus(mesh,
                               majorRadius, numMajorSegments,
                               minorRadius, numMinorSegments);
    }

  }

}