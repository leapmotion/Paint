using UnityEngine;
using System.Collections;

public class TestPrinting : MonoBehaviour {

  public Renderer notifierRenderable;
  public Material onMat;
  public Material offMat;

  public void PalmInwardPrint() {
    Debug.Log("Palm inward detected.");
    notifierRenderable.material = onMat;
  }

  public void PalmInwardCeasedPrint() {
    Debug.Log("Palm inward ceased.");
    notifierRenderable.material = offMat;
  }

}
