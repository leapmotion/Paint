using UnityEngine;
using System.Collections.Generic;

public interface IStrokeRenderer {

  void InitializeRenderer();
  void RefreshRenderer(List<StrokePoint> filteredStroke, int filterMaxMemoryWindowSize);
  void FinalizeRenderer();

}