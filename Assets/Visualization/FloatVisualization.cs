using Leap.Unity.RuntimeGizmos;
using Leap.Unity.Visualization.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Visualization {

  public class FloatVisualization : VisualizationBehaviour, IRuntimeGizmoComponent {

    public RingBuffer<float> sampleBuffer;

    private bool needsInit = true;

    void Start() {
      if (needsInit) Initialize();
    }

    public void Add(float sample) {
      if (needsInit) {
        Initialize();
      }
      sample = Mathf.Clamp(sample, minValue, maxValue);
      sampleBuffer.Add(sample);
    }

    private void Initialize() {
      sampleBuffer = new RingBuffer<float>(maxSamples);
      needsInit = false;
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (sampleBuffer.Count == 0) return;
      
      float smallestValue = 0;
      float largestValue = 0;
      if (rangeSpecified) {
        smallestValue = this.minValue;
        largestValue = this.maxValue;
      }
      else {
        for (int i = 0; i < sampleBuffer.Count; i++) {
          float sampleValue = sampleBuffer.GetFromEnd(i);
          if (sampleValue < smallestValue) {
            smallestValue = sampleValue;
          }
          if (sampleValue > largestValue) {
            largestValue = sampleValue;
          }
        }
      }

      drawer.color = this.color;

      drawer.matrix = this.transform.localToWorldMatrix;
      float radius = 0.01F;
      for (int i = 0; i < sampleBuffer.Count; i++) {
        Vector3 initPos = Vector3.right * radius * sampleBuffer.Count;
        Vector3 posOffset = Vector3.left * radius * i;
        Vector3 valueReadout = Vector3.up * sampleBuffer.GetFromEnd(i).Map(smallestValue, largestValue, 0F, radius * 20F);
        drawer.DrawSphere(initPos + posOffset + valueReadout, radius);
      }
    }

  }

}