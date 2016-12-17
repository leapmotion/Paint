using UnityEngine;
using System.Collections.Generic;
using StrokeProcessing;

public interface IStrokeRenderer {

  void InitializeRenderer();
  void UpdateRenderer(List<StrokePoint> filteredStroke, int maxChangedFromEnd);
  void FinalizeRenderer();

}