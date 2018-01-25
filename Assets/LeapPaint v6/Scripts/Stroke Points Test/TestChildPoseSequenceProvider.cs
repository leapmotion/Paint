using Leap.Unity.Query;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  [ExecuteInEditMode]
  public class TestChildPoseSequenceProvider : MonoBehaviour, IIndexable<Pose> {

    public string nameMustContain = "";

    private List<Pose> poses = new List<Pose>();

    private void Update() {
      var transforms = Pool<List<Transform>>.Spawn();
      transforms.Clear();
      poses.Clear();
      try {
        this.GetComponentsInChildren<Transform>(transforms);

        foreach (var transform in transforms
                                    .Query()
                                    .Where(t => string.IsNullOrEmpty(nameMustContain)
                                           || t.name.Contains(nameMustContain))) {
          poses.Add(transform.ToWorldPose());
        }
      }
      finally {
        transforms.Clear();
        Pool<List<Transform>>.Recycle(transforms);
      }
    }

    public Pose this[int idx] {
      get {
        return poses[idx];
      }
    }

    public int Count { get { return poses.Count; } }

  }

}
