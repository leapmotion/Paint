using UnityEngine;
using System.Collections.Generic;


namespace Leap.Unity.LeapPaint_v3 {
  public interface IStrokeRenderer {

    void InitializeRenderer();
    void UpdateRenderer(List<StrokePoint> filteredStroke, int maxChangedFromEnd);
    void FinalizeRenderer();

  }


}

