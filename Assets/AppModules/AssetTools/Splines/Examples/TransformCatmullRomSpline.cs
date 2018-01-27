using Leap.Unity.Animation;
using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Splines {

  public class TransformCatmullRomSpline : MonoBehaviour, IRuntimeGizmoComponent {

    public Transform A;
    public Transform B;
    public Transform C;
    public Transform D;

    public bool fullPoseSpline = false;

    public Color color = Color.white;

    private HermiteSpline3? _spline = null;
    private HermiteQuaternionSpline? _qSpline = null;

    void Update() {
      if (!fullPoseSpline) {
        Vector3 a = A.position, b = B.position, c = C.position, d = D.position;
        _spline = CatmullRom.ToCHS(a, b, c, d);
      }
      else {
        Pose a = A.ToPose(), b = B.ToPose(), c = C.ToPose(), d = D.ToPose();
        _spline = CatmullRom.ToCHS(a.position, b.position, c.position, d.position);
        _qSpline = CatmullRom.ToQuaternionCHS(a.rotation, b.rotation,
                                              c.rotation, d.rotation);
      }
    }
    
    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      drawer.color = color;

      if (!_spline.HasValue || (fullPoseSpline && !_qSpline.HasValue)) return;

      int resolution = 32;
      float incr = 1f / resolution;
      Vector3? lastPos = null;
      for (float t = 0; t <= 1f; t += incr) {
        var pos = _spline.Value.PositionAt(t);
        if (fullPoseSpline) {
          var rot = _qSpline.Value.RotationAt(t);

          drawer.DrawPose(new Pose(pos, rot), 0.01f);
        }

        if (lastPos.HasValue) {
          drawer.DrawLine(lastPos.Value, pos);
        }

        lastPos = pos;
      }
    }

  }

}
