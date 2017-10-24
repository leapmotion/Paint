

namespace Leap.Unity.Meshing {

  public struct Triangle {
    public int a, b, c;

    public IndexEnumerator GetEnumerator() { return new IndexEnumerator(this); }

    public struct IndexEnumerator {
      private int _curIdx;
      private Triangle _tri;
      public IndexEnumerator(Triangle tri) {
        _tri = tri;
        _curIdx = -1;
      }
      public int Current {
        get {
          if (_curIdx == 0) return _tri.a;
          if (_curIdx == 1) return _tri.b;
          return _tri.c;
        }
      }
      public bool MoveNext() {
        _curIdx += 1;
        return _curIdx < 3;
      }
    }
  }

}