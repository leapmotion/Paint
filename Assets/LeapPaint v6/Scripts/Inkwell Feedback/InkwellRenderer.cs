using Leap.Unity.Attributes;
using Leap.Unity.Query;
using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class InkwellRenderer : MonoBehaviour, IRuntimeGizmoComponent {
    
    public bool autoDetectPinchAmount = true;

    [DisableIf("autoDetectPinchAmount", isEqualTo: false)]
    public LeapProvider provider;
    [DisableIf("autoDetectPinchAmount", isEqualTo: false)]
    public Chirality whichHand;

    [DisableIf("autoDetectPinchAmount", isEqualTo: true)]
    public float pinchAmount = 0f;
    public void SetPinchAmount(float amount) { pinchAmount = amount; }

    List<Vector3> thumbPoints = new List<Vector3>(64);
    List<Vector3> indexPoints = new List<Vector3>(64);

    private void Reset() {
      if (provider == null) provider = Hands.Provider;
    }

    private void OnEnable() {
      provider.OnUpdateFrame -= onUpdateFrame;
      provider.OnUpdateFrame += onUpdateFrame;
    }

    private void OnDisable() {
      provider.OnUpdateFrame -= onUpdateFrame;
    }

    private void onUpdateFrame(Frame frame) {
      var hand = frame.Get(whichHand);

      thumbPoints.Clear();
      indexPoints.Clear();

      if (hand != null) {
        var index = hand.GetIndex(); var thumb = hand.GetThumb();

        //var indexLen = index.Length;
        // Exclude metacarpal for thumb length.
        //var thumbLen = thumb.bones[2].Length + thumb.bones[3].Length;
        //var indexLenOverThumbLen = indexLen / thumbLen;

        if (autoDetectPinchAmount) {
          var pinchStrength = Gestures.PinchGesture.GetCustomPinchStrength(hand).Clamped01();
          pinchAmount = pinchStrength;
        }

        var progress = pinchAmount.Map(0f, 1f, -0.2f, 1f);
        var positiveDir = -hand.PalmarAxis();
        for (float p = -0.2f; p <= progress; p += 0.01f) {
          Vector3 thumbPos, indexPos;

          // Thumb position evaluation
          var progressAlongThumb = p;
          if (progressAlongThumb < 0f) {
            var thumbMC = thumb.bones[1];
            thumbPos = Vector3.Lerp(thumbMC.PrevJoint.ToVector3(),
                                    thumbMC.NextJoint.ToVector3(),
                                    progressAlongThumb.Map(-0.5f, 0f, 0f, 1f));
          }
          else {
            Bone thumbBone;
            if (progressAlongThumb < 0.5f) {
              thumbBone = thumb.bones[2];
            }
            else {
              progressAlongThumb -= 0.5f;
              thumbBone = thumb.bones[3];
            }
            thumbPos = Vector3.Lerp(thumbBone.PrevJoint.ToVector3(),
                                    thumbBone.NextJoint.ToVector3(),
                                    progressAlongThumb.Map(0f, 0.5f, 0f, 1f));
          }

          // Non-thumb position evaluation
          float progressAlongFinger = p;
          Vector3 fingerPos;
          var finger = index;
          if (progressAlongFinger < 0f) {
            var fingerMC = finger.bones[0];
            fingerPos = Vector3.Lerp(fingerMC.PrevJoint.ToVector3(),
                                     fingerMC.NextJoint.ToVector3(),
                                     progressAlongFinger.Map(-0.5f, 0f, 0f, 1f));
          }
          else {
            Bone fingerBone;
            if (progressAlongFinger < 0.33f) {
              fingerBone = finger.bones[1];
            }
            else if (progressAlongFinger < 0.66f) {
              progressAlongFinger -= 0.33f;
              fingerBone = finger.bones[2];
            }
            else {
              progressAlongFinger -= 0.66f;
              fingerBone = finger.bones[3];
            }
            fingerPos = Vector3.Lerp(fingerBone.PrevJoint.ToVector3(),
                                      fingerBone.NextJoint.ToVector3(),
                                      progressAlongFinger.Map(0f, 0.33f, 0f, 1f));
          }

          indexPos = fingerPos;

          // Filter out any over-pinch.
          var isOverPinched = (indexPos - thumbPos).Dot(positiveDir) < 0f;
          if (!isOverPinched) {
            thumbPoints.Add(thumbPos);
            indexPoints.Add(indexPos);
          }

        }
      }
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      drawer.color = LeapColor.white;

      for (int i = 0; i < thumbPoints.Count; i++) {
        drawer.DrawLine(thumbPoints[i], indexPoints[i]);
      }
    }

  }

  public static class InkwellRendererExtensions {

    public static Hand Get(this LeapProvider provider, Chirality whichHand) {
      List<Hand> hands;
      if (Time.inFixedTimeStep) {
        hands = provider.CurrentFixedFrame.Hands;
      }
      else {
        hands = provider.CurrentFrame.Hands;
      }

      return hands.Query().FirstOrDefault(h => h.IsLeft == (whichHand == Chirality.Left));
    }

    public static Hand Get(this Frame frame, Chirality whichHand) {
      return frame.Hands.Query().FirstOrDefault(h => h.IsLeft == (whichHand == Chirality.Left));
    }

  }

}
