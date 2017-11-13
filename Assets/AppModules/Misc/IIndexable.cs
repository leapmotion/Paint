using System;
using Leap.Unity.Query;

namespace Leap.Unity {

  public interface IIndexable<T> {

    T this[int idx] { get; }

    int Count { get; }

  }

  public static class IIndexableExtensions {

    public static IIndexableEnumerator<T> GetEnumerator<T>(this IIndexable<T> indexable) {
      return new IIndexableEnumerator<T>(indexable);
    }

    public static QueryWrapper<T, IQueryOp<T>> Query<T>(this IIndexable<T> indexable) {
      return new QueryWrapper<T, IQueryOp<T>>(indexable.GetEnumerator());
    }

  }

  public struct IIndexableEnumerator<T> : IQueryOp<T> {

    IIndexable<T> indexable;
    int index;

    public IIndexableEnumerator(IIndexable<T> indexable) {
      this.indexable = indexable;
      index = -1;
    }

    public IIndexableEnumerator<T> GetEnumerator() { return this; }

    public bool MoveNext() { index++; return index < indexable.Count; }

    public bool TryGetNext(out T t) {
      if (MoveNext()) {
        t = Current;
        return true;
      }
      else {
        t = default(T);
        return false;
      }
    }

    public void Reset() { index = -1; }

    public T Current { get { return indexable[index]; } }

  }

}