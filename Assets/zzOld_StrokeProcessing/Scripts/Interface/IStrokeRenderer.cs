using UnityEngine;
using System.Collections.Generic;
using zzOldStrokeProcessing;

public interface IStrokeRenderer {

  void InitializeRenderer();
  void UpdateRenderer(List<StrokePoint> filteredStroke, int maxChangedFromEnd);
  void FinalizeRenderer();

}