using Leap.Unity.Query;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  [ExecuteInEditMode]
  public class TestChildPoseSequenceProvider : MonoBehaviour, IIndexable<Pose> {

    public string nameMustContain = "";

    private List<Pose> poses = new List<Pose>();

    public List<Transform> testTransforms = new List<Transform>();

    private void Update() {
      var transforms = Pool<List<Transform>>.Spawn();
      transforms.Clear();
      poses.Clear();
      testTransforms.Clear();
      try {
        this.GetComponentsInChildren<Transform>(transforms);

        foreach (var transform in transforms
                                    .Query()
                                    .Where(t => string.IsNullOrEmpty(nameMustContain)
                                           || t.name.Contains(nameMustContain))) {
          poses.Add(transform.ToWorldPose());

          testTransforms.Add(transform);
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
