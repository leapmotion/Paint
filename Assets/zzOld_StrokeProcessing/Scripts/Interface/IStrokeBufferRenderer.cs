using UnityEngine;
using System.Collections.Generic;
using zzOldStrokeProcessing;

namespace Leap.zzOldPaint {

  public interface IStrokeBufferRenderer {

    void InitializeRenderer();
    void RefreshRenderer(RingBuffer<StrokePoint> strokeBuffer);
    void StopRenderer();

  }

}