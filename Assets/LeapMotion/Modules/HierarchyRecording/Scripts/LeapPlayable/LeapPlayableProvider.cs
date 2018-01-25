/******************************************************************************
 * Copyright (C) Leap Motion, Inc. 2011-2017.                                 *
 * Leap Motion proprietary and  confidential.                                 *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Leap Motion and you, your company or other organization.           *
 ******************************************************************************/

using UnityEngine;
using Leap.Unity.RuntimeGizmos;

namespace Leap.Unity.Recording {

  public class LeapPlayableProvider : LeapProvider, IRuntimeGizmoComponent {

    private Frame _frame;

    public override Frame CurrentFixedFrame {
      get {
        return _frame;
      }
    }

    public override Frame CurrentFrame {
      get {
        return _frame;
      }
    }

    public void SetCurrentFrame(Frame frame) {
      _frame = frame;
      DispatchUpdateFrameEvent(frame);
      DispatchFixedFrameEvent(frame);
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      if (_frame != null && !Application.isPlaying) {
        foreach (var hand in _frame.Hands) {
          drawer.DrawWireSphere(hand.PalmPosition.ToVector3(), 0.01f);
          foreach (var finger in hand.Fingers) {
            drawer.DrawWireSphere(finger.TipPosition.ToVector3(), 0.01f);
          }
        }
      }
    }
  }
}
