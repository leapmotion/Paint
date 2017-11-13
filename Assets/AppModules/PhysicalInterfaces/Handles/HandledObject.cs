using Leap.Unity.Attributes;
using Leap.Unity.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class HandledObject : MovementObservingBehaviour, IHandle {

    #region Inspector

    [Header("Handles (each must be IHandle)")]
    //[ElementsImplementInterface(typeof(IHandle))]
    // TODO: Write a custom property drawer that renders ImplementsInterface fields
    // instead of plain Transform fields.
    [SerializeField, EditTimeOnly]
    private Transform[] _handles;
    public IIndexable<IHandle> handles {
      get {
        return new TransformArrayComponentWrapper<IHandle>(_handles);
      }
    }

    #endregion

    #region Unity Events

    private Dictionary<IHandle, Pose> _objToHandleDeltaPoses
      = new Dictionary<IHandle, Pose>();

    protected virtual void Start() {
      foreach (var handle in handles.GetEnumerator()) {
        _objToHandleDeltaPoses[handle] = handle.pose.From(this.pose);
      }
    }

    private IHandle _heldHandle = null;

    protected override void Update() {
      base.Update();

      if (_heldHandle != null && _heldHandle.wasReleased) {
        _heldHandle = null;
      }

      // Enforces only one handle is held at a time.
      // This isn't great, but needs to be true for now.
      {
        foreach (var handle in handles.GetEnumerator()) {
          if (handle.wasHeld && handle != _heldHandle) {
            if (_heldHandle != null) {
              _heldHandle.Release();
            }

            _heldHandle = handle;
          }
        }
      }

      if (_heldHandle != null) {
        // Handle movement -- easier when only one handle is held at any one
        // time.
        if (_heldHandle.wasMoved) {
          // Move this object based on the movement of the held handle.
          var objToHeldHandle =_objToHandleDeltaPoses[_heldHandle];
          this.Move(_heldHandle.pose * objToHeldHandle.inverse);

          // Move non-held handles to match the new pose of this object.
          var objPose = this.pose;
          foreach (var handle in handles.GetEnumerator()) {
            if (handle != _heldHandle) {
              handle.Move(objPose * _objToHandleDeltaPoses[handle]);
            }
          }
        }
      }

    }

    #endregion

    #region IHandle

    public override Pose pose {
      get { return this.transform.ToPose(); }
    }

    public bool isHeld {
      get {
        return handles.Query().Any(h => h.isHeld);
      }
    }

    public bool wasHeld {
      get {
        return handles.Query().Any(h => h.wasHeld);
      }
    }

    public bool wasMoved {
      get {
        return handles.Query().Any(h => h.wasMoved);
      }
    }

    public bool wasReleased {
      get {
        return handles.Query().Any(h => h.wasReleased);
      }
    }

    public bool wasThrown {
      get {
        return handles.Query().Any(h => h.wasReleased);
      }
    }

    public void Hold() {
      Debug.LogError("Can't hold a HandledObjct directy; instead, call Hold() on one "
                     + "of one of its Handles.");
    }

    public void Move(Pose newPose) {
      this.transform.SetWorldPose(newPose);
    }

    public void Release() {
      if (_heldHandle != null) {
        _heldHandle.Release();
      }
    }

    #endregion

  }

  public struct TransformArrayComponentWrapper<GetComponentType>
                : IIndexable<GetComponentType>
  {
    Transform[] _arr;

    public TransformArrayComponentWrapper(Transform[] arr) {
      _arr = arr;
    }

    public GetComponentType this[int idx] {
      get { return _arr[idx].GetComponent<GetComponentType>(); }
    }

    public int Count { get { return _arr.Length; } }
  }

}
