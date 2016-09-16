using UnityEngine;
using System.Collections;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity;

public class Eraser : MonoBehaviour, IRuntimeGizmoComponent {
  public float radius = 0.05f;
  public Transform EraserPosition;
  public HistoryManager history;
  public PinchStrokeProcessor LHand;
  public PinchStrokeProcessor RHand;
  public WearableAnchor hudAnchor;
  Stroke strokeToKill;
  int strokeIndexToKill = -1;
  int AcquisitionFrames = 3;
  int indexJitter = 0;

  void Update() {
    if (!hudAnchor.IsDisplaying&&Vector3.Distance(LHand._paintCursor.transform.position, RHand._paintCursor.transform.position) < 0.1f) {
      LHand.deactivateDrawing = true; RHand.deactivateDrawing = true;
      this.transform.position = (LHand._paintCursor.transform.position + RHand._paintCursor.transform.position) / 2f;
      this.transform.rotation = EraserPosition.rotation;

      if (indexJitter == AcquisitionFrames - 1) {
        indexJitter = 0;
      } else {
        indexJitter++;
      }

      float minDist = 0.05f * 0.05f;
      bool selectThisStroke = false;
      strokeToKill.strokePoints = null;
      strokeIndexToKill = -1;
      for (int i = 0; i < history.GetStrokes().Count; i++) {
        selectThisStroke = false;
        for (int j = indexJitter * 5; j < history.GetStrokes()[i].strokePoints.Count; j += (AcquisitionFrames * 5)) {
          float sqrDist = (transform.position - history.GetStrokes()[i].strokePoints[j].position).sqrMagnitude;
          if (sqrDist < minDist) {
            selectThisStroke = true;
            minDist = sqrDist;
            break;
          }
        }
        if (selectThisStroke) {
          strokeIndexToKill = i;
          strokeToKill = history.GetStrokes()[i];
          break;
        }
      }

      if (strokeIndexToKill != -1) {
        history.Undo(strokeIndexToKill);
      }
    } else {
      LHand.deactivateDrawing = false; RHand.deactivateDrawing = false;
    }
  }
  
  public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    if (LHand.deactivateDrawing) {
      if (strokeIndexToKill != -1) {
        drawer.color = Color.red;
      }
      drawer.DrawWireSphere(transform.position, radius);
    }
  }
}