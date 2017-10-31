using UnityEngine;
using System.Collections;


namespace Leap.Unity.LeapPaint_v3 {


  public class FilterIndexTipColor : MonoBehaviour, IBufferFilter<StrokePoint> {

    public IndexTipColor _indexTipColor;

    public int GetMinimumBufferSize() {
      return 0;
    }

    public void Process(RingBuffer<StrokePoint> data, RingBuffer<int> indices) {
      StrokePoint s = data.GetLatest();
      s.color = _indexTipColor.GetColor();
      data.SetLatest(s);
    }

    public void Reset() {
      return;
    }
  }


}