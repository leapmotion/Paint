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

  public class Matrix4x4Tests {

    public static float EPSILON = 0.0001f;

    private Matrix4x4 B {
      get {
        return Matrix4x4.TRS(Vector3.one * 20f,
                             Quaternion.AngleAxis(24f, Vector3.up),
                             Vector3.one * 2f);
      }
    }

    private Matrix4x4 A {
      get {
        return Matrix4x4.TRS(Vector3.right * 100f,
                             Quaternion.AngleAxis(77f, Vector3.one),
                             Vector3.one * 35f);
      }
    }

    [Test]
    public void SanityA() {
      var shouldBeIdentity = A.inverse * A;
      var shouldAlsoBeIdentity = A * A.inverse;

      Assert.That(AreMatricesEqual(shouldBeIdentity, Matrix4x4.identity));
      Assert.That(AreMatricesEqual(shouldAlsoBeIdentity, Matrix4x4.identity));
    }

    [Test]
    public void SanityB() {
      var shouldBeIdentity = B.inverse * B;
      var shouldAlsoBeIdentity = B * B.inverse;

      Assert.That(AreMatricesEqual(shouldBeIdentity, Matrix4x4.identity));
      Assert.That(AreMatricesEqual(shouldAlsoBeIdentity, Matrix4x4.identity));
    }

    [Test]
    public void GetToAFromB() {
      Assert.That(AreMatricesEqual(B.Then(A.From(B)), A));
    }

    [Test]
    public void GetToBFromA() {
      Assert.That(AreMatricesEqual(A.Then(B.From(A)), B));
    }

    private static bool AreMatricesEqual(Matrix4x4 a, Matrix4x4 b) {
      return AreVector3sEqual(a.GetVector3(), b.GetVector3())
          && AreQuaternionsEqual(a.GetQuaternion(), b.GetQuaternion());
    }

    private static bool AreVector3sEqual(Vector3 a, Vector3 b) {
      return (a - b).magnitude < EPSILON;
    }

    private static bool AreQuaternionsEqual(Quaternion a, Quaternion b) {
      //return (a.ToAngleAxisVector() - b.ToAngleAxisVector()).magnitude < EPSILON;

      return (a * Vector3.forward - b * Vector3.forward).magnitude < EPSILON
          && (a * Vector3.up      - b * Vector3.up).magnitude < EPSILON;
    }

  }
}
