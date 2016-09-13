using UnityEngine;
using System.Collections;

public class FilterIndexTipColor : MonoBehaviour, IMemoryFilter<StrokePoint> {

  public IndexTipColor _indexTipColor;

  public int GetMemorySize() {
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
