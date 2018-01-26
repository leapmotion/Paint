using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class PoseStreamProcessor_BezierSmoothPoints : MonoBehaviour,
                                                        IStreamReceiver<Pose>,
                                                        IStream<Pose> {

    public float samplesPerMeter = 256f;

    public event Action OnOpen  = () => { };
    public event Action<Pose> OnSend = (pose) => { };
    public event Action OnClose = () => { };
    
    private RingBuffer<Pose> _poseBuffer = new RingBuffer<Pose>(4);

    public void Open() {
      _poseBuffer.Clear();

      OnOpen();
    }

    private Vector3[] positionsBuffer = new Vector3[4];
    private float[] timesBuffer = new float[4];
    private Vector3[] smoothedPositionsBuffer = new Vector3[512];
    
    public void Receive(Pose data) {
      bool wasNotFull = false;
      if (!_poseBuffer.IsFull) wasNotFull = true;

      _poseBuffer.Add(data);

      if (_poseBuffer.IsFull) {
        if (wasNotFull) {
          send(_poseBuffer.Get(0), _poseBuffer.Get(0),
               _poseBuffer.Get(1), _poseBuffer.Get(2));
        }
        send(_poseBuffer.Get(0), _poseBuffer.Get(1),
             _poseBuffer.Get(2), _poseBuffer.Get(3));
      }
    }

    private void send(Pose a, Pose b, Pose c, Pose d, bool reverseOutput = false) {
      positionsBuffer[0] = a.position;
      positionsBuffer[1] = b.position;
      positionsBuffer[2] = c.position;
      positionsBuffer[3] = d.position;

      timesBuffer[0] = -1;
      timesBuffer[1] = 0;
      timesBuffer[2] = 1;
      timesBuffer[3] = 2;

      var interpolatedLength = (positionsBuffer[2] - positionsBuffer[1]).magnitude;
      var numSamples = getNumSamples(interpolatedLength);

      Splines.CatmullRom.InterpolatePoints(positionsBuffer, timesBuffer,
                                           ref smoothedPositionsBuffer,
                                           numPoints: numSamples);
      if (!reverseOutput) {
        for (int i = 0; i < numSamples - 1; i++) {
          OnSend(new Pose(smoothedPositionsBuffer[i]));
        }
      }
      else {
        for (int i = numSamples - 1; i >= 0; i--) {
          OnSend(new Pose(smoothedPositionsBuffer[i]));
        }
      }
    }

    private int getNumSamples(float length) {
      var numSamples = Mathf.FloorToInt(length * samplesPerMeter);
      numSamples = Mathf.Max(2, numSamples);
      return numSamples;
    }

    private Vector3 getBezier(Vector3 a, Vector3 b, Vector3 c, float t) {
      var lerpA = Vector3.Lerp(a, b, t);
      var lerpB = Vector3.Lerp(b, c, t);
      return Vector3.Lerp(lerpA, lerpB, t);
    }

    public void Close() {
      if (_poseBuffer.Length < 2) {
        return;
      }
      if (_poseBuffer.Length == 2) {
        OnSend(_poseBuffer.Get(0));
        OnSend(_poseBuffer.Get(1));
      }
      else {
        send(_poseBuffer.Get(3), _poseBuffer.Get(3),
             _poseBuffer.Get(2), _poseBuffer.Get(1),
             reverseOutput: true);
      }

      OnClose();
    }

  }

}
