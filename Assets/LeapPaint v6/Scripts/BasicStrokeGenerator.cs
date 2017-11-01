using Leap.Unity.Meshing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Drawing {

  public class BasicStrokeGenerator : MonoBehaviour, IStrokeGenerator {

    public const int MAX_NUM_STROKE_POINTS = 256;

    [Header("Stroke Object Output")]
    public StrokeObject outputStrokeObjectPrefab;
    public Transform  outputParentObject;

    private StrokeObject _curStrokeObject;

    private bool _strokeInProgress = false;

    private void initStroke() {
      _curStrokeObject = Instantiate(outputStrokeObjectPrefab);
      _curStrokeObject.transform.parent = outputParentObject;
    }

    private void addToStroke(Vector3 position, Vector3 normal,
                                 Color color, float size) {

      using (new ProfilerSample("addToStroke: Restart Stroke")) {
        if (_curStrokeObject.Count > MAX_NUM_STROKE_POINTS) {
          finalizeStroke();
          initStroke();
        }
      }

      using (new ProfilerSample("addToStroke: Modify Stroke")) {
        _curStrokeObject.Add(new StrokePoint() {
          position = position,
          normal = normal,
          color = color,
          size = size
        });
      }
    }

    private void finalizeStroke() {
      _curStrokeObject = null;
    }

    #region IStrokeGenerator

    public void Initialize() {
      if (_strokeInProgress) {
        finalizeStroke();
      }

      _strokeInProgress = false;
    }

    public void AddPoint(Vector3 position, Vector3 normal, Color color, float size) {
      if (!_strokeInProgress) {
        _strokeInProgress = true;

        initStroke();
      }

      using (new ProfilerSample("BasicStrokeGenerator addToStroke")) {
        addToStroke(position, normal, color, size);
      }
    }

    public void Finish() {
      if (_strokeInProgress) {
        finalizeStroke();
      }

      _strokeInProgress = false;
    }

    #endregion

  }

}
