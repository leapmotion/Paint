/******************************************************************************
 * Copyright (C) Leap Motion, Inc. 2011-2017.                                 *
 * Leap Motion proprietary and  confidential.                                 *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Leap Motion and you, your company or other organization.           *
 ******************************************************************************/

using Leap.Unity.PhysicalInterfaces;
using System;
using UnityEngine;

namespace Leap.Unity.Animation {

  /// <summary>
  /// Represents a spline for poses -- positions and rotations -- that travel from one
  /// position and rotation in space to another over a specified time frame.  The two
  /// endpoints are specified, as well as the instantaneous velocity and angular velocity
  /// at those two endpoints.
  /// 
  /// You may ask for the position, rotation, velocity, or angular velocity at any time
  /// along the spline's duration.
  /// </summary>
  [Serializable]
  public struct HermitePoseSpline {
    public float t0, t1;
    public Vector3 pos0, pos1;
    public Vector3 vel0, vel1;
    public Quaternion rot0, rot1;
    public Vector3 angVel0, angVel1;

    /// <summary>
    /// Constructs a spline by specifying the poses of the two endpoints. The velocity
    /// and angular velocity at each endpoint is zero, and the time range of the spline
    /// is 0 to 1.
    /// </summary>
    public HermitePoseSpline(Pose pose0, Pose pose1) {
      t0 = 0;
      t1 = 1;

      this.vel0 = default(Vector3);
      this.vel1 = default(Vector3);

      this.angVel0 = default(Vector3);
      this.angVel1 = default(Vector3);

      this.pos0 = pose0.position;
      this.pos1 = pose1.position;

      this.rot0 = pose0.rotation;
      this.rot1 = pose1.rotation;
    }

    /// <summary>
    /// Constructs a spline by specifying the poses and movements of the two
    /// endpoints. The time range of the spline is 0 to 1.
    /// </summary>
    public HermitePoseSpline(Pose pose0, Pose pose1, Movement move0, Movement move1) {
      t0 = 0;
      t1 = 1;

      this.vel0 = move0.velocity;
      this.vel1 = move1.velocity;

      this.angVel0 = move0.angularVelocity;
      this.angVel1 = move1.angularVelocity;

      this.pos0 = pose0.position;
      this.pos1 = pose1.position;

      this.rot0 = pose0.rotation;
      this.rot1 = pose1.rotation;
    }

    /// <summary>
    /// Constructs a spline by specifying the positions and velocities of the two
    /// endpoints. The time range of the spline is 0 to duration.
    /// </summary>
    public HermitePoseSpline(Pose pose0, Pose pose1,
                             Movement move0, Movement move1,
                             float duration) {
      t0 = 0;
      t1 = duration;

      this.vel0 = move0.velocity;
      this.vel1 = move1.velocity;

      this.angVel0 = move0.angularVelocity;
      this.angVel1 = move1.angularVelocity;

      this.pos0 = pose0.position;
      this.pos1 = pose1.position;

      this.rot0 = pose0.rotation;
      this.rot1 = pose1.rotation;
    }

    /// <summary>
    /// Constructs a spline by specifying the positions, velocities, and times of the
    /// endpoints.
    /// </summary>
    public HermitePoseSpline(float t0, float t1,
                             Pose pose0, Pose pose1,
                             Movement move0, Movement move1) {
      this.t0 = t0;
      this.t1 = t1;

      this.vel0 = move0.velocity;
      this.vel1 = move1.velocity;

      this.angVel0 = move0.angularVelocity;
      this.angVel1 = move1.angularVelocity;

      this.pos0 = pose0.position;
      this.pos1 = pose1.position;

      this.rot0 = pose0.rotation;
      this.rot1 = pose1.rotation;
    }

    /// <summary>
    /// Gets the position at time t along this spline. The time is clamped within the
    /// t0 - t1 range.
    /// </summary>
    public Vector3 PositionAt(float t) {
      float i = Mathf.Clamp01((t - t0) / (t1 - t0));
      float i2 = i * i;
      float i3 = i2 * i;

      Vector3 h00 = (2 * i3 - 3 * i2 + 1) * pos0;
      Vector3 h10 = (i3 - 2 * i2 + i) * (t1 - t0) * vel0;
      Vector3 h01 = (-2 * i3 + 3 * i2) * pos1;
      Vector3 h11 = (i3 - i2) * (t1 - t0) * vel1;

      return h00 + h10 + h01 + h11;
    }

    /// <summary>
    /// Gets the rotation at time t along this spline. The time is clamped within the
    /// t0 - t1 range.
    /// </summary>
    public Quaternion RotationAt(float t) {
      float i = Mathf.Clamp01((t - t0) / (t1 - t0));
      float i2 = i * i;
      float i3 = i2 * i;

      //Quaternion h00 = (2 * i3 - 3 * i2 + 1) * rot0;
      //Quaternion h10 = (i3 - 2 * i2 + i) * (t1 - t0) * angVel0;
      //Quaternion h01 = (-2 * i3 + 3 * i2) * rot1;
      //Quaternion h11 = (i3 - i2) * (t1 - t0) * angVel1;
      //
      //return h00 + h10 + h01 + h11;

      var identity = Quaternion.identity;
      var dRot0 = Utils.QuaternionFromAngleAxisVector(angVel0);
      var dRot1 = Utils.QuaternionFromAngleAxisVector(angVel1);

      Quaternion h00 = Quaternion.SlerpUnclamped(identity, rot0, 2 * i3 - 3 * i2 + 1);
      Quaternion h10 = Quaternion.SlerpUnclamped(identity, dRot0, (i3 - 2 * i2 + i) * (t1 - t0));
      Quaternion h01 = Quaternion.SlerpUnclamped(identity, rot1, -2 * i3 + 3 * i2);
      Quaternion h11 = Quaternion.SlerpUnclamped(identity, dRot1, (i3 - i2) * (t1 - t0));

      return h00.Then(h10).Then(h01).Then(h11);
    }

    /// <summary>
    /// Gets the pose at time t along this spline. The time is clamped within the t0 - t1
    /// range.
    /// </summary>
    public Pose PoseAt(float t) {
      return new Pose(PositionAt(t), RotationAt(t));
    }

    /// <summary>
    /// Gets the first derivative of position at time t. The time is clamped within the
    /// t0 - t1 range.
    /// </summary>
    public Vector3 VelocityAt(float t) {
      float C00 = t1 - t0;
      float C1 = 1.0f / C00;

      float i, i2;
      float i_, i2_, i3_;
      {
        i = Mathf.Clamp01((t - t0) * C1);
        i_ = C1;

        i2 = i * i;
        i2_ = 2 * i * i_;

        i3_ = i2_ * i + i_ * i2;
      }

      Vector3 h00_ = (i3_ * 2 - i2_ * 3) * pos0;
      Vector3 h10_ = (i3_ - 2 * i2_ + i_) * C00 * vel0;
      Vector3 h01_ = (i2_ * 3 - 2 * i3_) * pos1;
      Vector3 h11_ = (i3_ - i2_) * C00 * vel1;

      return h00_ + h01_ + h10_ + h11_;
    }

    /// <summary>
    /// Gets the first derivative of rotation at time t. The time is clamped within the
    /// t0 - t1 range. Angular velocity is encoded as an angle-axis vector.
    /// </summary>
    public Vector3 AngularVelocityAt(float t) {
      float C00 = t1 - t0;
      float C1 = 1.0f / C00;

      float i, i2;
      float i_, i2_, i3_;
      {
        i = Mathf.Clamp01((t - t0) * C1);
        i_ = C1;

        i2 = i * i;
        i2_ = 2 * i * i_;

        i3_ = i2_ * i + i_ * i2;
      }

      //Quaternion h00_ = (i3_ * 2 - i2_ * 3) * rot0;
      //Quaternion h10_ = (i3_ - 2 * i2_ + i_) * C00 * angVel0;
      //Quaternion h01_ = (i2_ * 3 - 2 * i3_) * rot1;
      //Quaternion h11_ = (i3_ - i2_) * C00 * angVel1;
      //
      //return h00_ + h01_ + h10_ + h11_;

      var identity = Quaternion.identity;
      var dRot0 = Utils.QuaternionFromAngleAxisVector(angVel0);
      var dRot1 = Utils.QuaternionFromAngleAxisVector(angVel1);

      Quaternion h00_ = Quaternion.SlerpUnclamped(identity, rot0, (i3_ * 2 - i2_ * 3));
      Quaternion h10_ = Quaternion.SlerpUnclamped(identity, dRot0, (i3_ - 2 * i2_ + i_) * C00);
      Quaternion h01_ = Quaternion.SlerpUnclamped(identity, rot1, (i2_ * 3 - 2 * i3_));
      Quaternion h11_ = Quaternion.SlerpUnclamped(identity, dRot1, (i3_ - i2_) * C00);

      return h00_.Then(h01_).Then(h10_).Then(h11_).ToAngleAxisVector();
    }

    public Movement MovementAt(float t) {
      return new Movement(VelocityAt(t), AngularVelocityAt(t));
    }

    /// <summary>
    /// Gets both the position and the first derivative of position at time t. The time
    /// is clamped within the t0 - t1 range.
    /// </summary>
    public void PositionAndVelAt(float t, out Vector3 position, out Vector3 velocity) {
      float C00 = t1 - t0;
      float C1 = 1.0f / C00;

      float i, i2, i3;
      float i_, i2_, i3_;
      {
        i = Mathf.Clamp01((t - t0) * C1);
        i_ = C1;

        i2 = i * i;
        i2_ = 2 * i * i_;

        i3 = i2 * i;
        i3_ = i2_ * i + i_ * i2;
      }

      Vector3 h00 = (2 * i3 - 3 * i2 + 1) * pos0;
      Vector3 h00_ = (i3_ * 2 - i2_ * 3) * pos0;

      Vector3 h10 = (i3 - 2 * i2 + i) * C00 * vel0;
      Vector3 h10_ = (i3_ - 2 * i2_ + i_) * C00 * vel0;

      Vector3 h01 = (3 * i2 - 2 * i3) * pos1;
      Vector3 h01_ = (i2_ * 3 - 2 * i3_) * pos1;

      Vector3 h11 = (i3 - i2) * C00 * vel1;
      Vector3 h11_ = (i3_ - i2_) * C00 * vel1;

      position = h00 + h01 + h10 + h11;
      velocity = h00_ + h01_ + h10_ + h11_;
    }

    /// <summary>
    /// Gets both the rotation and the first derivative of rotation at time t. The time
    /// is clamped within the t0 - t1 range. Angular velocity is encoded as an angle-axis
    /// vector.
    /// </summary>
    public void RotationAndAngVelAt(float t, out Quaternion rotation,
                                             out Vector3 angularVelocity) {
      float C00 = t1 - t0;
      float C1 = 1.0f / C00;

      float i, i2, i3;
      float i_, i2_, i3_;
      {
        i = Mathf.Clamp01((t - t0) * C1);
        i_ = C1;

        i2 = i * i;
        i2_ = 2 * i * i_;

        i3 = i2 * i;
        i3_ = i2_ * i + i_ * i2;
      }

      var identity = Quaternion.identity;
      var dRot0 = Utils.QuaternionFromAngleAxisVector(angVel0);
      var dRot1 = Utils.QuaternionFromAngleAxisVector(angVel1);


      Quaternion h00 = Quaternion.SlerpUnclamped(identity, rot0, 2 * i3 - 3 * i2 + 1);
      Quaternion h00_ = Quaternion.SlerpUnclamped(identity, rot0, (i3_ * 2 - i2_ * 3));

      Quaternion h10 = Quaternion.SlerpUnclamped(identity, dRot0, (i3 - 2 * i2 + i) * C00);
      Quaternion h10_ = Quaternion.SlerpUnclamped(identity, dRot0, (i3_ - 2 * i2_ + i_) * C00);

      Quaternion h01 = Quaternion.SlerpUnclamped(identity, rot1, -2 * i3 + 3 * i2);
      Quaternion h01_ = Quaternion.SlerpUnclamped(identity, rot1, (i2_ * 3 - 2 * i3_));

      Quaternion h11 = Quaternion.SlerpUnclamped(identity, dRot1, (i3 - i2) * C00);
      Quaternion h11_ = Quaternion.SlerpUnclamped(identity, dRot1, (i3_ - i2_) * C00);


      rotation = h00.Then(h10).Then(h01).Then(h11);
      angularVelocity = h00_.Then(h01_).Then(h10_).Then(h11_).ToAngleAxisVector();
    }


    /// <summary>
    /// Gets both the rotation and the first derivative of rotation at time t. The time
    /// is clamped within the t0 - t1 range. Angular velocity is encoded as an angle-axis
    /// vector.
    /// 
    /// Gets both the pose and position/rotation first derivative at time t. The time is
    /// clamped within the t0 - t1 range. Angular velocity is encoded as an angle-axis
    /// vector.
    /// </summary>
    public void PoseAndMovementAt(float t, out Pose pose,
                                           out Movement movement) {
      float C00 = t1 - t0;
      float C1 = 1.0f / C00;

      float i, i2, i3;
      float i_, i2_, i3_;
      {
        i = Mathf.Clamp01((t - t0) * C1);
        i_ = C1;

        i2 = i * i;
        i2_ = 2 * i * i_;

        i3 = i2 * i;
        i3_ = i2_ * i + i_ * i2;
      }

      var identity = Quaternion.identity;
      var dRot0 = Utils.QuaternionFromAngleAxisVector(angVel0);
      var dRot1 = Utils.QuaternionFromAngleAxisVector(angVel1);


      Vector3 h00 = (2 * i3 - 3 * i2 + 1) * pos0;
      Vector3 h00_ = (i3_ * 2 - i2_ * 3) * pos0;
      Quaternion R_h00 = Quaternion.SlerpUnclamped(identity, rot0, 2 * i3 - 3 * i2 + 1);
      Quaternion R_h00_ = Quaternion.SlerpUnclamped(identity, rot0, (i3_ * 2 - i2_ * 3));

      Vector3 h10 = (i3 - 2 * i2 + i) * C00 * vel0;
      Vector3 h10_ = (i3_ - 2 * i2_ + i_) * C00 * vel0;
      Quaternion R_h10 = Quaternion.SlerpUnclamped(identity, dRot0, (i3 - 2 * i2 + i) * C00);
      Quaternion R_h10_ = Quaternion.SlerpUnclamped(identity, dRot0, (i3_ - 2 * i2_ + i_) * C00);

      Vector3 h01 = (3 * i2 - 2 * i3) * pos1;
      Vector3 h01_ = (i2_ * 3 - 2 * i3_) * pos1;
      Quaternion R_h01 = Quaternion.SlerpUnclamped(identity, rot1, -2 * i3 + 3 * i2);
      Quaternion R_h01_ = Quaternion.SlerpUnclamped(identity, rot1, (i2_ * 3 - 2 * i3_));

      Vector3 h11_ = (i3_ - i2_) * C00 * vel1;
      Vector3 h11 = (i3 - i2) * C00 * vel1;
      Quaternion R_h11 = Quaternion.SlerpUnclamped(identity, dRot1, (i3 - i2) * C00);
      Quaternion R_h11_ = Quaternion.SlerpUnclamped(identity, dRot1, (i3_ - i2_) * C00);

      
      var position = h00 + h01 + h10 + h11;
      var velocity = h00_ + h01_ + h10_ + h11_;
      
      var rotation = R_h00.Then(R_h10).Then(R_h01).Then(R_h11);
      var angularVelocity = R_h00_.Then(R_h01_).Then(R_h10_).Then(R_h11_).ToAngleAxisVector();

      pose = new Pose(position, rotation);
      movement = new Movement(velocity, angularVelocity);
    }
  }

  public static class HermitePoseSplineExtensions {
    
    public static void DrawPoseSpline(this RuntimeGizmos.RuntimeGizmoDrawer drawer,
                                      HermitePoseSpline spline,
                                      Color? color = null,
                                      float poseGizmoScale = 0.02f,
                                      int splineResolution = 32,
                                      int drawPosePeriod = 8) {
      if (!color.HasValue) {
        color = LeapColor.brown.WithAlpha(0.4f);
      }
      drawer.color = color.Value;

      var tWidth = spline.t1 - spline.t0;

      Vector3? prevPos = null;
      int counter = 0;
      float tStep = (1f / splineResolution) * tWidth;
      for (float t = spline.t0; t <= spline.t0 + tWidth; t += tStep) {
        var pose = spline.PoseAt(t);

        if (counter % drawPosePeriod == 0) {
          drawer.DrawPose(pose, 0.02f);
        }

        if (prevPos.HasValue) {
          drawer.DrawLine(prevPos.Value, pose.position);
        }

        prevPos = pose.position;
        counter++;
      }
    }

  }
}
