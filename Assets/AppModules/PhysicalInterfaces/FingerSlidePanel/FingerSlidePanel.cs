using Leap.Unity.Interaction;
using Leap.Unity.Portals;
using Leap.Unity.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  using IntObj = InteractionBehaviour;

  public class FingerSlidePanel : MonoBehaviour {

    public Portal portalObj;

    public IntObj portalSurfaceIntObj;

    public Transform slideableObjectsRoot;

    public float thickness = 0.02f;

    public float depthOffset = 0f;

    [Header("Debug")]
    public bool drawDebug = false;

    private void Reset() {
      portalSurfaceIntObj = GetComponent<IntObj>();
    }

    private Vector3?[] touchingFingerPositions = new Vector3?[5];
    private StablePositionsDelta stableFingersDelta = new StablePositionsDelta(5);

    private void OnEnable() {

    }

    private void OnDisable() {

    }

    private void Update() {
      if (portalSurfaceIntObj.isPrimaryHovered) {

        var hand = portalSurfaceIntObj.primaryHoveringController.intHand.leapHand;
        touchingFingerPositions.ClearWithDefaults();
        
        if (hand != null) {
          for (int i = 0; i < hand.Fingers.Count; i++) {
            var finger = hand.Fingers[i];
            var fingertipPosition = finger.TipPosition.ToVector3();

            var portalPose = portalSurfaceIntObj.transform.ToPose() + new Pose(portalSurfaceIntObj.transform.forward * depthOffset);
            var isProjectionOnRect = false;
            var isFingertipOnRect = false;
            var clampedFingertip = fingertipPosition
                                    .ClampedToRect(portalPose, portalObj.width, portalObj.height,
                                                   out isProjectionOnRect, out isFingertipOnRect,
                                                   tolerance: thickness);
            if (isFingertipOnRect && isProjectionOnRect) {
              touchingFingerPositions[i] = clampedFingertip;

              if (drawDebug) {
                DebugPing.Ping(clampedFingertip, LeapColor.amber, 0.10f);
              }
            }

          }
        }

        stableFingersDelta.UpdateCentroidMovement(touchingFingerPositions.ToIndexable());

        if (stableFingersDelta.isMoving) {
          slideableObjectsRoot.transform.position += stableFingersDelta.movement;
        }

      }
    }

    #region Display



    #endregion

    #region Support

    public class StablePositionsDelta {

      private Vector3?[] _lastPositions = null;

      private Vector3? _centroid = null;
      public Vector3? centroid {
        get { return _centroid; }
      }

      private bool _isMoving = false;
      public bool isMoving {
        get { return _isMoving; }
      }

      private bool _didCentroidAppear = false;
      public bool didCentroidAppear {
        get { return _didCentroidAppear; }
      }

      private bool _didCentroidTeleport = false;
      public bool didCentroidTeleport {
        get { return _didCentroidTeleport; }
      }

      private bool _didCentroidDisappear = false;
      public bool didCentroidDisappear {
        get { return _didCentroidDisappear; }
      }

      private Vector3 _avgDelta;
      public Vector3 movement { get { return _avgDelta; } }
       
      public StablePositionsDelta(int maxPositions) {
        _lastPositions = new Vector3?[maxPositions];
      }

      public void UpdateCentroidMovement(IIndexable<Vector3?> positions) {
        bool[] useableIndices = new bool[_lastPositions.Length];

        _didCentroidAppear = false;
        _didCentroidDisappear = false;

        int numLastValidPositions = CountValid(_lastPositions);
        int numCurValidPositions = CountValid(positions);

        if (numLastValidPositions == 0 && numCurValidPositions > 0) {
          _didCentroidAppear = true;
        }
        if (numLastValidPositions > 0 && numCurValidPositions == 0) {
          _didCentroidDisappear = true;
        }

        // Useable indices have valid positions in both the "last" and "current" arrays.
        for (int i = 0; i < _lastPositions.Length; i++) {
          if (i >= positions.Count) break;

          var lastV = _lastPositions[i];
          var curV = positions[i];

          if (lastV.HasValue && curV.HasValue) {
            useableIndices[i] = true;
          }
          else if (!lastV.HasValue && !curV.HasValue) {
            // One index has a value in one array and no value in the other;
            // this means the Centroid is going to teleport.
            _didCentroidTeleport = true;
          }
        }

        _isMoving = false;
        _avgDelta = Vector3.zero;
        int count = 0;
        for (int i = 0; i < useableIndices.Length; i++) {
          _isMoving = true;
          if (useableIndices[i]) {
            _avgDelta += (positions[i] - _lastPositions[i]).Value;
            count++;
          }
        }
        if (count > 0) {
          _avgDelta /= count;
        }

        
        // Update centroid state.

        if (_didCentroidAppear) {
          _centroid = positions.Query()
                               .Select(maybeV => maybeV.GetValueOrDefault())
                               .Fold((acc, v) => acc + v)
                      / numCurValidPositions;

          //DebugPing.Ping(_centroid.Value, LeapColor.cyan, 0.20f);
        }

        if (_centroid != null) {
          _centroid += _avgDelta;

          //DebugPing.Ping(_centroid.Value, LeapColor.green, 0.15f);
        }

        if (_didCentroidDisappear) {
          //DebugPing.Ping(_centroid.Value, LeapColor.black, 0.20f);

          _centroid = null;
        }


        // Set last positions with the current positions.

        for (int i = 0; i < _lastPositions.Length; i++) {
          if (i >= positions.Count) {
            _lastPositions[i] = null;
          }
          else {
            _lastPositions[i] = positions[i];
          }
        }
      }

      private int CountValid(Vector3?[] positions) {
        return positions.Query().Where(v => v.HasValue).Count();
      }

      private int CountValid(IIndexable<Vector3?> positions) {
        return positions.Query().Where(v => v.HasValue).Count();
      }
    }

    #endregion

  }

}
