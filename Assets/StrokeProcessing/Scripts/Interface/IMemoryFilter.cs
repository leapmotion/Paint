using UnityEngine;

public interface IMemoryFilter<T> {

  int GetMemorySize();
  void Process(RingBuffer<T> data, RingBuffer<int> indices);
  void Reset();

}