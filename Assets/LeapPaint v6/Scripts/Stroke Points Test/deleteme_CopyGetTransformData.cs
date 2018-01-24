using Leap.Unity.Attributes;
using Leap.Unity.Query;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class deleteme_CopyGetTransformData : MonoBehaviour {

    [QuickButton("Go!", "Go")]
    public bool unused;

    public GameObject thePrefab;

    private void Go() {
      var children = this.gameObject.transform.GetSelfAndAllChildren()
                                    .Query()
                                    .Where(t => t.name.Contains("Spline Pose Point"))
                                    .ToList();

      foreach (var t in children) {
        var pose = t.ToWorldPose();
        var numberStr = t.name.Substring(t.name.IndexOf("(") + 1,
                                         t.name.IndexOf(")") - (t.name.IndexOf("(") + 1));
        var number = int.Parse(numberStr);
        
        var clone = UnityEditor.PrefabUtility.InstantiatePrefab(thePrefab as GameObject) as GameObject;
        clone.transform.parent = t.transform.parent;

        DestroyImmediate(t.gameObject);

        clone.transform.SetWorldPose(pose);
        clone.gameObject.name = "Test Spline Pose Point (" + number + ")";
      }
    }

  }

}
