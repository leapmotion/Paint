using UnityEngine;
using System.Collections;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity;

public class PaintCursor : MonoBehaviour, IRuntimeGizmoComponent {

  public PinchDetector _pinchDetector;
  public IndexTipColor _indexTipColor;

  public float _thicknessMult = 2.5F;

  [HideInInspector]
  public IHandModel _handModel;

  private float _radius = 0F;
  private float _minRadius = 0.02F;
  private float _maxRadius = 0.03F;
  private Color _cursorColor = Color.white;
  private Color _drawBeginMarkerCircleColor = Color.white;
  private bool _isPaintingPossible = true;
  private bool _canBeginPainting = true;
  private bool _isPainting = false;

  public Vector3 Position {
    get { return this.transform.position; }
  }
  public Quaternion Rotation {
    get { return this.transform.rotation; }
  }
  public bool IsActive {
    get { return this._pinchDetector.IsActive; }
  }
  public bool IsTracked {
    get { return this._pinchDetector.HandModel.IsTracked; }
  }
  public bool DidStartPinch {
    get { return this._pinchDetector.DidStartPinch; }
  }
  public Chirality Handedness {
    get { return this._pinchDetector.HandModel.Handedness; }
  }
  public Color Color {
    get { return _indexTipColor.GetColor(); }
  }

  protected virtual void Start() {
    _handModel = _pinchDetector.GetComponentInParent<IHandModel>();
    _minRadius = _pinchDetector.ActivateDistance / 2F;
  }

  protected virtual void Update() {
    float pinchRadius = _pinchDetector.Distance / 2;
    _radius = Mathf.Max(_minRadius, pinchRadius);
    float alpha = 1F - ((_radius - _minRadius) / (_maxRadius - _minRadius));

    if (!_isPaintingPossible) {
      alpha = 0F;
    }
    else if (!_canBeginPainting && !_isPainting) {
      alpha = 0F;
    }

    _cursorColor = Color.Lerp(_cursorColor, new Color(_cursorColor.r, _cursorColor.g, _cursorColor.b, alpha), 0.3F);
    _drawBeginMarkerCircleColor = Color.Lerp(_drawBeginMarkerCircleColor, new Color(_drawBeginMarkerCircleColor.r, _drawBeginMarkerCircleColor.g, _drawBeginMarkerCircleColor.b, alpha), 0.3F);
  }

  public void NotifyPossibleToActualize(bool isPossible) {
    _isPaintingPossible = isPossible;
  }

  public void NotifyPossibleToBeginActualizing(bool canBeginPainting) {
    _canBeginPainting = canBeginPainting;
  }

  public void NotifyIsPainting(bool isPainting) {
    _isPainting = isPainting;
  }

  #region Gizmos

  private bool _gizmosEnabled = true;

  public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    if (_gizmosEnabled) {
      drawer.PushMatrix();

      drawer.matrix = this.transform.localToWorldMatrix;

      drawer.color = _drawBeginMarkerCircleColor;
      drawer.DrawCircle(Vector3.zero, _minRadius * _thicknessMult, Vector3.up);

      drawer.color = _cursorColor;
      drawer.DrawCircle(Vector3.zero, _radius * _thicknessMult, Vector3.up);

      drawer.PopMatrix();
    }
  }

  #endregion

}

public static class RuntimeGizmoDrawerExtensions {
  public static void DrawCircle(this RuntimeGizmoDrawer drawer, Vector3 position, float radius, Vector3 planeDirection) {
    Vector3 perpDirection = Vector3.Cross(planeDirection, Vector3.up).normalized;
    if (perpDirection.magnitude < 0.99F) {
      perpDirection = Vector3.Cross(planeDirection, Vector3.right).normalized;
    }
    int numSegments = 64;
    for (int i = 0; i < numSegments; i++) {
      drawer.DrawLine(position + Quaternion.AngleAxis(360F * (i / (float)numSegments), planeDirection) * perpDirection * radius,
        position + Quaternion.AngleAxis(360F * ((i + 1) / (float)numSegments), planeDirection) * perpDirection * radius);
    }
  }
}