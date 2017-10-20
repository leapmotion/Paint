

namespace Leap.Unity {

  public interface IIndexable<T> {

    T this[int idx] { get; }

    int Count { get; }

  }

  public struct IIndexableEnumerator<T> {

    IIndexable<T> indexable;
    int index;

    public IIndexableEnumerator(IIndexable<T> indexable) {
      this.indexable = indexable;
      index = 0;
    }

    public bool MoveNext() { index++; return index < indexable.Count - 1; }
    public T Current { get { return indexable[index]; } }

  }

}