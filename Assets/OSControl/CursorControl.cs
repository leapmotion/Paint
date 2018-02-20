using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Leap.Unity.OSControl {

  public static class CursorControl {

    [DllImport("User32.dll")]
    public static extern long SetCursorPos(int x, int y);

    public static void Test_MoveCursorToOrigin() {
      SetCursorPos(0, 0);
    }

    public static void SetPosition(Vector2Int position) {
      SetCursorPos(position.x, position.y);
    }

  }

}
