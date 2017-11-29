using Leap.Unity.RuntimeGizmos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class DebugPing : MonoBehaviour, IRuntimeGizmoComponent {

    public const string PING_OBJECT_NAME = "__Debug Ping Runner__";

    public const float DEFAULT_PING_RADIUS = 0.10f;

    public const float PING_DURATION = 0.25f;

    public static AnimationCurve pingAnimCurve = DefaultCurve.SigmoidUp;

    public enum ShapeType {
      Sphere,
      Capsule,
      Cone
    }

    public enum AnimType {
      Expand,
      Fade,
      ExpandAndFade
    }

    public struct PingState {
      public Vector3 position0, position1;
      public Func<Vector3> position0Func, position1Func;
      public Quaternion rotation0;
      public float sizeMultiplier;
      public float time;
      public Color color;
      public ShapeType shapeType;
      public AnimType animType;
    }

    private static void Ping(Vector3 worldPosition0,
                             Vector3 worldPosition1 = default(Vector3),
                             Func<Vector3> worldPosition0Func = null,
                             Func<Vector3> worldPosition1Func = null,
                             Quaternion rotation0 = default(Quaternion),
                             float sizeMultiplier = 1f,
                             Color color = default(Color),
                             AnimType animType = default(AnimType),
                             ShapeType shapeType = default(ShapeType)) {
      ensurePingRunnerExists();

      rotation0 = rotation0.ToNormalized();

      s_instance.AddPing(new PingState() {
        position0 = worldPosition0,
        position1 = worldPosition1,
        position0Func = worldPosition0Func,
        position1Func = worldPosition1Func,
        rotation0 = rotation0,
        sizeMultiplier = sizeMultiplier,
        time = 0f,
        color = color,
        animType = animType,
        shapeType = shapeType
      });
    }

    #region Static Ping API

    public static void Ping(Pose worldPose) {
      Ping(worldPose, Color.white, 1f);
    }

    public static void Ping(Vector3 worldPosition) {
      Ping(worldPosition, Color.white, 1f);
    }

    public static void Ping(Vector3 worldPosition, Color color) {
      Ping(worldPosition, color, 1f);
    }

    public static void Ping(Pose worldPose, Color color) {
      Ping(worldPose, color, 1f);
    }

    public static void Ping(Func<Vector3> worldPositionFunc,
                            Color color,
                            float sizeMultiplier,
                            AnimType animType = AnimType.Expand) {
      Ping(
        Vector3.zero,
        worldPosition0Func: worldPositionFunc,
        color: color,
        sizeMultiplier: sizeMultiplier,
        animType: animType
      );
    }

    public static void Ping(Vector3 worldPosition,
                            Color color,
                            float sizeMultiplier,
                            AnimType animType = AnimType.Expand) {
      Ping(
        worldPosition,
        Vector3.zero,
        color:          color,
        sizeMultiplier: sizeMultiplier,
        animType:       animType
      );
    }

    public static void Ping(Pose worldPose,
                            Color color,
                            float sizeMultiplier,
                            AnimType animType = AnimType.Expand) {
      Ping(
        worldPose.position,
        default(Vector3),
        null, null,
        worldPose.rotation,
        color: color,
        sizeMultiplier: sizeMultiplier,
        animType: animType
      );
    }

    public static void PingCapsule(Func<Vector3> worldPosition0Func,
                                   Func<Vector3> worldPosition1Func,
                                   Color color,
                                   float sizeMultiplier,
                                   AnimType animType = AnimType.Expand) {
      Ping(
        Vector3.zero,
        worldPosition0Func: worldPosition0Func,
        worldPosition1Func: worldPosition1Func,
        color: color,
        sizeMultiplier: sizeMultiplier,
        shapeType: ShapeType.Capsule,
        animType: animType
      );
    }

    public static void PingCapsule(Vector3 worldPosition0,
                                   Vector3 worldPosition1,
                                   Color color,
                                   float sizeMultiplier,
                                   AnimType animType = AnimType.Expand) {
      Ping(
        worldPosition0, worldPosition1,
        color: color,
        sizeMultiplier: sizeMultiplier,
        shapeType: ShapeType.Capsule,
        animType: animType
      );
    }

    public static void PingCone(Vector3 worldPosition0,
                                Vector3 worldPosition1,
                                Color color,
                                float sizeMultiplier,
                                AnimType animType = AnimType.Expand) {
      Ping(
        worldPosition0,
        worldPosition1,
        color: color,
        sizeMultiplier: sizeMultiplier,
        animType: animType,
        shapeType: ShapeType.Cone
      );
    }

    public static void PingCone(Func<Vector3> worldPosition0Func,
                                Func<Vector3> worldPosition1Func,
                                Color color,
                                float sizeMultiplier,
                                AnimType animType = AnimType.Expand) {
      Ping(
        Vector3.zero,
        worldPosition0Func: worldPosition0Func,
        worldPosition1Func: worldPosition1Func,
        color: color,
        sizeMultiplier: sizeMultiplier,
        animType: animType,
        shapeType: ShapeType.Cone
      );
    }

    #endregion

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
      Color pingColor;
      float pingSize;
      float animTime;
      Vector3 pingPos0, pingPos1;
      Quaternion pingRot0;
      foreach (var ping in _activePings) {

        if (ping.position0Func != null) {
          pingPos0 = ping.position0Func();
        }
        else {
          pingPos0 = ping.position0;
        }
        
        if (ping.position1Func != null) {
          pingPos1 = ping.position1Func();
        }
        else {
          pingPos1 = ping.position1;
        }

        pingRot0 = ping.rotation0;

        pingColor = ping.color;

        animTime = Mathf.Lerp(0f, 1f, ping.time / PING_DURATION);
        animTime = pingAnimCurve.Evaluate(animTime);

        pingSize = ping.sizeMultiplier * DEFAULT_PING_RADIUS;

        switch (ping.animType) {
          case AnimType.Expand:
            pingSize = pingSize * animTime;
            break;
          case AnimType.Fade:
            pingColor = ping.color.WithAlpha(1f - animTime);
            break;
          case AnimType.ExpandAndFade:
            pingSize = pingSize * animTime;
            pingColor = ping.color.WithAlpha(1f - animTime);
            break;
        }
        

        drawer.color = pingColor;

        switch (ping.shapeType) {
          case ShapeType.Sphere:
            drawer.DrawWireSphere(pingPos0, pingRot0, pingSize);
            break;
          case ShapeType.Capsule:
            drawer.DrawWireCapsule(pingPos0, pingPos1, pingSize);
            break;
          case ShapeType.Cone:
            drawer.DrawCone(pingPos0, pingPos1, pingSize);
            break;
        }

      }
    }

  }

  public static class RuntimeGizmoDrawerExtensions {

    public static void DrawWireSphere(this RuntimeGizmoDrawer drawer,
                                      Vector3 position,
                                      Quaternion rotation,
                                      float radius) {
      drawer.PushMatrix();

      drawer.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);

      drawer.DrawWireSphere(Vector3.zero, radius);

      drawer.PopMatrix();
    }

    public static void DrawCone(this RuntimeGizmoDrawer drawer,
                                Vector3 pos0, Vector3 pos1,
                                float radius,
                                int resolution = 24) {
      var dir = pos1 - pos0;
      var R = dir.Perpendicular().normalized * radius;
      Quaternion rot = Quaternion.AngleAxis(360f / 24, dir);
      for (int i = 0; i < resolution; i++) {
        drawer.DrawLine(pos0 + R, pos1);
        var nextR = rot * R;
        drawer.DrawLine(pos0 + R, pos0 + nextR);
        R = nextR;
      }
    }

  }

}