using UnityEngine;
using System.Text;

public class FilterLogMemory : IMemoryFilter<StrokePoint> {

  public int GetMemorySize() {
    return 16;
  }

  public void Process(RingBuffer<StrokePoint> data) {
    Debug.Log("Data size is " + data.Size);
    StringBuilder sb = new StringBuilder();
    for (int i = 0; i < data.Size; i++) {
      sb.Append("Ring buffer " + i + ": ");
      sb.Append(data.GetFromEnd(i));
    }
  }

  public void Reset() {
    return;
  }

}