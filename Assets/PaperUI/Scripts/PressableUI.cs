using UnityEngine;
using System.Collections;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity;
using UnityEngine.Events;

namespace Leap.zzOldPaint {

  public class PressableUI : MonoBehaviour, IRuntimeGizmoComponent {

    #region Setup

    public float _xWidth = 1F;
    public float _zWidth = 1F;
    public float _maxPenetrationDistance = 1F;

    private PressableUIManager _manager;
    private UIActivator _activator;

    protected void OnValidate() {
      OnValidateLayers();
    }

    protected virtual void Start() {
      OnValidateLayers();
      InitLayerState();
      _manager = GameObject.FindObjectOfType<PressableUIManager>();
      _manager.RegisterPressable(this);
    }

    #endregion

    #region Properties

    public bool IsActivated {
      get { return _activator != null; }
    }

    public UIActivator Activator {
      get { return _activator; }
    }

    #endregion

    protected void FixedUpdate() {
      FixedActivatorUpdate();
    }

    protected void Update() {
      LayerUpdate();
    }

    #region Activator State

    public void NotifyClosestActivator(UIActivator activator) {
      ProcessActivator(activator);
    }

    private void FixedActivatorUpdate() {
      if (_activator != null) {
        ProcessActivator(_activator);
      }
    }

    private void ProcessActivator(UIActivator activator) {
      if (!this.isActiveAndEnabled) return;
      if (GetUnsignedWorldPointActivationVolumeDistance(activator.transform.position) < this.transform.InverseTransformVector(Vector3.up * activator.WorldRadius).magnitude) {
        _activator = activator;
      }
      else {
        _activator = null;
      }
    }

    #endregion

    #region Layer State

    [System.Serializable]
    public struct Layer {
      [HideInInspector]
      public string label;
      public Transform layerTransform;
      public float maxHeight;
      public float minHeight;
      [HideInInspector]
      public float height;
      [HideInInspector]
      public float upperLayersThickness;
    }
    public Layer[] _layers = new Layer[0];

    public UnityEvent OnPress;
    public UnityEvent OnRelease;

    public SoundEffect soundEffect;

    private bool _pressed = false;

    private float _activationVolumeHeight;
    private float _totalLayerHeight;

    private SmoothedFloat _smoothedLimitedPressDepth = new SmoothedFloat();

    private void OnValidateLayers() {
      _totalLayerHeight = 0F;
      for (int i = 0; i < _layers.Length; i++) {
        _layers[i].label = "Layer " + (i);
        _totalLayerHeight += _layers[i].maxHeight;
        float upperLayersThickness = 0F;
        for (int j = i; j < _layers.Length; j++) {
          upperLayersThickness += _layers[j].minHeight;
        }
        _layers[i].upperLayersThickness = upperLayersThickness;
      }
      _activationVolumeHeight = _maxPenetrationDistance + _totalLayerHeight;
    }

    private void InitLayerState() {
      _smoothedLimitedPressDepth.delay = 0.04F;
      _smoothedLimitedPressDepth.reset = true;
      _smoothedLimitedPressDepth.value = _totalLayerHeight;
    }

    protected virtual void LayerUpdate() {
      float rawPressDistance = GetRawPressDistance();
      if (rawPressDistance <= 0F && !_pressed) {
        OnPress.Invoke();
        _pressed = true;
        soundEffect.PlayOnTransform(transform, 1);
      }
      else if (rawPressDistance > 0F && _pressed) {
        OnRelease.Invoke();
        _pressed = false;
      }
      float limitedRawPressDepth = Mathf.Max(0F, rawPressDistance);
      _smoothedLimitedPressDepth.Update(limitedRawPressDepth, Time.deltaTime);

      float distanceLeft = GetPressDistance();
      float distanceAccum = 0F;
      for (int i = 0; i < _layers.Length; i++) {
        Layer layer = _layers[i];
        float layerHeight = Mathf.Min(layer.maxHeight, Mathf.Max(layer.minHeight, distanceLeft - layer.upperLayersThickness));
        _layers[i].height = distanceAccum + layerHeight;
        if (_layers[i].layerTransform != null) {
          _layers[i].layerTransform.position = this.transform.TransformPoint(Vector3.up * _layers[i].height);
        }
        distanceLeft -= layerHeight;
        distanceAccum += layerHeight;
      }
    }

    #endregion

    #region Utility

    public float GetPressDistance() {
      return _smoothedLimitedPressDepth.value;
    }

    private Vector3 GetPressOffset() {
      return Vector3.up * GetPressDistance();
    }

    private float GetRawPressDistance() {
      if (_activator == null) {
        return _totalLayerHeight;
      }
      else {
        return ConvertWorldLengthToLocal(Vector3.Dot(this.transform.up, (_activator.transform.position - this.transform.position)) - _activator.WorldRadius, Vector3.up);
      }
    }

    private Vector3 GetRawPressOffset() {
      return Vector3.up * GetRawPressDistance();
    }

    // General utility

    protected Vector3 GetActivatorWorldPosition() {
      if (_activator != null) {
        return _activator.transform.position;
      }
      else {
        return Vector3.zero;
      }
    }

    protected float ConvertWorldLengthToLocal(float length, Vector3 localDirection) {
      int polarity = length < 0F ? -1 : 1;
      localDirection = localDirection.normalized;
      return new Vector3(length * localDirection.x / transform.lossyScale.x, length * localDirection.y / transform.lossyScale.y, length * localDirection.z / transform.lossyScale.z).magnitude * polarity;
    }

    private Vector3 GetActivationVolumeBoxBounds() {
      return new Vector3(_xWidth, _activationVolumeHeight, _zWidth);
    }

    private Vector3 GetActivationVolumeBoxOffset() {
      return Vector3.up * (_activationVolumeHeight / 2F - _maxPenetrationDistance);
    }

    private float GetUnsignedWorldPointActivationVolumeDistance(Vector3 worldPoint) {
      return UnsignedDistanceBox(
        this.transform.InverseTransformPoint(worldPoint) - GetActivationVolumeBoxOffset(),
        GetActivationVolumeBoxBounds() / 2F
        );
    }

    // http://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
    private float UnsignedDistanceBox(Vector3 p, Vector3 b) {
      return Length(Max(Abs(p) - b, Vector3.zero));
    }
    private float SignedDistanceBox(Vector3 p, Vector3 b) {
      Vector3 d = Abs(p) - b;
      return Min(Max(d.x, Max(d.y, d.z)), 0.0F) + Length(Max(d, Vector3.zero));
    }
    private Vector3 Abs(Vector3 vec) {
      return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
    }
    private float Min(float a, float b) {
      return Mathf.Min(a, b);
    }
    private float Max(float a, float b) {
      return Mathf.Max(a, b);
    }
    private Vector3 Max(Vector3 a, Vector3 b) {
      return Vector3.Max(a, b);
    }
    private float Length(Vector3 v) {
      return v.magnitude;
    }

    #endregion

    public void PlayActivationSoundEffect() {
      soundEffect.PlayOnTransform(transform, 1);
    }

    #region Gizmos

    protected bool _enableGizmos = false;
    private RingBuffer<Color> _gizmoColors;

    public virtual void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (_enableGizmos) {
        drawer.PushMatrix();
        drawer.matrix = this.transform.localToWorldMatrix;

        drawer.color = Color.green;
        if (IsActivated) { drawer.color = Color.white; }
        drawer.DrawWireCube(GetActivationVolumeBoxOffset(), GetActivationVolumeBoxBounds());

        if (_gizmoColors == null) {
          _gizmoColors = new RingBuffer<Color>(4);
          _gizmoColors.Add(Color.red);
          _gizmoColors.Add(Color.yellow);
          _gizmoColors.Add(Color.green);
          _gizmoColors.Add(Color.blue);
        }
        for (int i = 0; i < _layers.Length; i++) {
          drawer.color = _gizmoColors.Get(i);
          drawer.DrawWireCube(Vector3.up * _layers[i].height, new Vector3(_xWidth - 0.2F, 0.01F, _zWidth - 0.2F));
          drawer.DrawWireCube(Vector3.up * _layers[i].height, new Vector3(_xWidth - 0.4F, 0.01F, _zWidth - 0.4F));
          drawer.DrawWireCube(Vector3.up * _layers[i].height, new Vector3(_xWidth - 0.6F, 0.01F, _zWidth - 0.6F));
        }

        drawer.PopMatrix();
      }
    }

    #endregion

  }

}