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

    public float deadzoneWidth = 0.02f;

    public float minDeadzoneWidth = 0.01f;

    public float momentumDecayFriction = 7f;

    [Header("Debug")]
    public bool drawDebug = false;

    private void Reset() {
      portalSurfaceIntObj = GetComponent<IntObj>();
    }

    private Vector3?[] touchingFingerPositions = new Vector3?[5];
    private float[] fingerStrengths = new float[5];
    private StablePositionsDelta stableFingersDelta = new StablePositionsDelta(5);

    // Deadzone
    private Vector3 _deadzoneOrigin = Vector3.zero;
    private bool _useDeadzone = true;

    // Momentum & Smoothing
    /// <summary> 0: Own momentum only. 1: Hand's momentum only. </summary>
    private float _momentumBlend = 0f;
    private Vector3 _ownMomentum = Vector3.zero;

    private void OnEnable() {

    }

    private void OnDisable() {

    }

    private void Update() {

      Vector3 movementFromHand = Vector3.zero;

      // Reset momentum blend, adjusted again if fingertips are nearby the portal plane.
      _momentumBlend = 0f;

      if (portalSurfaceIntObj.isPrimaryHovered) {

        var hand = portalSurfaceIntObj.primaryHoveringController.intHand.leapHand;
        touchingFingerPositions.ClearWithDefaults();
        fingerStrengths.ClearWithDefaults();


        if (hand != null) {
          for (int i = 0; i < hand.Fingers.Count; i++) {
            var finger = hand.Fingers[i];
            var fingertipPosition = finger.TipPosition.ToVector3();

            var portalPose = portalSurfaceIntObj.transform.ToPose() + new Pose(portalSurfaceIntObj.transform.forward * depthOffset);
            var isProjectionOnRect = false;
            var sqrDistToRect = 0f;
            var clampedFingertip = fingertipPosition
                                    .ClampedToRect(portalPose, portalObj.width, portalObj.height,
                                                   out sqrDistToRect, out isProjectionOnRect);

            var pressStrength = sqrDistToRect.Map(0f, thickness * thickness, 1f, 0f);

            if (pressStrength > 0f && isProjectionOnRect) {
              touchingFingerPositions[i] = clampedFingertip;
              fingerStrengths[i] = pressStrength;

              if (drawDebug) {
                DebugPing.Ping(clampedFingertip, LeapColor.amber, 0.10f);
              }
            }

          }
        }

        // Calculate momentum blend increase based on total finger proximity.
        var fingerStrengthMax = fingerStrengths.Query().Fold((acc, f) => (f > acc ? f : acc));
        //var targetBlend = fingerStrengthMax.Map(0f, 0.2f, 0f, 1f);
        //_momentumBlend = Mathf.Lerp(_momentumBlend, targetBlend, 20f * Time.deltaTime);
        _momentumBlend = fingerStrengthMax.Map(0f, 0.4f, 0f, 1f);

        stableFingersDelta.UpdateCentroidMovement(touchingFingerPositions.ToIndexable(),
                                                  fingerStrengths.ToIndexable(),
                                                  drawDebug: drawDebug);

        if (stableFingersDelta.didCentroidAppear) {
          _deadzoneOrigin = stableFingersDelta.centroid.Value;
        }

        if (stableFingersDelta.isMoving) {
          movementFromHand = stableFingersDelta.movement;
          
          if (_useDeadzone) {
            var sqrDistFromOrigin = (_deadzoneOrigin - stableFingersDelta.centroid.Value).sqrMagnitude;

            var deadzoneCoeff = sqrDistFromOrigin.Map(minDeadzoneWidth * minDeadzoneWidth,
                                                    deadzoneWidth * deadzoneWidth,
                                                    0f, 1f);

            if (deadzoneCoeff >= 1f) {
              _useDeadzone = false;
            }

            movementFromHand *= deadzoneCoeff;
          }
        }
        else {
          _useDeadzone = true;
        }

      }

      // Decay momentum via friction.
      var ownMomentumPostFriction = _ownMomentum - (_ownMomentum * momentumDecayFriction * Time.deltaTime);
      if (Vector3.Dot(_ownMomentum, ownMomentumPostFriction) < 0) {
        _ownMomentum = Vector3.zero;
      }
      else {
        _ownMomentum = ownMomentumPostFriction;
      }

      // Calculate and apply momentum.
      _ownMomentum = Vector3.Lerp(_ownMomentum, movementFromHand, _momentumBlend);

      slideableObjectsRoot.transform.position += _ownMomentum;
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

      public void UpdateCentroidMovement(IIndexable<Vector3?> positions,
                                         IIndexable<float> strengths = null,
                                         bool drawDebug = false) {
        if (strengths != null && positions.Count != strengths.Count) {
          throw new InvalidOperationException(
            "positions and strengths Indexables must have the same Count.");
        }


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
          if (useableIndices[i]) {
            _isMoving = true;
            var addedDelta = (positions[i] - _lastPositions[i]).Value;
            if (strengths != null) {
              addedDelta *= strengths[i];
            }
            _avgDelta += addedDelta;
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

          if (drawDebug) {
            DebugPing.Ping(_centroid.Value, LeapColor.cyan, 0.20f);
          }
        }

        if (_centroid != null) {
          _centroid += _avgDelta;

          if (drawDebug) {
            DebugPing.Ping(_centroid.Value, LeapColor.green, 0.15f);
          }
        }

        if (_didCentroidDisappear) {
          if (drawDebug) {
            DebugPing.Ping(_centroid.Value, LeapColor.black, 0.20f);
          }

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

      public void UpdateCentroidMovement(IIndexable<Vector3?> positions,
                                         bool drawDebug = false) {
        UpdateCentroidMovement(positions, null, drawDebug);
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
