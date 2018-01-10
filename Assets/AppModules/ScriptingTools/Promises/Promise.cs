using UnityEngine;
using System.Collections;
using System;

namespace Leap.Unity.Promises {

  //public enum ReturnThreadType {
  //  UnityThread,
  //  // UnityRenderThread, // not yet implemented
  //  NonUnityThread
  //}

  public struct Promise<T> {

    public Promise(Func<T> tReturningFunc) {
      _returnType = typeof(T);
      _promiseId = Scheduler.UNSCHEDULED_PROMISE_ID;
      _isReady = false;
      _fulfillFunc = tReturningFunc;
    }

    private Type _returnType;
    public Type returnType { get { return _returnType; } }

    private int _promiseId;
    public int promiseId { get { return _promiseId; } }

    private bool _isReady;
    public bool isReady { get { return _isReady; } }

    private Func<T> _fulfillFunc;
    public Func<T> fulfillFunc { get { return _fulfillFunc; } }

    public T Fulfill() {
      if (!isReady) {
        return Scheduler.FulfillNow<T>(this);
      }
      else {
        return Scheduler.Fulfilled<T>(this);
      }
    }

  }

}

