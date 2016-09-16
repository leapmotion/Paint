using UnityEngine;
using System.Collections;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity;

public class Eraser : MonoBehaviour, IRuntimeGizmoComponent {
  public float radius = 0.05f;
  public Transform EraserPosition;
  public HistoryManager history;
  Stroke strokeToKill;
  int AcquisitionFrames = 3;
  int indexJitter = 0;

  void Update() {
    this.transform.position = EraserPosition.position;
    this.transform.rotation = EraserPosition.rotation;

    if (indexJitter == AcquisitionFrames-1) {
      indexJitter = 0;
    } else {
      indexJitter++;
    }

    float minDist = 0.05f*0.05f;
    bool selectThisStroke = false;
    strokeToKill.strokePoints = null;
    for (int i = 0; i < history.GetStrokes().Count; i ++) {
      selectThisStroke = false;
      for (int j = indexJitter*5; j < history.GetStrokes()[i].strokePoints.Count; j +=(AcquisitionFrames*5)) {
        float sqrDist = (transform.position - history.GetStrokes()[i].strokePoints[j].position).sqrMagnitude;
        if(sqrDist < minDist){
          selectThisStroke = true;
          minDist = sqrDist;
          break;
        }
      }
      if (selectThisStroke) {
        strokeToKill = history.GetStrokes()[i];
        break;
      }
    }
  }
  
  public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    if (strokeToKill.strokePoints != null) {
      drawer.DrawLine(transform.position, strokeToKill.strokePoints[0].position);
      drawer.color = Color.red;
    }
    drawer.DrawWireSphere(transform.position, radius);
  }
}