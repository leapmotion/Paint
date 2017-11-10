using Leap.Unity.Geometry;
using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Leap.Unity.Launcher {

  public class PanelCommandController : MonoBehaviour, IRuntimeGizmoComponent {

    public Sphere visibilitySphere;
    public Point  proximityPoint;

    private void Reset() {
      visibilitySphere = new Sphere(0.10f, this);
      proximityPoint = new Point(this);
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      var color = Color.Lerp(LeapColor.cerulean, Color.cyan, 0.7f);
      var subtleColor = color.WithAlpha(0.4f);

      proximityPoint.RenderSphere(color, 3f);
      proximityPoint.Render(color, 3f);

      visibilitySphere.Render(subtleColor);
    }
  }

}
