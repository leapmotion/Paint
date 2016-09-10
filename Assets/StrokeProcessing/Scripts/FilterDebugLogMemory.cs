using UnityEngine;
using System.Text;

public class FilterDebugLogMemory : IMemoryFilter<StrokePoint> {

  public int GetMemorySize() {
    return 16;
  }

  public void Process(RingBuffer<StrokePoint> data, RingBuffer<int> indices) {
    Debug.Log("Data size is " + data.Size);
    StringBuilder sb = new StringBuilder();
    for (int i = 0; i < data.Size; i++) {
      sb.Append("Ring buffer " + i + " from end: ");
      sb.Append(data.GetFromEnd(i));
      sb.Append(" corresponds to data index " + indices.GetFromEnd(i));
      sb.Append("\n");
    }
    Debug.Log(sb.ToString());
  }

  public void Reset() {
    return;
  }

}