using UnityEngine;
using System.Collections;

public class FilterPitchYawTangent2 : FilterPitchYawTangent {

  private float globalRoll = 0F;

  public override void Process(RingBuffer<StrokePoint> data, RingBuffer<int> indices) {
    base.Process(data, indices);

    //if (data.Size == 8) {
    //  Vector3 p0 = data.GetFromEnd(7);
    //  Vector3 p7 = data.GetFromEnd(0);
    //  Vector3 longSegment = (p7 - p0).normalized;

    //}
  }

  public override void Reset() {
    base.Reset();
    globalRoll = 0F;
  }

}
