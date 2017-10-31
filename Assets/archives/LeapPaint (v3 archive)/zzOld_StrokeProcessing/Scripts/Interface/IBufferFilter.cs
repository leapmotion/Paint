using UnityEngine;

namespace Leap.Unity.LeapPaint_v3 {

  public interface IBufferFilter<T> {

    /// <summary>
    /// The buffer size will be as large as the largest registered filter buffer size requested here.
    /// </summary>
    int GetMinimumBufferSize();

    /// <summary>
    /// Process is called each time a new datapoint is added.
    /// 
    /// actualizedIndices is of equal size to data and contains the actualized stroke index for that datapoint,
    /// or -1 if the stroke is not being actualized. 0 indicates the beginning of the stroke, and the largest
    /// index in the list is the end of the stroke (followed by -1s).
    /// </summary>
    void Process(RingBuffer<T> data, RingBuffer<int> actualizedIndices);

    /// <summary>
    /// Called when a new stroke begins.
    /// </summary>
    void Reset();

  }

}