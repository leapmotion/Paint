using System;
using System.Collections.Generic;

namespace TweenInternal {

  public static class Pool<T> {
    private static Queue<T> _pool = new Queue<T>();
    private static Func<T> _factory;
    private static Action<T> _spawn, _return;

    public static void RegisterInstance(Func<T> factory, Action<T> spawnAction, Action<T> returnAction) {
      _factory = factory;
      _spawn = spawnAction;
      _return = returnAction;
    }

    public static T Spawn() {
      if (_factory == null) {
        throw new Exception("Cannot spawn an object that has not registered itself with the pool!");
      }

      T instance;
      if (_pool.Count == 0) {
        instance = _factory();
      } else {
        instance = _pool.Dequeue();
      }

      _spawn(instance);
      return instance;
    }

    public static void Return(T t) {
      if (_factory == null) {
        throw new Exception("Cannot return an object that has not registered itself with the pool!");
      }

      _return(t);
      _pool.Enqueue(t);
    }
  }

}