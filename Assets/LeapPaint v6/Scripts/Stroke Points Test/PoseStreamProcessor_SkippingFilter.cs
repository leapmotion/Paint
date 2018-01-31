using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public class PoseStreamProcessor_SkippingFilter : MonoBehaviour,
                                                          IStreamReceiver<Pose>,
                                                          IStream<Pose> {

    #region Stream<Pose> Events

    public event Action OnOpen = () => { };
    public event Action<Pose> OnSend = (pose) => { };
    public event Action OnClose = () => { };

    #endregion

    #region IStreamReceiver<Pose> Implementation

    public float alwaysSkipDistance = 0.01f;
    public float maxSkipDistance = 0.20f;
    public float maxSkipAngle = 5f;
    public float maxSkipRotationAngle = 20f;

    private Pose? _lastOutputPose = null;
    private List<Pose> _skippedPoses = new List<Pose>();

    public void Open() {
      _lastOutputPose = null;
      _skippedPoses.Clear();

      OnOpen();
    }

    public void Receive(Pose pose) {
      updateFilter(pose);
    }

    private void updateFilter(Pose pose) {
      Pose? outputPose = null;

      if (!_lastOutputPose.HasValue) {
        outputPose = pose;
      }
      else {
        // Should we skip this pose?
        bool skipThisPose = false;
        bool rememberSkippedPose = true;

        if (_skippedPoses.Count == 0) {
          // Always skip a pose when there are no other skipped poses to compare it to.
          skipThisPose = true;
        }
        else {
          var lastPose = _lastOutputPose.Value;
          var curPose = _skippedPoses[_skippedPoses.Count - 1];
          var nextPose = pose;

          var a = curPose.position - lastPose.position;
          var b = nextPose.position - lastPose.position;

          var sqrDistAB = (b - a).sqrMagnitude;
          if (sqrDistAB < alwaysSkipDistance * alwaysSkipDistance) {
            skipThisPose = true;
            rememberSkippedPose = false;
          }
          else {
            var angleError = Vector3.Angle(a, b);

            var rotAngleError = Quaternion.Angle(curPose.rotation, nextPose.rotation);

            var sqrDistanceSoFar = (a).sqrMagnitude
                                 + (nextPose.position - curPose.position).sqrMagnitude;

            if (angleError > sqrDistanceSoFar.Map(0f, maxSkipDistance * maxSkipDistance,
                                                  maxSkipAngle, 0f)
                || rotAngleError > sqrDistanceSoFar.Map(0f,
                                                        maxSkipDistance * maxSkipDistance,
                                                        maxSkipRotationAngle, 0f)) {
              outputPose = curPose; // note that this is not the input pose,
                                    // but a previously skipped pose.
              _skippedPoses.Clear(); // Clear the other (previous) skipped poses.
              skipThisPose = true;
            }
            else {
              if (sqrDistanceSoFar > maxSkipDistance * maxSkipDistance) {
                outputPose = curPose;
                _skippedPoses.Clear();
                skipThisPose = true;
              }
            }
          }
        }
        
        if (skipThisPose && rememberSkippedPose) {
          _skippedPoses.Add(pose);
        }
      }

      // Potentially output a pose.
      if (outputPose.HasValue) {
        _lastOutputPose = outputPose;

        OnSend(outputPose.Value);
      }
    }

    public void Close() {
      // Output final poses since we know there won't be any more.
      if (_skippedPoses.Count > 0) {
        var lastSkippedPose = _skippedPoses[_skippedPoses.Count - 1];
        OnSend(lastSkippedPose);
      }

      _lastOutputPose = null;
      _skippedPoses.Clear();

      OnClose();
    }

    #endregion

  }

}
