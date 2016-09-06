﻿using UnityEngine;
using System.Collections.Generic;
using System;

public class StrokeProcessor {

  #region Stroke State

  // Stroke processing configuration
  private List<IMemoryFilter<StrokePoint>> _strokeFilters = null;
  private int _maxMemory = 0;

  // Stroke state
  private bool _strokeInProgress = false;
  private RingBuffer<StrokePoint> _strokeBuffer;
  private List<StrokePoint> _strokeOutput = null;

  // Stroke renderers
  private List<IStrokeRenderer> _strokeRenderers = null;

  public StrokeProcessor() {
    _strokeFilters = new List<IMemoryFilter<StrokePoint>>();
    _strokeRenderers = new List<IStrokeRenderer>();
    _strokeOutput = new List<StrokePoint>();
  }

  public void RegisterStrokeFilter(IMemoryFilter<StrokePoint> strokeFilter) {
    _strokeFilters.Add(strokeFilter);

    int filterMemorySize = strokeFilter.GetMemorySize();
    if (filterMemorySize + 1 > _maxMemory) {
      _maxMemory = filterMemorySize + 1;
    }

    if (_strokeInProgress) {
      Debug.LogWarning("[StrokeProcessor] Registering stroke filters destroys the current stroke processing queue.");
    }
    _strokeBuffer = new RingBuffer<StrokePoint>(_maxMemory);
  }

  public void RegisterStrokeRenderer(IStrokeRenderer strokeRenderer) {
    _strokeRenderers.Add(strokeRenderer);
    if (_strokeInProgress) {
      Debug.LogWarning("[StrokeProcessor] Stroke in progress; Newly registered stroke renderers will not render the entire stroke if a stroke is already in progress.");
    }
  }

  public void BeginStroke() {
    if (_strokeInProgress) {
      Debug.LogError("[StrokeMeshGenerator] Stroke in progress; cannot begin new stroke. Call EndStroke() to finalize the current stroke first.");
      return;
    }
    _strokeInProgress = true;

    _strokeOutput.Clear();
    _strokeBuffer.Clear();

    for (int i = 0; i < _strokeFilters.Count; i++) {
      _strokeFilters[i].Reset();
    }
    for (int i = 0; i < _strokeRenderers.Count; i++) {
      _strokeRenderers[i].InitializeRenderer();
    }
  }

  public void UpdateStroke(StrokePoint strokePoint) {
    _strokeOutput.Add(strokePoint);
    _strokeBuffer.Add(strokePoint);

    // Apply all filters in order on current stroke buffer.
    for (int i = 0; i < _strokeFilters.Count; i++) {
      _strokeFilters[i].Process(_strokeBuffer);
    }

    // Update latest points in stroke output with latest points from the filtered buffer.
    int bufferIdx = 0;
    for (int i = _strokeOutput.Count - _strokeBuffer.Size; i < _strokeOutput.Count; i++) {
      _strokeOutput[i] = _strokeBuffer.Get(bufferIdx++);
    }

    // Refresh all renderers.
    for (int i = 0; i < _strokeRenderers.Count; i++) {
      _strokeRenderers[i].RefreshRenderer(_strokeOutput, _maxMemory);
    }
  }

  public void EndStroke() {
    _strokeInProgress = false;

    for (int i = 0; i < _strokeRenderers.Count; i++) {
      _strokeRenderers[i].FinalizeRenderer();
    }
  }

  #endregion

}
