using UnityEngine;
using System.Collections;

namespace Leap.Unity.LeapPaint_v3 {


  public class ColorMarble : MonoBehaviour {

    public MeshRenderer _leftColorMeshRenderer;
    public MeshRenderer _rightColorMeshRenderer;

    public void NotifyLeftColor(Color color) {
      _leftColorMeshRenderer.material.color = color;
    }

    public void NotifyRightColor(Color color) {
      _rightColorMeshRenderer.material.color = color;
    }

  }


}
