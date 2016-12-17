using UnityEngine;
using System.Collections.Generic;
using StrokeProcessing;

namespace Leap.Paint {

  public interface IStrokeBufferRenderer {

    void InitializeRenderer();
    void RefreshRenderer(RingBuffer<StrokePoint> strokeBuffer);
    void StopRenderer();

  }

}