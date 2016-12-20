using UnityEngine;
using System.Collections;
using zzOldStrokeProcessing;

namespace Leap.zzOldPaint {

  public class FilterIndexTipColor : MonoBehaviour, IBufferFilter<StrokePoint> {

    public IndexTipColor _indexTipColor;

    public int GetMinimumBufferSize() {
      return 0;
    }

    public void Process(RingBuffer<StrokePoint> data, RingBuffer<int> indices) {
      StrokePoint s = data.GetFromEnd(0);
      s.color = _indexTipColor.GetColor();
      data.SetFromEnd(0, s);
    }

    public void Reset() {
      return;
    }
  }


}