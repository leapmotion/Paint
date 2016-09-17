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
  int AcquisitionFrames = 5;
  int indexJitter = 0;
  Vector3 eraserPos = Vector3.zero;
  Leap.Hand _hand;

  float peaceStrength(Leap.Hand _hand) {
    return 
      -Vector3.Dot(_hand.Fingers[0].Direction.ToVector3(), _hand.Direction.ToVector3()) +
      Vector3.Dot(_hand.Fingers[1].Direction.ToVector3(), _hand.Direction.ToVector3()) +
      Vector3.Dot(_hand.Fingers[2].Direction.ToVector3(), _hand.Direction.ToVector3()) +
      //-Vector3.Dot(_hand.Fingers[3].Direction.ToVector3(), _hand.Direction.ToVector3()) + // Traitor finger!
      -Vector3.Dot(_hand.Fingers[4].Direction.ToVector3(), _hand.Direction.ToVector3());
  }

  void Update() {
    eraserPos = Vector3.zero;
    if(((RHand._paintCursor._handModel != null) && (RHand._paintCursor._handModel.IsTracked) && ((_hand = RHand._paintCursor._handModel.GetLeapHand()) != null) && (peaceStrength(_hand) > 1.3f)) ||
       ((LHand._paintCursor._handModel != null) && (LHand._paintCursor._handModel.IsTracked) && ((_hand = LHand._paintCursor._handModel.GetLeapHand()) != null) && (peaceStrength(_hand) > 1.3f))) {
         eraserPos = _hand.PalmPosition.ToVector3() + _hand.Direction.ToVector3() * 0.1f;
    }

    if (!hudAnchor.IsDisplaying && !eraserPos.Equals(Vector3.zero)) {
      LHand.deactivateDrawing = true; RHand.deactivateDrawing = true;
      this.transform.position = eraserPos;
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
        for (int j = indexJitter * 3; j < history.GetStrokes()[i].strokePoints.Count; j += (AcquisitionFrames * 3)) {
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