using UnityEngine;
using System.Collections.Generic;
using System;

public class StrokeProcessor {

  #region Stroke State

  // Stroke processing configuration
  private List<IMemoryFilter<StrokePoint>> _strokeFilters = null;
  private int _maxMemory = 0;

  // Stroke state
  private bool _isBufferingStroke = false;
  private bool _isActualizingStroke = false;
  private RingBuffer<StrokePoint> _strokeBuffer;
  private RingBuffer<int> _strokeIdxBuffer;
  private int curStrokeIdx = 0;
  private List<StrokePoint> _strokeOutput = null;

  // Stroke renderers
  private List<IStrokeRenderer> _strokeRenderers = null;

  // Stroke buffer renderers
  private List<IStrokeBufferRenderer> _strokeBufferRenderers = null;

  public StrokeProcessor() {
    _strokeFilters = new List<IMemoryFilter<StrokePoint>>();
    _strokeRenderers = new List<IStrokeRenderer>();
    _strokeBufferRenderers = new List<IStrokeBufferRenderer>();
    _strokeOutput = new List<StrokePoint>();
  }

  public void RegisterStrokeFilter(IMemoryFilter<StrokePoint> strokeFilter) {
    _strokeFilters.Add(strokeFilter);

    int filterMemorySize = strokeFilter.GetMemorySize();
    if (filterMemorySize + 1 > _maxMemory) {
      _maxMemory = filterMemorySize + 1;
    }

    if (_isBufferingStroke) {
      Debug.LogWarning("[StrokeProcessor] Registering stroke filters destroys the current stroke processing queue.");
    }
    _strokeBuffer = new RingBuffer<StrokePoint>(_maxMemory);
    _strokeIdxBuffer = new RingBuffer<int>(_maxMemory);
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

  public void BeginStroke() {
    if (_isBufferingStroke) {
      Debug.LogError("[StrokeProcessor] Stroke in progress; cannot begin new stroke. Call EndStroke() to finalize the current stroke first.");
      return;
    }
    _isBufferingStroke = true;

    _strokeBuffer.Clear();
    _strokeIdxBuffer.Clear();
    curStrokeIdx = 0;

    for (int i = 0; i < _strokeFilters.Count; i++) {
      _strokeFilters[i].Reset();
    }
    for (int i = 0; i < _strokeBufferRenderers.Count; i++) {
      _strokeBufferRenderers[i].InitializeRenderer();
    }
    for (int i = 0; i < _strokeRenderers.Count; i++) {
      _strokeRenderers[i].InitializeRenderer();
    }
  }

  public void StartActualizingStroke() {
    //if (!_strokeInProgress) {
    //  BeginStroke();
    //}

    //if (_shouldActualizeStroke) {
    //  Debug.LogError("[StrokeProcessor] Stroke already actualizing; cannot begin actualizing stroke. Call StopActualizingStroke() first.");
    //  return;
    //}
    //_shouldActualizeStroke = true;
    //_strokeOutput = new List<StrokePoint>(); // can't clear -- other objects have references to the old stroke output.
  }

  public void UpdateStroke(StrokePoint strokePoint) {
    _strokeOutput.Add(strokePoint);
    _strokeBuffer.Add(strokePoint);
    _strokeIdxBuffer.Add(curStrokeIdx++);

    // Apply all filters in order on current stroke buffer.
    for (int i = 0; i < _strokeFilters.Count; i++) {
      _strokeFilters[i].Process(_strokeBuffer, _strokeIdxBuffer);
    }

    if (_isActualizingStroke) {
      // Update latest points in stroke output with latest points from the filtered buffer.
      int bufferIdx = 0;
      for (int i = _strokeOutput.Count - _strokeBuffer.Size; i < _strokeOutput.Count; i++) {
        _strokeOutput[i] = _strokeBuffer.Get(bufferIdx++);
      }

      // Refresh stroke renderers.
      for (int i = 0; i < _strokeRenderers.Count; i++) {
        _strokeRenderers[i].RefreshRenderer(_strokeOutput, _maxMemory);
      }
    }

    // Refresh stroke preview renderers.
    for (int i = 0; i < _strokeBufferRenderers.Count; i++) {
      _strokeBufferRenderers[i].RefreshRenderer(_strokeBuffer);
    }
  }

  public void StopActualizingStroke() {
    //_shouldActualizeStroke = false;
  }

  public void EndStroke() {
    if (_isActualizingStroke) {
      StopActualizingStroke();
    }

    _isBufferingStroke = false;

    for (int i = 0; i < _strokeBufferRenderers.Count; i++) {
      _strokeBufferRenderers[i].StopRenderer();
    }
    for (int i = 0; i < _strokeRenderers.Count; i++) {
      _strokeRenderers[i].FinalizeRenderer();
    }
  }

  #endregion

}
