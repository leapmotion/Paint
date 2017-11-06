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

  /// <summary>
  /// Tests for From() and Then() extension methods on Vector3 and Quaternion types.
  /// </summary>
  public class FromThenExtensionTests {

    public static float EPSILON = 0.0001f;

    public static Vector3 VEC_A = new Vector3(0.5f,  0.2f,  0.8f);
    public static Vector3 VEC_B = new Vector3(0.13f, 0.98f, 3000f);

    public static Quaternion QUAT_A {
      get { return Quaternion.AngleAxis(90f, Vector3.up); }
    }
    public static Quaternion QUAT_B {
      get { return Quaternion.AngleAxis(43f, Vector3.one.normalized); }
    }

    [Test]
    public void FromAVecToBVec() {
      Assert.That(AreVector3sEqual(VEC_A.Then(VEC_B.From(VEC_A)), VEC_B));
    }

    [Test]
    public void FromBVecToAVec() {
      Assert.That(AreVector3sEqual(VEC_B.Then(VEC_A.From(VEC_B)), VEC_A));
    }

    [Test]
    public void FromAQuatToBQuat() {
      Assert.That(AreQuaternionsEqual(QUAT_A.Then(QUAT_B.From(QUAT_A)), QUAT_B));
    }

    [Test]
    public void FromBQuatToAQuat() {
      Assert.That(AreQuaternionsEqual(QUAT_B.Then(QUAT_A.From(QUAT_B)), QUAT_A));
    }

    private static bool AreVector3sEqual(Vector3 a, Vector3 b) {
      return (a - b).magnitude < EPSILON;
    }

    private static bool AreQuaternionsEqual(Quaternion a, Quaternion b) {
      return (a.ToAngleAxisVector() - b.ToAngleAxisVector()).magnitude < EPSILON;
    }

  }
}
