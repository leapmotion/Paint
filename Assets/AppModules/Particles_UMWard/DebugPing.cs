using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class DebugPing : MonoBehaviour, IRuntimeGizmoComponent {

    public const string PING_OBJECT_NAME = "__Debug Ping Runner__";

    public const float DEFAULT_PING_RADIUS = 0.15f;

    public const float PING_DURATION = 0.25f;

    public static AnimationCurve pingRadiusCurve = DefaultCurve.SigmoidUp;

    public struct PingState {
      public Vector3 position;
      public float sizeMultiplier;
      public float time;
      public Color color;
    }

    public static void Ping(Vector3 worldPosition) {
      Ping(worldPosition, Color.white, 1f);
    }

    public static void Ping(Vector3 worldPosition, Color color) {
      Ping(worldPosition, color, 1f);
    }

    public static void Ping(Vector3 worldPosition,
                            Color color,
                            float sizeMultiplier) {
      ensurePingRunnerExists();

      s_instance.AddPing(new PingState() {
        position = worldPosition,
        sizeMultiplier = sizeMultiplier,
        time = 0f,
        color = color
      });
    }

    private static DebugPing s_instance = null;

    private static void ensurePingRunnerExists() {
      if (s_instance == null) {
        s_instance = new GameObject(PING_OBJECT_NAME).AddComponent<DebugPing>();

      }
    }

    private List<PingState> _activePings = new List<PingState>();

    public void AddPing(PingState ping) {
      _activePings.Add(ping);
    }

    void Update() {
      var indicesToRemove = Pool<List<int>>.Spawn();
      try {
        for (int i = 0; i < _activePings.Count; i++) {
          var curPing = _activePings[i];

          curPing.time += Time.deltaTime;

          if (curPing.time > 1f) {
            indicesToRemove.Add(i);
          }

          _activePings[i] = curPing;
        }

        for (int i = indicesToRemove.Count - 1; i >= 0; i--) {
          _activePings.RemoveAt(i);
        }
      }
      finally {
        indicesToRemove.Clear();
        Pool<List<int>>.Recycle(indicesToRemove);
      }
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      foreach (var ping in _activePings) {
        drawer.color = ping.color;
        drawer.DrawWireSphere(ping.position,
          ping.sizeMultiplier * DEFAULT_PING_RADIUS
          * Mathf.Lerp(0f, 1f, ping.time / PING_DURATION));
      }
    }

}

}