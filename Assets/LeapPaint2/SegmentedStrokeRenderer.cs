using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Paint2 {

  [RequireComponent(typeof(SegmentRenderer))]
  public class SegmentedStrokeRenderer : StrokeRendererBase {

    private SegmentRenderer _segmentRenderer;
    public SegmentRenderer SegmentRenderer {
      get {
        if (_segmentRenderer == null) _segmentRenderer = GetComponent<SegmentRenderer>();
        return _segmentRenderer;
      }
    }

    public override void UpdateRenderer(Stroke stroke, StrokeModificationHint modHint) {

      switch (modHint.modType) {
        case StrokeModificationType.Overhaul:
          SegmentRenderer.Initialize();
          throw new System.NotImplementedException(); // TODO: Support via repeated AddedPoint handling?
        case StrokeModificationType.AddedPoint:
          if (stroke.Count == 0) {
            //SegmentRenderer.AddPoint(stroke[0]);
            //SegmentRenderer.AddStartCap(0);
            //SegmentRenderer.AddEndCap(0);
          }
          else {
            //SegmentRenderer.RemoveEndCapAtEnd();
            //SegmentRenderer.AddPoint(stroke[stroke.Count - 1]);
            //SegmentRenderer.AddEndCap(stroke.Count - 1);
          }
          break;
        default:
          throw new System.NotImplementedException();
      }

    }

  }

}