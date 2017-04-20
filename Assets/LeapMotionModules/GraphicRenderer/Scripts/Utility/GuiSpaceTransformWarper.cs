using Leap.Unity.Space;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.GraphicalRenderer {

  [ExecuteInEditMode]
  [RequireComponent(typeof(LeapGraphic))]
  public class GuiSpaceTransformWarper : MonoBehaviour {

    public Transform toWarp;

    private LeapGraphic _element;
    private LeapGraphicRenderer _attachedGui;
    private ITransformer _spaceWarper; // Space Warper, Transformer of Curved Spaces

    void Start() {
      TryInitialize();
    }

    private void TryInitialize() {
      try {
        _element = GetComponent<LeapGraphic>();
        _attachedGui = GetComponentInParent<LeapGraphicRenderer>();
        _spaceWarper = _element.transformer;
      }
      catch (System.Exception) { }
    }

    void Update() {
      if (_spaceWarper == null) {
        TryInitialize();
      }

      if (_spaceWarper != null && toWarp != null) {

        var localPosInRectSpace = _attachedGui.transform.InverseTransformPoint(this.transform.position);

        toWarp.transform.position = _attachedGui.transform.TransformPoint(
                                      _spaceWarper.TransformPoint(
                                        localPosInRectSpace));

        toWarp.transform.rotation = _attachedGui.transform.TransformRotation(
                                      _spaceWarper.TransformRotation(
                                        localPosInRectSpace,
                                        _attachedGui.transform.InverseTransformRotation(
                                          this.transform.rotation)));
      }
    }

  }


}
