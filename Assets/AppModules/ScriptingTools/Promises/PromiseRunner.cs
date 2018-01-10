using UnityEngine;

namespace Leap.Unity.Promises {

  public class PromiseRunner : MonoBehaviour {

    #region Constants

    public const int UNSCHEDULED_PROMISE_ID = 0;

    public const string PROMISE_RUNNER_OBJECT_NAME = "__Promise Runner__";

    #endregion

    #region Promise Wrapper Class

    private class PromiseWrapper {

      public int promiseId { get; set; }
      public object promiseFuncStore { get; set; }

      public void Clear() {
        promiseId = UNSCHEDULED_PROMISE_ID;
        promiseFuncStore = null;
      }

    }

    #endregion

    #region Memory & Events

    private static ProduceConsumeBuffer<PromiseWrapper> _pendingPromises
       = new ProduceConsumeBuffer<PromiseWrapper>(256);

    public static PromiseRunner instance = null;

    [RuntimeInitializeOnLoadMethod]
    private static void RuntimeInitializeOnLoad() {
      var promiseRunnerObj = new GameObject(PROMISE_RUNNER_OBJECT_NAME);
      instance = promiseRunnerObj.AddComponent<PromiseRunner>();
    }

    private void Update() {
      staticUpdate();
    }

    private static void staticUpdate() {
      _pendingPromises.
    }

    #endregion

    #region Static API

    public static void SchedulePromise<T>(Promise<T> promise) {
      var promiseWrapper = Pool<PromiseWrapper>.Spawn();
      promiseWrapper.Clear();

      promiseWrapper.promiseId = 1;
      promiseWrapper.promiseFuncStore = promise.fulfillFunc as object;

      if (!_pendingPromises.TryEnqueue(ref promiseWrapper)) {
        throw new System.InvalidOperationException(
          "Couldn't schedule new promise for a " + typeof(T).ToString()
        + "; ProduceConsumeBuffer failed to enqueue the promise wrapper.");
      }
    }

    public static T FulfillNow<T>(Promise<T> promise) {
      return promise.fulfillFunc();
    }

    public static T Fulfilled<T>(Promise<T> promise) {
      return promise.fulfillFunc();
    }

    #endregion

  }

}
