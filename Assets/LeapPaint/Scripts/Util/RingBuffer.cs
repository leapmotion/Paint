using UnityEngine;

[System.Serializable]
public class RingBuffer<T> {

  [SerializeField]
  private T[] _data;
  private int _start;
  private int _size;

  public RingBuffer(int capacity) {
    _data = new T[capacity];
    _start = 0;
    _size = 0;
  }

  public int Size { get { return _size; } }
  public int Capacity { get { return _data.Length; } }

  private int RingIndex(int index) {
    return (_start + index) % _size;
  }

  private int RingIndexFromEnd(int indexFromEnd) {
    return RingIndex(_size - 1 - indexFromEnd);
  }

  public void Add(T t) {
    if (_size == _data.Length) {
      _start += 1; _start %= _size; 
    }
    else {
      _size = Mathf.Min(_data.Length, _size + 1);
    }

    _data[RingIndex(_size - 1)] = t;
  }

  public T Get(int index) {
    if (_size == 0) {
      Debug.LogWarning("[RingBuffer] Tried to Get, but this RingBuffer is empty.");
      return default(T);
    }
    return _data[RingIndex(index)];
  }

  public void Set(int index, T t) {
    _data[RingIndex(index)] = t;
  }

  public T GetFromEnd(int indexFromEnd) {
    if (_size == 0) {
      Debug.LogWarning("[RingBuffer] Tried to GetFromEnd, but this RingBuffer is empty.");
      return default(T);
    }
    return _data[RingIndexFromEnd(indexFromEnd)];
  }

  public void SetFromEnd(int indexFromEnd, T t) {
    _data[RingIndexFromEnd(indexFromEnd)] = t;
  }

  public void Clear() {
    _start = 0;
    _size = 0;
  }

  //public T[] Slice(int start, int end) {
  //  T[] sliced = new T[end - start];
  //  int copyCount = 0;
  //  for (int i = start; i < end && i < _list.Count; i++) {
  //    sliced[i - start] = _list[i];
  //    copyCount++;
  //  }
  //  if (copyCount < sliced.Length) {
  //    T[] slicedSmaller = new T[copyCount];
  //    for (int i = 0; i < copyCount; i++) {
  //      slicedSmaller[i] = sliced[i];
  //    }
  //    sliced = slicedSmaller;
  //  }
  //  return sliced;
  //}
  ///// <summary> Returns a right-justified slice window on the array of length (end-start) </summary>
  //public T[] TailSlice(int start, int end) {
  //  int offset = _list.Count - end;
  //  return Slice(start + offset, end + offset);
  //}

}