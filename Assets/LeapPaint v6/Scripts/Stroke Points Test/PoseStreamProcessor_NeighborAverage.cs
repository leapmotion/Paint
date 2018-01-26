using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class PoseStreamProcessor_NeighborAverage : MonoBehaviour,
                                                     IStreamReceiver<Pose>,
                                                     IStream<Pose> {

    [Range(0, 16)]
    public int neighborRadius = 4;

    public event Action OnOpen  = () => { };
    public event Action<Pose> OnSend = (pose) => { };
    public event Action OnClose = () => { };

    private RingBuffer<Pose> buffer;

    private int totalWidth { get { return neighborRadius * 2 + 1; } }

    public void Open() {
      if (buffer == null || buffer.Capacity != totalWidth) {
        buffer = new RingBuffer<Pose>(totalWidth);
      }
      buffer.Clear();

      OnOpen();
    }

    public void Receive(Pose data) {
      if (buffer.Capacity == 0) {
        OnSend(data);
      }

      bool bufferWasNotFull = false;
      if (!buffer.IsFull) {
        bufferWasNotFull = true;
      }

      buffer.Add(data);

      if (buffer.IsFull) {
        if (bufferWasNotFull) {
          for (int i = 0; i < buffer.Length / 2; i++) {
            OnSend(new Pose(getAverage(0, i)));
          }
        }

        OnSend(new Pose(getAverage(0, buffer.Length)));
      }
    }

    private Vector3 getAverage(int start, int end) {
      if (start == end) return buffer.Get(start).position;

      var sum = Vector3.zero;
      for (int i = start; i < end; i++) {
        sum += buffer.Get(i).position;
      }
      return sum / (end - start);
    }

    public void Close() {
      var finalPose = buffer.GetLatest();

      for (int i = 0; i < buffer.Length - 1; i++) {
        Receive(finalPose);
      }

      OnClose();
    }

  }

}
