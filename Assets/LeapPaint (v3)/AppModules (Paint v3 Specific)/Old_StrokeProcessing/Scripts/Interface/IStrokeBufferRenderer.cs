using UnityEngine;
using System.Collections.Generic;


namespace Leap.Unity.LeapPaint_v3 {

  public interface IStrokeBufferRenderer {

    void InitializeRenderer();
    void RefreshRenderer(RingBuffer<StrokePoint> strokeBuffer);
    void StopRenderer();

  }

}