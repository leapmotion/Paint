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
  public struct HermitePoseSpline : ISpline<Pose, Movement>,
                                    ISpline<Vector3, Vector3> {

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

      //if (Quaternion.Dot(rot0, rot1) < 0f) {
      //  Debug.Log("Quaternions negative dot product; flipping one.");
      //  rot1 = new Quaternion(-rot1.x, -rot1.y, -rot1.z, rot1.w);
      //}
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

      //if (Quaternion.Dot(rot0, rot1) < 0f) {
      //  Debug.Log("Quaternions negative dot product; flipping one.");
      //  rot1 = new Quaternion(-rot1.x, -rot1.y, -rot1.z, rot1.w);
      //}
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

      //if (Quaternion.Dot(rot0, rot1) < 0f) {
      //  Debug.Log("Quaternions negative dot product; flipping one.");
      //  rot1 = new Quaternion(-rot1.x, -rot1.y, -rot1.z, -rot1.w);
      //}
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

      this.rot0 = pose0.rotation.ToNormalized();
      this.rot1 = pose1.rotation.ToNormalized();

      //if (Quaternion.Dot(rot0, rot1) < 0f) {
      //  if (Quaternion.Dot(rot0, Quaternion.identity) < 0f) {
      //    rot0 = rot0.Flipped();
      //  }
      //  else if (Quaternion.Dot(rot1, Quaternion.identity) < 0f) {
      //    rot1 = rot1.Flipped();
      //  }
      //}
    }

    /// <summary>
    /// Gets the position at time t along this spline. The time is clamped within the
    /// t0 - t1 range.
    /// </summary>
    public Vector3 PositionAt(float t) {
      float i = Mathf.Clamp01((t - t0) / (t1 - t0));
      float i2 = i * i;
      float i3 = i2 * i;

      float dt = t1 - t0;

      Vector3 h00 = (2 * i3 - 3 * i2 + 1) * pos0;
      Vector3 h10 = (i3 - 2 * i2 + i) * dt * vel0;
      Vector3 h01 = (-2 * i3 + 3 * i2) * pos1;
      Vector3 h11 = (i3 - i2) * dt * vel1;

    //   ( 2*i3 - 3*i2 + 0*i + 1) * A
    // + ( 1*i3 - 2*i2 + 1*i + 0) * (tWidth) * B
    // + (-2*i3 + 3*i2 + 0*i + 0) * C
    // + ( 1*i3 - 1*i2 + 0*i + 0) * (tWidth) * D;

      return h00 + h10 + h01 + h11;
    }

    private Quaternion QuatFromAngleAxis(Vector3 angleAxisVector) {
      var angle = angleAxisVector.magnitude;
      var axis = angleAxisVector / angle;
      return Quaternion.AngleAxis(angle, axis);
    }

    // Quaternion CHS reference:
    // Kim, Kim, and Shin, 1995.
    // A General Construction Scheme for Unit Quaternion Curves with Simple High Order
    // Derivatives.
    // http://graphics.cs.cmu.edu/nsp/course/15-464/Fall05/papers/kimKimShin.pdf

    /// <summary>
    /// Gets the rotation at time t along this spline. The time is clamped within the
    /// t0 - t1 range.
    /// </summary>
    public Quaternion RotationAt(float t) {
      float i = Mathf.Clamp01((t - t0) / (t1 - t0));
      float i2 = i * i;
      float i3 = i2 * i;

      float dt = t1 - t0;

      var oneThird = 1 / 3f;

      var w1 = Quaternion.Inverse(rot0) * angVel0 * dt * oneThird;
      var w3 = Quaternion.Inverse(rot1) * angVel1 * dt * oneThird;
      var w2 = (QuatFromAngleAxis(-w1)
                * Quaternion.Inverse(rot0)
                * rot1
                * QuatFromAngleAxis(-w3)).ToAngleAxisVector();

      var beta1 = i3 - (3 * i2) + (3 * i);
      var beta2 = -2 * i3 + 3 * i2;
      var beta3 = i3;

      return rot0 * QuatFromAngleAxis(w1 * beta1)
                  * QuatFromAngleAxis(w2 * beta2)
                  * QuatFromAngleAxis(w3 * beta3);

      // A cubic Bezier quaternion curve can be used to define a Hermite quaternion curve
      // which interpolates two end unit quaternions, q_a and q_b, and two angular
      // velocities omega_a and omega_b.
      //
      // var q_at_t = q_0 * PRODUCT(i = 1, i < 3, exp(omega_i * beta_q(i, t))
      // "where beta_q(i, t) is beta_q(i, 3, t) for i = 1, 2, 3."
      // 
      // q coefficients:
      // q_0 = q_a
      // q_1 = q_a * exp(omega_a / 3)
      // q_2 = q_b * exp(omega_b / 3)^-1
      // q_3 = q_b
      //
      // omega coefficients:
      // omega_1 = omega_a / 3
      // omega_2 = log(exp(omega_a / 3)^-1 * q_a^-1 * q_b * exp(omega_b / 3)^-1)
      // omega_3 = omega_b / 3
      //
      // Dependencies/definitions:
      // "Bernstein basis", referred to as "beta"
      // beta(i, n, t) = (n Choose i) * (1 - t)^(n - i) * t^i
      // The form used in the formula are the cumulative basis functions:
      // beta_q(i, n, t) = SUM(j = i, n, beta(i, n, t))
      //
      // The Exponential Mapping exp(v) and its Inverse
      // "The exponential map can be interpreted as a mapping from the angular velocity
      // vector (measured in S^3) into the unit quaternion which represents the rotation."
      // ...
      // "Given a vector v = theta * v_norm (in R^3), the exponential:
      // exp(v) = SUM(i = 0, i -> inf, v^i) = (cos(theta), v_norm * sin(theta)) in S^3
      // becomes the unit quaternion which represents the rotation by angle 2*theta
      // about the axis v_norm, where v^i is computed using the quaternion multiplication."
      // 
      // In other words... The Exponential converts a VECTOR represented by an ANGLE
      // and a normalized AXIS to a quaternion. A Unity function for this:
      // exp(v) -> Q := Quaternion.AngleAxis(v.magnitude, v.normalized);
      //
      // Its inverse map, log, would thus be the conversion from a Quaternion to an
      // Angle-Axis representation. Quaternion.ToAngleAxisVector():
      // log(Q) -> v := Q.ToAngleAxisVector() -> v
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
      //return Vector3.zero;

      float i = Mathf.Clamp01((t - t0) / (t1 - t0));
      float i2 = i * i;
      float i3 = i2 * i;

      float dt = t1 - t0;

      var oneThird = 1 / 3f;

      var w1 = Quaternion.Inverse(rot0) * angVel0 * dt * oneThird;
      var w3 = Quaternion.Inverse(rot1) * angVel1 * dt * oneThird;
      var w2 = (QuatFromAngleAxis(-w1)
                * Quaternion.Inverse(rot0)
                * rot1
                * QuatFromAngleAxis(-w3)).ToAngleAxisVector();

      var beta1 = i3 - (3 * i2) + (3 * i);
      var beta2 = -2 * i3 + 3 * i2;
      //var beta3 = i3;

      // Derivatives of beta1, beta2, beta3
      var dotBeta1 = 3 * i2 - 5 * i + 3;
      var dotBeta2 = -6 * i2 + 6 * i;
      var dotBeta3 = 3 * i2;

      var rot0_times_w1beta1 = rot0 * QuatFromAngleAxis(w1 * beta1);

      return
        rot0 * w1 * dotBeta1 +
        rot0_times_w1beta1 * w2 * dotBeta2 +
        rot0_times_w1beta1 * QuatFromAngleAxis(w2 * beta2) * w3 * dotBeta3;
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
      float i = Mathf.Clamp01((t - t0) / (t1 - t0));

      rotation = Quaternion.Slerp(rot0, rot1, DefaultCurve.SigmoidUp.Evaluate(i));
      angularVelocity = Vector3.zero;

      //float C00 = t1 - t0;
      //float C1 = 1.0f / C00;
      //
      //float i, i2, i3;
      //float i_, i2_, i3_;
      //{
      //  i = Mathf.Clamp01((t - t0) * C1);
      //  i_ = C1;
      //
      //  i2 = i * i;
      //  i2_ = 2 * i * i_;
      //
      //  i3 = i2 * i;
      //  i3_ = i2_ * i + i_ * i2;
      //}
      //
      //var identity = Quaternion.identity;
      //var dRot0 = Utils.QuaternionFromAngleAxisVector(angVel0).ToNormalized();
      //var dRot1 = Utils.QuaternionFromAngleAxisVector(angVel1).ToNormalized();
      //
      //
      //Quaternion h00 = Quaternion.SlerpUnclamped(identity, rot0, 2 * i3 - 3 * i2 + 1);
      //Quaternion h00_ = Quaternion.SlerpUnclamped(identity, rot0, (i3_ * 2 - i2_ * 3));
      //
      //Quaternion h10 = Quaternion.SlerpUnclamped(identity, dRot0, (i3 - 2 * i2 + i) * C00);
      //Quaternion h10_ = Quaternion.SlerpUnclamped(identity, dRot0, (i3_ - 2 * i2_ + i_) * C00);
      //
      //Quaternion h01 = Quaternion.SlerpUnclamped(identity, rot1, -2 * i3 + 3 * i2);
      //Quaternion h01_ = Quaternion.SlerpUnclamped(identity, rot1, (i2_ * 3 - 2 * i3_));
      //
      //Quaternion h11 = Quaternion.SlerpUnclamped(identity, dRot1, (i3 - i2) * C00);
      //Quaternion h11_ = Quaternion.SlerpUnclamped(identity, dRot1, (i3_ - i2_) * C00);
      //
      //rotation = h00.Then(h10).Then(h01).Then(h11);
      //angularVelocity = h00_.Then(h01_).Then(h10_).Then(h11_).ToAngleAxisVector();
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

    #region ISpline<Pose, Movement>

    public float minT { get { return t0; } }

    public float maxT { get { return t1; } }

    public Pose ValueAt(float t) {
      return PoseAt(t);
    }

    public Movement DerivativeAt(float t) {
      return MovementAt(t);
    }

    public void ValueAndDerivativeAt(float t, out Pose value, out Movement deltaValuePerSec) {
      PoseAndMovementAt(t, out value, out deltaValuePerSec);
    }

    #endregion

    #region ISpline<Vector3, Vector3>
    
    float ISpline<Vector3, Vector3>.minT { get { return t0; } }

    float ISpline<Vector3, Vector3>.maxT { get { return t1; } }

    Vector3 ISpline<Vector3, Vector3>.ValueAt(float t) {
      return PoseAt(t).position;
    }

    Vector3 ISpline<Vector3, Vector3>.DerivativeAt(float t) {
      return MovementAt(t).velocity;
    }

    void ISpline<Vector3, Vector3>.ValueAndDerivativeAt(float t, 
                                                        out Vector3 value,
                                                        out Vector3 deltaValuePerT) {
      Pose pose;
      Movement movement;
      PoseAndMovementAt(t, out pose, out movement);

      value = pose.position;
      deltaValuePerT = movement.velocity;
    }

    #endregion

  }

  public static class HermitePoseSplineExtensions {
    
    public static void DrawPoseSpline(this RuntimeGizmos.RuntimeGizmoDrawer drawer,
                                      HermitePoseSpline spline,
                                      Color? color = null,
                                      float poseGizmoScale = 0.02f,
                                      int splineResolution = 32,
                                      int drawPosePeriod = 8,
                                      bool drawPoses = true,
                                      bool drawSegments = true) {
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

        if (counter % drawPosePeriod == 0 && drawPoses) {
          drawer.DrawPose(pose, 0.02f);
        }

        if (prevPos.HasValue && drawSegments) {
          drawer.DrawLine(prevPos.Value, pose.position);
        }

        prevPos = pose.position;
        counter++;
      }
    }

  }
}
