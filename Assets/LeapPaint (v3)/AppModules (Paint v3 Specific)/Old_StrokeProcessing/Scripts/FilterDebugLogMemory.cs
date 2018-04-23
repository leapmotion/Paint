using UnityEngine;
using System.Text;


namespace Leap.Unity.LeapPaint_v3 {

  public class FilterDebugLogMemory : IBufferFilter<StrokePoint> {

    public int GetMinimumBufferSize() {
      return 16;
    }

    public void Process(RingBuffer<StrokePoint> data, RingBuffer<int> indices) {
      Debug.Log("Data size is " + data.Count);
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < data.Count; i++) {
        sb.Append("Ring buffer " + i + " from end: ");
        sb.Append(data.GetFromEnd(i));
        sb.Append(" corresponds to data index " + indices.Get(data.Count - 1 - i));
        sb.Append("\n");
      }
      Debug.Log(sb.ToString());
    }

    public void Reset() {
      return;
    }

  }

}