using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using Leap.Unity.LiveUI;

namespace Leap.Unity.Promises {

  public class Promise : PromiseBase {

    private static PromiseUnderConstruction spawnPUC() {
      var promiseUnderConstruction = Pool<PromiseUnderConstruction>.Spawn();
      promiseUnderConstruction.Clear();

      return promiseUnderConstruction;
    }

    public static PromiseUnderConstruction To(Action somethingCallable) {
      var puc = spawnPUC();

      puc._promiseMethodInfo = somethingCallable.Method;

      return puc;
    }
    public static PromiseUnderConstruction To<R>(Func<R> somethingCallable) {
      var puc = spawnPUC();

      puc._promiseMethodInfo = somethingCallable.Method;

      return puc;
    }

    internal object To(Func<Browser> constructBrowser) {
      throw new NotImplementedException();
    }

    public static PromiseUnderConstruction To<A1, R>(Func<A1, R> somethingCallable) {
      var puc = spawnPUC();

      puc._promiseMethodInfo = somethingCallable.Method;

      return puc;
    }

    public class PromiseUnderConstruction : Promise<T> {

      public PromiseUnderConstruction() { }

      public PromiseUnderConstruction WithArgs(params object[] args) {
        _promiseArgs = args;

        return this;
      }

      public PromiseUnderConstruction OnThread(ThreadType threadType) {
        _promiseThreadType = threadType;

        return this;
      }

      public PromiseUnderConstruction Otherwise(Action<Exception> exceptionReceiver) {
        _exceptionReceiver = exceptionReceiver;

        return this;
      }

      //public static implicit operator Promise(PromiseUnderConstruction promiseUnderConstruction) {
      //  var promise = Pool<Promise>.Spawn();

      //  promise._promiseArgs = promiseUnderConstruction._promiseArgs;
      //  promise._promiseThreadType = promiseUnderConstruction._promiseThreadType;
      //  promise._exceptionReceiver = promiseUnderConstruction._exceptionReceiver;

      //  promiseUnderConstruction.Clear();
      //  Pool<PromiseUnderConstruction>.Recycle(promiseUnderConstruction);

      //  return promise;
      //}

      public void Clear() {
        _promiseArgs = null;
        _promiseThreadType = default(ThreadType);
        _exceptionReceiver = null;
        _promiseMethodInfo = null;
      }

    }

  }

  public abstract class PromiseBase {

    protected MethodInfo _promiseMethodInfo;

    protected object[] _promiseArgs;
    protected ThreadType _promiseThreadType;
    protected Action<Exception> _exceptionReceiver;

  }


  public enum ThreadType {
    UnityThread,
    // UnityRenderThread, // not yet implemented
    NonUnityThread
  }

  //public class Promise<T> {

  //  private Promise() { }

  //  public static Promise<T> New(Func<T> tReturningFunc,
  //                               ThreadType workThreadType = ThreadType.UnityThread) {
  //    var promise = new Promise<T>();

  //    promise._returnType = typeof(T);
  //    promise._promiseId = PromiseRunner.UNSCHEDULED_PROMISE_ID;
  //    promise._fulfillFunc = tReturningFunc;
  //    promise._workThreadType = workThreadType;

  //    return promise;
  //  }

  //  private Type _returnType;
  //  public Type resultType { get { return _returnType; } }

  //  private int _promiseId;
  //  public int promiseId { get { return _promiseId; } }

  //  public bool isReady { get { return PromiseRunner.IsResultReady(promiseId); } }

  //  private Func<T> _fulfillFunc;
  //  public Func<T> fulfillFunc { get { return _fulfillFunc; } }

  //  private ThreadType _workThreadType;
  //  public ThreadType workThreadType { get { return _workThreadType; } }

  //  public T Fulfill() {
  //    if (!isReady) {
  //      throw new System.InvalidOperationException(
  //        "Cannot fulfill promise for " + resultType.ToString() + " yet; the result isn't "
  //      + "ready.");
  //    }
  //    else {
  //      return PromiseRunner.GetFulfilled<T>(this);
  //    }
  //  }

  //  public void NotifyPromiseId(int promiseId) {
  //    this._promiseId = promiseId;
  //  }

  //}

}

