using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Drawing {

  public class StrokeObject : MonoBehaviour, IIndexable<StrokePoint> {

    [SerializeField]
    private List<StrokePoint> _data;

    /// <summary>
    /// Fired when this Stroke Object is modified.
    /// </summary>
    public event Action OnModified = () => { };

    /// <summary>
    /// Fired when this Stroke Object is modified; passes itself as an argument.
    /// </summary>
    public event Action<StrokeObject> OnStrokeModified = (stroke) => { };

    // TODO: I don't think this is necessary actually, the LivePolyMesh can probably
    // do this.
    //public Maybe<StrokePoint> previousStrokePointHint;
    //public Maybe<StrokePoint> nextStrokePointHint;

    void Awake() {
      _data = Pool<List<StrokePoint>>.Spawn();
      _data.Clear();
    }

    void OnDestroy() {
      _data.Clear();
      Pool<List<StrokePoint>>.Recycle(_data);
    }

    public StrokePoint this[int idx] {
      get { return _data[idx]; }
    }

    public int Count {
      get { return _data.Count; }
    }

    public void Add(StrokePoint strokePoint) {
      _data.Add(strokePoint);

      OnModified();
      OnStrokeModified(this);
    }

  }

}
