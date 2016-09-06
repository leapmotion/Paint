using UnityEngine;

public interface IMemoryFilter<T> {

  int GetMemorySize();
  void Process(RingBuffer<T> data);
  void Reset();

}