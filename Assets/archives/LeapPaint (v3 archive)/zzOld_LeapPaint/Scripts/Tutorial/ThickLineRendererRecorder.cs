using System;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Recording;
using Leap.Unity.LeapPaint_v3;

[RequireComponent(typeof(ThickRibbonRenderer))]
public class ThickLineRendererRecorder : BasicMethodRecording<ThickLineRendererRecorder.Args> {
  private ThickRibbonRenderer _renderer;

  protected override void Awake() {
    base.Awake();
    _renderer = GetComponent<ThickRibbonRenderer>();
  }

  public override void EnterRecordingMode() {
    _renderer.OnInitializeRenderer += () => SaveArgs(new Args() {
      method = Method.InitializeRenderer
    });

    _renderer.OnUpdateRenderer += (stroke, max) => SaveArgs(new Args() {
      method = Method.UpdateRenderer,
      stroke = stroke,
      maxChangedFromEnd = max
    });

    _renderer.OnFinalizeRenderer += () => SaveArgs(new Args() {
      method = Method.FinalizeRenderer
    });
  }

  protected override void InvokeArgs(Args args) {
    switch (args.method) {
      case Method.InitializeRenderer:
        _renderer.InitializeRenderer();
        break;
      case Method.UpdateRenderer:
        _renderer.UpdateRenderer(args.stroke, args.maxChangedFromEnd);
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
