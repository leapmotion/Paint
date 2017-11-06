/******************************************************************************
 * Copyright (C) Leap Motion, Inc. 2011-2017.                                 *
 * Leap Motion proprietary and  confidential.                                 *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Leap Motion and you, your company or other organization.           *
 ******************************************************************************/

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

namespace Leap.Unity.Tests {

  public class PoseTests {

    public static float EPSILON = 0.0001f;

    public static Vector3 VEC_A = new Vector3(0.5f,  0.2f,  0.8f);
    public static Vector3 VEC_B = new Vector3(0.13f, 0.98f, 3000f);

    public static Quaternion QUAT_A {
      get { return Quaternion.AngleAxis(90f, Vector3.up); }
    }
    public static Quaternion QUAT_B {
      get { return Quaternion.AngleAxis(43f, Vector3.one.normalized); }
    }

    public static Pose POSE_A {
      get { return new Pose(VEC_A, QUAT_A); }
    }
    public static Pose POSE_B {
      get { return new Pose(VEC_B, QUAT_B); }
    }

    [Test]
    public void FromAPoseToBPose() {
      Pose aFromB = POSE_A.From(POSE_B);

      Pose recoverA = POSE_B.Then(aFromB);

      Assert.That(AreVector3sEqual(recoverA.position, VEC_A));
      Assert.That(AreQuaternionsEqual(recoverA.rotation, QUAT_A));
    }

    [Test]
    public void FromBPoseToAPose() {
      Pose bFromA = POSE_B.From(POSE_A);

      Pose recoverB = POSE_A.Then(bFromA);

      Assert.That(AreVector3sEqual(recoverB.position, VEC_B));
      Assert.That(AreQuaternionsEqual(recoverB.rotation, QUAT_B));
    }

    private static bool AreVector3sEqual(Vector3 a, Vector3 b) {
      return (a - b).magnitude < EPSILON;
    }

    private static bool AreQuaternionsEqual(Quaternion a, Quaternion b) {
      return (a.ToAngleAxisVector() - b.ToAngleAxisVector()).magnitude < EPSILON;
    }

  }
}
