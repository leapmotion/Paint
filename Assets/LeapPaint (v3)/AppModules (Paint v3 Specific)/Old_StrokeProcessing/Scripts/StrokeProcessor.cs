using UnityEngine;
using System.Collections.Generic;


namespace Leap.Unity.LeapPaint_v3 {

  public class StrokeProcessor {

    // Stroke processing configuration
    private List<IBufferFilter<StrokePoint>> _strokeFilters = null;
    private int _maxMemory = 0;

    // Stroke state
    private bool _isBufferingStroke = false;
    private bool _isActualizingStroke = false;

    private RingBuffer<StrokePoint> _strokeBuffer;
    private RingBuffer<int> _actualizedStrokeIdxBuffer;
    private int _actualizedStrokeIdx = 0;

    private List<StrokePoint> _strokeOutput = null;
    private int _outputBufferEndOffset = 0;

    public bool IsBufferingStroke { get { return _isBufferingStroke; } }
    public bool IsActualizingStroke { get { return _isActualizingStroke; } }

    // Stroke renderers
    private List<IStrokeRenderer> _strokeRenderers = null;

    // Stroke buffer renderers
    private List<IStrokeBufferRenderer> _strokeBufferRenderers = null;

    public StrokeProcessor() {
      _strokeFilters = new List<IBufferFilter<StrokePoint>>();
      _strokeRenderers = new List<IStrokeRenderer>();
      _strokeBufferRenderers = new List<IStrokeBufferRenderer>();
      _strokeOutput = new List<StrokePoint>();
    }

    public void RegisterStrokeFilter(IBufferFilter<StrokePoint> strokeFilter) {
      _strokeFilters.Add(strokeFilter);

      int filterMemorySize = strokeFilter.GetMinimumBufferSize();
      if (filterMemorySize + 1 > _maxMemory) {
        _maxMemory = Mathf.Max(1, filterMemorySize);
      }

      if (_isBufferingStroke) {
        Debug.LogWarning("[StrokeProcessor] Registering stroke filters destroys the current stroke processing queue.");
      }
      _strokeBuffer = new RingBuffer<StrokePoint>(_maxMemory);
      _actualizedStrokeIdxBuffer = new RingBuffer<int>(_maxMemory);
    }

    public void RegisterStrokeRenderer(IStrokeRenderer strokeRenderer) {
      _strokeRenderers.Add(strokeRenderer);
      if (_isBufferingStroke) {
        Debug.LogError("[StrokeProcessor] Stroke in progress; Newly registered stroke renderers will not render the entire stroke if a stroke is already in progress.");
      }
    }

    public void RegisterPreviewStrokeRenderer(IStrokeBufferRenderer strokeBufferRenderer) {
      _strokeBufferRenderers.Add(strokeBufferRenderer);
      if (_isBufferingStroke) {
        Debug.LogError("[StrokeProcessor] Stroke buffer already active; Newly registered stroke buffer renderers will not render the entire preview stroke if a stroke is already in progress.");
      }
    }

    public void BeginTrackingStroke() {
      if (_isBufferingStroke) {
        Debug.LogError("[StrokeProcessor] Stroke in progress; cannot begin new stroke. Call EndStroke() to finalize the current stroke first.");
        return;
      }
      _isBufferingStroke = true;

      _strokeBuffer.Clear();
      _actualizedStrokeIdxBuffer.Clear();

      for (int i = 0; i < _strokeFilters.Count; i++) {
        _strokeFilters[i].Reset();
      }
      for (int i = 0; i < _strokeBufferRenderers.Count; i++) {
        _strokeBufferRenderers[i].InitializeRenderer();
      }
    }

    public void StartStroke() {
      if (!_isBufferingStroke) {
        BeginTrackingStroke();
      }

      if (_isActualizingStroke) {
        Debug.LogError("[StrokeProcessor] Stroke already actualizing; cannot begin actualizing stroke. Call StopActualizingStroke() first.");
        return;
      }
      _isActualizingStroke = true;
      _actualizedStrokeIdx = 0;
      _strokeOutput = new List<StrokePoint>(); // can't clear -- other objects have references to the old stroke output.
      _outputBufferEndOffset = 0;

      for (int i = 0; i < _strokeRenderers.Count; i++) {
        _strokeRenderers[i].InitializeRenderer();
      }
    }

    public void UpdateStroke(StrokePoint strokePoint) {
      UpdateStroke(strokePoint, true);
    }

    // shouldUpdateRenderers provides an optimization for updating multiple stroke points at once,
    // where it's more efficient to do the updating without rendering and then refreshing renderers
    // at the end.
    private void UpdateStroke(StrokePoint strokePoint, bool shouldUpdateRenderers = true) {
      _strokeBuffer.Add(strokePoint);
      _actualizedStrokeIdxBuffer.Add(-1);

      // Apply all filters in order on current stroke buffer.
      for (int i = 0; i < _strokeFilters.Count; i++) {
        _strokeFilters[i].Process(_strokeBuffer, _actualizedStrokeIdxBuffer);
      }

      if (_isActualizingStroke) {
        _actualizedStrokeIdxBuffer.SetLatest(_actualizedStrokeIdx++);

        // Output points from the buffer to the actualized stroke output.
        int offset = Mathf.Min(_outputBufferEndOffset, _strokeBuffer.Count - 1);
        for (int i = 0; i <= offset; i++) {
          int outputIdx = Mathf.Max(0, _outputBufferEndOffset - (_strokeBuffer.Count - 1)) + i;
          StrokePoint bufferStrokePoint = _strokeBuffer.Get(_strokeBuffer.Count - 1 - (Mathf.Min(_strokeBuffer.Count - 1, _outputBufferEndOffset) - i));
          if (outputIdx > _strokeOutput.Count - 1) {
            _strokeOutput.Add(bufferStrokePoint);
          }
          else {
            _strokeOutput[outputIdx] = bufferStrokePoint;
          }
        }
        _outputBufferEndOffset += 1;

        // Refresh stroke renderers.
        if (shouldUpdateRenderers) {
          UpdateStrokeRenderers();
        }
      }

      // Refresh stroke preview renderers.
      if (shouldUpdateRenderers) {
        UpdateStrokeBufferRenderers();
      }
    }

    public void UpdateStroke(List<StrokePoint> strokePoints) {
      // UpdateStroke without updating renderers.
      for (int i = 0; i < strokePoints.Count; i++) {
        UpdateStroke(strokePoints[i], false);
      }

      // Manually update renderers.
      if (strokePoints.Count > 0) {
        if (_isActualizingStroke) {
          UpdateStrokeRenderers();
        }
        UpdateStrokeBufferRenderers();
      }
    }

    private void UpdateStrokeRenderers() {
      for (int i = 0; i < _strokeRenderers.Count; i++) {
        _strokeRenderers[i].UpdateRenderer(_strokeOutput, _maxMemory);
      }
    }

    private void UpdateStrokeBufferRenderers() {
      for (int i = 0; i < _strokeBufferRenderers.Count; i++) {
        _strokeBufferRenderers[i].RefreshRenderer(_strokeBuffer);
      }
    }

    public void StopActualizingStroke() {
      if (!_isActualizingStroke) {
        Debug.LogError("[StrokeProcessor] Can't stop actualizing stroke; stroke never began actualizing in the first place.");
        Debug.Break();
      }
      _isActualizingStroke = false;

      for (int i = 0; i < _strokeRenderers.Count; i++) {
        _strokeRenderers[i].FinalizeRenderer();
      }
    }

    public void EndStroke() {
      if (_isActualizingStroke) {
        StopActualizingStroke();
      }

      _isBufferingStroke = false;

      for (int i = 0; i < _strokeBufferRenderers.Count; i++) {
        _strokeBufferRenderers[i].StopRenderer();
      }
    }

  }


}