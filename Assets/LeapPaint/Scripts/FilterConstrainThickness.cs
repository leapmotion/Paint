using UnityEngine;
using Leap.Unity.RuntimeGizmos;

public class FilterConstrainThickness : IMemoryFilter<StrokePoint> {

  public int GetMemorySize() {
    return 4;
  }

  public void Process(RingBuffer<StrokePoint> data, RingBuffer<int> indices) {
    if (data.Size < 15) {
      return;
    }
    
    var currStroke = data.GetFromEnd(8);

    float thickness = data.GetFromEnd(0).thickness;

    /*
    for(int i=9; i<14; i++) {
      var prevStroke = data.GetFromEnd(i);
      Plane prevPlane = new Plane(prevStroke.rotation * Vector3.forward, prevStroke.position);

      Ray currRay = new Ray(currStroke.position, currStroke.rotation * Vector3.right);
      float dist = 0;
      prevPlane.Raycast(currRay, out dist);

      thickness = Mathf.Min(thickness, Mathf.Abs(dist));
    }
    */

    /*
    for(int i=0; i<=14; i++) {
      var da = data.GetFromEnd(i);
      da.thickness = thickness;
      data.SetFromEnd(i, da);
    }
    */
  }

  public void Reset() { }
}
