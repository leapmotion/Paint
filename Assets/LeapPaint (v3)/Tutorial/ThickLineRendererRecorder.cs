using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Recording;
using Leap.Unity.LeapPaint_v3;

[RequireComponent(typeof(ThickRibbonRenderer))]
public class ThickLineRendererRecorder : BasicMethodRecording<ThickLineRendererArgs, ThickLineRendererRecorder.Args> {
  private ThickRibbonRenderer _renderer;

  public float clipTime = 0;
  public float playbackOffset;

  protected override void Awake() {
    base.Awake();
    _renderer = GetComponent<ThickRibbonRenderer>();
  }

  [ContextMenu("Clip Start")]
  public void ClipStart() {
    if (data.args.Count(a => a.method == Method.InitializeRenderer) != 1) {
      Debug.LogWarning("Cannot clip because there are more than one stroke.");
      return;
    }

    int clipToIndex = data.times.Where(t => t < clipTime).Count();

    int strokePointsToRemove = data.args[clipToIndex - 1].stroke.Count;

    data.times = data.times.Skip(clipToIndex).ToList();
    data.args = data.args.Skip(clipToIndex).ToList();

    foreach (var arg in data.args) {
      if (arg.method == Method.UpdateRenderer) {
        arg.stroke.RemoveRange(0, strokePointsToRemove);
      }
    }

    data.args.Insert(0, new Args() {
      method = Method.InitializeRenderer
    });

    data.times.Insert(0, data.times[0] - 0.001f);
  }

  public override void EnterRecordingMode() {
    base.EnterRecordingMode();

    _renderer.OnInitializeRenderer += () => SaveArgs(new Args() {
      method = Method.InitializeRenderer
    });

    _renderer.OnUpdateRenderer += (stroke, max) => SaveArgs(new Args() {
      method = Method.UpdateRenderer,
      stroke = new List<StrokePoint>(stroke),
      maxChangedFromEnd = max
    });

    _renderer.OnFinalizeRenderer += () => SaveArgs(new Args() {
      method = Method.FinalizeRenderer
    });
  }

  private List<StrokePoint> _strokeList = new List<StrokePoint>();

  protected override void InvokeArgs(Args args) {
    switch (args.method) {
      case Method.InitializeRenderer:
        _renderer.InitializeRenderer();
        break;
      case Method.UpdateRenderer:
        _strokeList.Clear();
        _strokeList.AddRange(args.stroke);
        for(int i=0; i<_strokeList.Count; i++) {
          var item = _strokeList[i];
          item.position = item.position + Vector3.up * playbackOffset;
          _strokeList[i] = item;
        }

        _renderer.UpdateRenderer(_strokeList, args.maxChangedFromEnd);
        break;
      case Method.FinalizeRenderer:
        _renderer.FinalizeRenderer();
        break;
    }
  }

  [Serializable]
  public struct Args {
    public Method method;
    public List<StrokePoint> stroke;
    public int maxChangedFromEnd;
  }

  public enum Method {
    InitializeRenderer,
    UpdateRenderer,
    FinalizeRenderer
  }
}
