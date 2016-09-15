using UnityEngine;
using System.Collections.Generic;

public interface IStrokeBufferRenderer {

  void InitializeRenderer();
  void RefreshRenderer(RingBuffer<StrokePoint> strokeBuffer);
  void StopRenderer();

}