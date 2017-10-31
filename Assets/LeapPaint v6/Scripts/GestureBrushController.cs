using Leap.Unity.Attributes;
using Leap.Unity.Drawing;
using Leap.Unity.Gestures;
using Leap.Unity.Intention;
using UnityEngine;

namespace Leap.Unity.LeapPaint {

  public class GestureBrushController : MonoBehaviour {

    [ImplementsInterface(typeof(IBrush))]
    [SerializeField]
    private MonoBehaviour _brush;
    public IBrush brush {
      get { return _brush as IBrush; }
      set { _brush = value as MonoBehaviour; }
    }

    [SerializeField, OnEditorChange("poseGesture")]
    [ImplementsInterface(typeof(IPoseGesture))]
    private MonoBehaviour _poseGesture;
    public IPoseGesture poseGesture {
      get { return _poseGesture as IPoseGesture; }
      set { _poseGesture = value as MonoBehaviour; }
    }

    void Update() {
      brush.Move(poseGesture.currentPose);

      if (poseGesture.isActive && !brush.isBrushing) {
        brush.Begin();
      }

      if (!poseGesture.isActive && brush.isBrushing) {
        brush.End();
      }
    }

  }

}
