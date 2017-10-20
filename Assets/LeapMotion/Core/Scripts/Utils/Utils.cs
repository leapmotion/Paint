/******************************************************************************
 * Copyright (C) Leap Motion, Inc. 2011-2017.                                 *
 * Leap Motion proprietary and  confidential.                                 *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Leap Motion and you, your company or other organization.           *
 ******************************************************************************/

using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.IO;
using System.Collections.Generic;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity.Query;

namespace Leap.Unity {

  public static class Utils {

    #region Generic Utils

    /// <summary>
    /// Swaps the references of a and b.  Note that you can pass
    /// in references to array elements if you want!
    /// </summary>
    public static void Swap<T>(ref T a, ref T b) {
      T temp = a;
      a = b;
      b = temp;
    }

    /// <summary>
    /// Utility extension to swap the elements at index a and index b.
    /// </summary>
    public static void Swap<T>(this IList<T> list, int a, int b) {
      T temp = list[a];
      list[a] = list[b];
      list[b] = temp;
    }

    /// <summary>
    /// Utility extension to swap the elements at index a and index b.
    /// </summary>
    public static void Swap<T>(this T[] array, int a, int b) {
      Swap(ref array[a], ref array[b]);
    }

    /// <summary>
    /// System.Array.Reverse is actually suprisingly complex / slow.  This
    /// is a basic generic implementation of the reverse algorithm.
    /// </summary>
    public static void Reverse<T>(this T[] array) {
      int mid = array.Length / 2;
      int i = 0;
      int j = array.Length;
      while (i < mid) {
        array.Swap(i++, --j);
      }
    }

    /// <summary>
    /// Shuffle the given list into a different permutation.
    /// </summary>
    public static void Shuffle<T>(this IList<T> list) {
      for (int i = 0; i < list.Count; i++) {
        Utils.Swap(list, i, UnityEngine.Random.Range(0, list.Count));
      }
    }

    public static void DoubleCapacity<T>(ref T[] array) {
      T[] newArray = new T[array.Length * 2];
      Array.Copy(array, newArray, array.Length);
      array = newArray;
    }

    /// <summary>
    /// Returns whether or not two lists contain the same elements ignoring order.
    /// </summary>
    public static bool AreEqualUnordered<T>(IList<T> a, IList<T> b) {
      var _count = Pool<Dictionary<T, int>>.Spawn();
      try {
        int _nullCount = 0;

        foreach (var i in a) {
          if (i == null) {
            _nullCount++;
          } else {
            int count;
            if (!_count.TryGetValue(i, out count)) {
              count = 0;
            }
            _count[i] = count + 1;
          }
        }

        foreach (var i in b) {
          if (i == null) {
            _nullCount--;
          } else {
            int count;
            if (!_count.TryGetValue(i, out count)) {
              return false;
            }
            _count[i] = count - 1;
          }
        }

        if (_nullCount != 0) {
          return false;
        }

        foreach (var pair in _count) {
          if (pair.Value != 0) {
            return false;
          }
        }

        return true;
      } finally {
        _count.Clear();
        Pool<Dictionary<T, int>>.Recycle(_count);
      }
    }

    // http://stackoverflow.com/a/19317229/2471635
    /// <summary>
    /// Returns whether this type implements the argument interface type.
    /// If the argument type is not an interface, returns false.
    /// </summary>
    public static bool ImplementsInterface(this Type type, Type ifaceType) {
      Type[] intf = type.GetInterfaces();
      for (int i = 0; i < intf.Length; i++) {
        if (intf[i] == ifaceType) {
          return true;
        }
      }
      return false;
    }

    public static bool IsActiveRelativeToParent(this Transform obj, Transform parent) {
      Assert.IsTrue(obj.IsChildOf(parent));

      if (!obj.gameObject.activeSelf) {
        return false;
      } else {
        if (obj.parent == null || obj.parent == parent) {
          return true;
        } else {
          return obj.parent.IsActiveRelativeToParent(parent);
        }
      }
    }

    /// <summary>
    /// Given a list of comparable types, return an ordering that orders the
    /// elements into sorted order.  The ordering is a list of indices where each
    /// index refers to the element located at that index in the original list.
    /// </summary>
    public static List<int> GetSortedOrder<T>(this IList<T> list) where T : IComparable<T> {
      Assert.IsNotNull(list);

      List<int> ordering = new List<int>();
      for (int i = 0; i < list.Count; i++) {
        ordering.Add(i);
      }

      ordering.Sort((a, b) => list[a].CompareTo(list[b]));

      return ordering;
    }

    /// <summary>
    /// Given a list and an ordering, order the list according to the ordering.
    /// This method assumes the ordering is a valid ordering.
    /// </summary>
    public static void ApplyOrdering<T>(this IList<T> list, List<int> ordering) {
      Assert.IsNotNull(list);
      Assert.IsNotNull(ordering);
      Assert.AreEqual(list.Count, ordering.Count, "List must be the same length as the ordering.");

      List<T> copy = Pool<List<T>>.Spawn();
      try {
        copy.AddRange(list);
        for (int i = 0; i < list.Count; i++) {
          list[i] = copy[ordering[i]];
        }
      } finally {
        copy.Clear();
        Pool<List<T>>.Recycle(copy);
      }
    }

    public static string MakeRelativePath(string relativeTo, string path) {
      if (string.IsNullOrEmpty(relativeTo)) throw new ArgumentNullException("relativeTo");
      if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");

      Uri relativeToUri = new Uri(relativeTo);
      Uri pathUri = new Uri(path);

      if (relativeToUri.Scheme != pathUri.Scheme) { return path; } // path can't be made relative.

      Uri relativeUri = relativeToUri.MakeRelativeUri(pathUri);
      string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

      if (pathUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase)) {
        relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
      }

      return relativePath;
    }

    #endregion

    #region String Utils
    /// <summary>
    /// Trims a specific number of characters off of the end of the
    /// provided string.  When the number of trimmed characters is
    /// equal to or greater than the length of the string, the empty
    /// string is always returned.
    /// </summary>
    public static string TrimEnd(this string str, int characters) {
      return str.Substring(0, Mathf.Max(0, str.Length - characters));
    }

    /// <summary>
    /// Trims a specific number of characters off of the begining of
    /// the provided string.  When the number of trimmed characters is
    /// equal to or greater than the length of the string, the empty
    /// string is always returned.
    /// </summary>
    public static string TrimStart(this string str, int characters) {
      return str.Substring(Mathf.Min(str.Length, characters));
    }

    /// <summary>
    /// Capitalizes a simple string.  Only looks at the first character,
    /// so if your string has any kind of non-letter character as the first
    /// character this method will do nothing.
    /// </summary>
    public static string Capitalize(this string str) {
      char c = str[0];
      if (char.IsLetter(c)) {
        return char.ToUpper(c) + str.Substring(1);
      } else {
        return str;
      }
    }

    /// <summary>
    /// Takes a variable-like name and turns it into a nice human readable
    /// name.  Examples:
    /// 
    /// _privateVar     =>  Private Var
    /// multBy32        =>  Mult By 32
    /// the_key_code    =>  The Key Code
    /// CamelCaseToo    =>  Camel Case Too
    /// _is2_equalTo_5  =>  Is 2 Equal To 5
    /// GetTheSCUBANow  =>  Get The SCUBA Now
    /// m_privateVar    =>  Private Var
    /// kConstantVar    =>  Constant Var
    /// </summary>
    public static string GenerateNiceName(string value) {
      string result = "";
      string curr = "";

      Func<char, bool> wordFunc = c => {
        //Can't build any further if it's already capitalized
        if (curr.Length > 0 && char.IsUpper(curr[0])) {
          return false;
        }

        //Can't add non-letters to words
        if (!char.IsLetter(c)) {
          return false;
        }

        curr = c + curr;
        return true;
      };

      Func<char, bool> acronymFunc = c => {
        //Can't add non-letters to acronyms
        if (!char.IsLetter(c)) {
          return false;
        }

        //Can't add lowercase letters to acronyms
        if (char.IsLower(c)) {
          return false;
        }

        curr = c + curr;
        return true;
      };

      Func<char, bool> numberFunc = c => {
        //Can't add non-digits to a number
        if (!char.IsDigit(c)) {
          return false;
        }

        curr = c + curr;
        return true;
      };

      Func<char, bool> fluffFunc = c => {
        //Can't add digits or numbers to 'fluff'
        if (char.IsDigit(c) || char.IsLetter(c)) {
          return false;
        }

        return true;
      };


      Func<char, bool> currFunc = null;
      int currIndex = value.Length;

      while (currIndex != 0) {
        currIndex--;
        char c = value[currIndex];

        if (currFunc != null) {
          if (currFunc(c)) {
            continue;
          } else {
            currFunc = null;
          }
        }

        if (currFunc == null) {
          if (curr != "") {
            result = " " + curr.Capitalize() + result;
            curr = "";
          }

          if (acronymFunc(c)) {
            currFunc = acronymFunc;
          } else if (wordFunc(c)) {
            currFunc = wordFunc;
          } else if (numberFunc(c)) {
            currFunc = numberFunc;
          } else if (fluffFunc(c)) {
            currFunc = fluffFunc;
          } else {
            throw new Exception("Unexpected state, no function matched character " + c);
          }
        }
      }

      if (curr != "") {
        result = curr.Capitalize() + result;
      }

      result = result.Trim();

      if (result.StartsWith("M ") || result.StartsWith("K ")) {
        result = result.Substring(2);
      }

      return result.Trim();
    }
    #endregion

    #region Math Utils

    public static int Repeat(int x, int m) {
      int r = x % m;
      return r < 0 ? r + m : r;
    }

    /// <summary>
    /// Returns a vector that is perpendicular to this vector.
    /// The returned vector will have the same length as the
    /// input vector.
    /// </summary>
    public static Vector2 Perpendicular(this Vector2 vector) {
      return new Vector2(vector.y, -vector.x);
    }

    /// <summary>
    /// Returns a vector that is perpendicular to this vector.
    /// The returned vector is not guaranteed to be a unit vector,
    /// nor is its length guaranteed to be the same as the source
    /// vector's.
    /// </summary>
    public static Vector3 Perpendicular(this Vector3 vector) {
      float x2 = vector.x * vector.x;
      float y2 = vector.y * vector.y;
      float z2 = vector.z * vector.z;

      float mag0 = z2 + x2;
      float mag1 = y2 + x2;
      float mag2 = z2 + y2;

      if (mag0 > mag1) {
        if (mag0 > mag2) {
          return new Vector3(-vector.z, 0, vector.x);
        } else {
          return new Vector3(0, vector.z, -vector.y);
        }
      } else {
        if (mag1 > mag2) {
          return new Vector3(vector.y, -vector.x, 0);
        } else {
          return new Vector3(0, vector.z, -vector.y);
        }
      }
    }

    public static bool ContainsNaN(this Vector3 v) {
      return float.IsNaN(v.x)
          || float.IsNaN(v.y)
          || float.IsNaN(v.z);
    }

    public static bool IsBetween(this float f, float f0, float f1) {
      if (f0 > f1) Utils.Swap(ref f0, ref f1);

      return f0 <= f && f <= f1;
    }

    public static bool IsBetween(this double d, double d0, double d1) {
      if (d0 > d1) Utils.Swap(ref d0, ref d1);

      return d0 <= d && d <= d1;
    }

    /// <summary>
    /// Extrapolates using time values for positions a and b at extrapolatedTime.
    /// </summary>
    public static Vector3 TimedExtrapolate(Vector3 a, float aTime,
                                           Vector3 b, float bTime,
                                           float extrapolatedTime) {
      return Vector3.LerpUnclamped(a, b, extrapolatedTime.MapUnclamped(aTime, bTime, 0f, 1f));
    }

    /// <summary>
    /// Extrapolates using time values for rotations a and b at extrapolatedTime.
    /// </summary>
    public static Quaternion TimedExtrapolate(Quaternion a, float aTime,
                                              Quaternion b, float bTime,
                                              float extrapolatedTime) {
      return Quaternion.SlerpUnclamped(a, b, extrapolatedTime.MapUnclamped(aTime, bTime, 0f, 1f));
    }

    #endregion

    #region Value Mapping Utils

    /// <summary>
    /// Maps the value between valueMin and valueMax to its linearly proportional equivalent between resultMin and resultMax.
    /// The input value is clamped between valueMin and valueMax; if this is not desired, see MapUnclamped.
    /// </summary>
    public static float Map(this float value, float valueMin, float valueMax, float resultMin, float resultMax) {
      if (valueMin == valueMax) return resultMin;
      return Mathf.Lerp(resultMin, resultMax, ((value - valueMin) / (valueMax - valueMin)));
    }

    /// <summary>
    /// Maps the value between valueMin and valueMax to its linearly proportional equivalent between resultMin and resultMax,
    /// without clamping the result value between resultMin and resultMax.
    /// </summary>
    public static float MapUnclamped(this float value, float valueMin, float valueMax, float resultMin, float resultMax) {
      if (valueMin == valueMax) return resultMin;
      return Mathf.LerpUnclamped(resultMin, resultMax, ((value - valueMin) / (valueMax - valueMin)));
    }

    /// <summary>
    /// Maps each Vector2 component between valueMin and valueMax to its linearly proportional equivalent between resultMin and resultMax.
    /// The input values are clamped between valueMin and valueMax; if this is not desired, see MapUnclamped.
    /// </summary>
    public static Vector2 Map(this Vector2 value, float valueMin, float valueMax, float resultMin, float resultMax) {
      return new Vector2(value.x.Map(valueMin, valueMax, resultMin, resultMax),
                        value.y.Map(valueMin, valueMax, resultMin, resultMax));
    }

    /// <summary>
    /// Maps each Vector2 component between valueMin and valueMax to its linearly proportional equivalent between resultMin and resultMax,
    /// without clamping the result value between resultMin and resultMax.
    /// </summary>
    public static Vector2 MapUnclamped(this Vector2 value, float valueMin, float valueMax, float resultMin, float resultMax) {
      return new Vector2(value.x.MapUnclamped(valueMin, valueMax, resultMin, resultMax),
                        value.y.MapUnclamped(valueMin, valueMax, resultMin, resultMax));
    }

    /// <summary>
    /// Maps each Vector3 component between valueMin and valueMax to its linearly proportional equivalent between resultMin and resultMax.
    /// The input values are clamped between valueMin and valueMax; if this is not desired, see MapUnclamped.
    /// </summary>
    public static Vector3 Map(this Vector3 value, float valueMin, float valueMax, float resultMin, float resultMax) {
      return new Vector3(value.x.Map(valueMin, valueMax, resultMin, resultMax),
                        value.y.Map(valueMin, valueMax, resultMin, resultMax),
                        value.z.Map(valueMin, valueMax, resultMin, resultMax));
    }

    /// <summary>
    /// Maps each Vector3 component between valueMin and valueMax to its linearly proportional equivalent between resultMin and resultMax,
    /// without clamping the result value between resultMin and resultMax.
    /// </summary>
    public static Vector3 MapUnclamped(this Vector3 value, float valueMin, float valueMax, float resultMin, float resultMax) {
      return new Vector3(value.x.MapUnclamped(valueMin, valueMax, resultMin, resultMax),
                        value.y.MapUnclamped(valueMin, valueMax, resultMin, resultMax),
                        value.z.MapUnclamped(valueMin, valueMax, resultMin, resultMax));
    }

    /// <summary>
    /// Maps each Vector4 component between valueMin and valueMax to its linearly proportional equivalent between resultMin and resultMax.
    /// The input values are clamped between valueMin and valueMax; if this is not desired, see MapUnclamped.
    /// </summary>
    public static Vector4 Map(this Vector4 value, float valueMin, float valueMax, float resultMin, float resultMax) {
      return new Vector4(value.x.Map(valueMin, valueMax, resultMin, resultMax),
                        value.y.Map(valueMin, valueMax, resultMin, resultMax),
                        value.z.Map(valueMin, valueMax, resultMin, resultMax),
                        value.w.Map(valueMin, valueMax, resultMin, resultMax));
    }

    /// <summary>
    /// Maps each Vector4 component between valueMin and valueMax to its linearly proportional equivalent between resultMin and resultMax,
    /// without clamping the result value between resultMin and resultMax.
    /// </summary>
    public static Vector4 MapUnclamped(this Vector4 value, float valueMin, float valueMax, float resultMin, float resultMax) {
      return new Vector4(value.x.MapUnclamped(valueMin, valueMax, resultMin, resultMax),
                        value.y.MapUnclamped(valueMin, valueMax, resultMin, resultMax),
                        value.z.MapUnclamped(valueMin, valueMax, resultMin, resultMax),
                        value.w.MapUnclamped(valueMin, valueMax, resultMin, resultMax));
    }

    /// <summary>
    /// Returns a vector between resultMin and resultMax based on the input value's position
    /// between valueMin and valueMax.
    /// The input value is clamped between valueMin and valueMax.
    /// </summary>
    public static Vector2 Map(float input, float valueMin, float valueMax, Vector2 resultMin, Vector2 resultMax) {
      return Vector2.Lerp(resultMin, resultMax, Mathf.InverseLerp(valueMin, valueMax, input));
    }

    /// <summary>
    /// Returns a vector between resultMin and resultMax based on the input value's position
    /// between valueMin and valueMax.
    /// The input value is clamped between valueMin and valueMax.
    /// </summary>
    public static Vector3 Map(float input, float valueMin, float valueMax, Vector3 resultMin, Vector3 resultMax) {
      return Vector3.Lerp(resultMin, resultMax, Mathf.InverseLerp(valueMin, valueMax, input));
    }

    /// <summary>
    /// Returns a vector between resultMin and resultMax based on the input value's position
    /// between valueMin and valueMax.
    /// The input value is clamped between valueMin and valueMax.
    /// </summary>
    public static Vector4 Map(float input, float valueMin, float valueMax, Vector4 resultMin, Vector4 resultMax) {
      return Vector4.Lerp(resultMin, resultMax, Mathf.InverseLerp(valueMin, valueMax, input));
    }

    /// <summary>
    /// Returns a new Vector2 via component-wise multiplication.
    /// This operation is equivalent to Vector3.Scale(A, B).
    /// </summary>
    public static Vector2 CompMul(this Vector2 A, Vector2 B) {
      return new Vector2(A.x * B.x, A.y * B.y);
    }

    /// <summary>
    /// Returns a new Vector3 via component-wise multiplication.
    /// This operation is equivalent to Vector3.Scale(A, B).
    /// </summary>
    public static Vector3 CompMul(this Vector3 A, Vector3 B) {
      return new Vector3(A.x * B.x, A.y * B.y, A.z * B.z);
    }

    /// <summary>
    /// Returns a new Vector4 via component-wise multiplication.
    /// This operation is equivalent to Vector3.Scale(A, B).
    /// </summary>
    public static Vector4 CompMul(this Vector4 A, Vector4 B) {
      return new Vector4(A.x * B.x, A.y * B.y, A.z * B.z, A.w * B.w);
    }

    /// <summary>
    /// Returns a new Vector3 via component-wise division.
    /// This operation is the inverse of A.CompMul(B).
    /// </summary>
    public static Vector2 CompDiv(this Vector2 A, Vector2 B) {
      return new Vector2(A.x / B.x, A.y / B.y);
    }

    /// <summary>
    /// Returns a new Vector3 via component-wise division.
    /// This operation is the inverse of A.CompMul(B).
    /// </summary>
    public static Vector3 CompDiv(this Vector3 A, Vector3 B) {
      return new Vector3(A.x / B.x, A.y / B.y, A.z / B.z);
    }

    /// <summary>
    /// Returns a new Vector4 via component-wise division.
    /// This operation is the inverse of A.CompMul(B).
    /// </summary>
    public static Vector4 CompDiv(this Vector4 A, Vector4 B) {
      return new Vector4(A.x / B.x, A.y / B.y, A.z / B.z, A.w / B.w);
    }

    /// <summary>
    /// Returns the sum of the components of the input vector.
    /// </summary>
    public static float CompSum(this Vector2 v) {
      return v.x + v.y;
    }

    /// <summary>
    /// Returns the sum of the components of the input vector.
    /// </summary>
    public static float CompSum(this Vector3 v) {
      return v.x + v.y + v.z;
    }

    /// <summary>
    /// Returns the sum of the components of the input vector.
    /// </summary>
    public static float CompSum(this Vector4 v) {
      return v.x + v.y + v.z + v.w;
    }

    /// <summary>
    /// Returns the largest component of the input vector.
    /// </summary>
    public static float CompMax(this Vector2 v) {
      return Mathf.Max(v.x, v.y);
    }

    /// <summary>
    /// Returns the largest component of the input vector.
    /// </summary>
    public static float CompMax(this Vector3 v) {
      return Mathf.Max(Mathf.Max(v.x, v.y), v.z);
    }

    /// <summary>
    /// Returns the largest component of the input vector.
    /// </summary>
    public static float CompMax(this Vector4 v) {
      return Mathf.Max(Mathf.Max(Mathf.Max(v.x, v.y), v.z), v.w);
    }

    /// <summary>
    /// Returns the smallest component of the input vector.
    /// </summary>
    public static float CompMin(this Vector2 v) {
      return Mathf.Min(v.x, v.y);
    }

    /// <summary>
    /// Returns the smallest component of the input vector.
    /// </summary>
    public static float CompMin(this Vector3 v) {
      return Mathf.Min(Mathf.Min(v.x, v.y), v.z);
    }

    /// <summary>
    /// Returns the smallest component of the input vector.
    /// </summary>
    public static float CompMin(this Vector4 v) {
      return Mathf.Min(Mathf.Min(Mathf.Min(v.x, v.y), v.z), v.w);
    }

    #endregion

    #region Unity Object Utils

    /// <summary>
    /// Usage is the same as FindObjectOfType, but this method will also return objects
    /// that are inactive.
    /// 
    /// Use this method to search for singleton-pattern objects even if they are disabled,
    /// but be warned that it's not cheap to call!
    /// </summary>
    public static T FindObjectInHierarchy<T>() where T : UnityEngine.Object {
      return Resources.FindObjectsOfTypeAll<T>().Query()
        .Where(o => {
#if UNITY_EDITOR
          // Exclude prefabs.
          var prefabType = UnityEditor.PrefabUtility.GetPrefabType(o);
          if (prefabType == UnityEditor.PrefabType.ModelPrefab
          || prefabType == UnityEditor.PrefabType.Prefab) {
            return false;
          }
#endif
          return true;
        })
        .FirstOrDefault();
    }

    #endregion

    #region Transform Utils

    /// <summary>
    /// Returns the children of this Transform in sibling index order.
    /// </summary>
    public static ChildrenEnumerator GetChildren(this Transform t) {
      return new ChildrenEnumerator(t);
    }

    public struct ChildrenEnumerator : IEnumerator<Transform> {
      private Transform _t;
      private int _idx;
      private int _count;

      public ChildrenEnumerator(Transform t) {
        _t = t;
        _idx = -1;
        _count = t.childCount;
      }

      public ChildrenEnumerator GetEnumerator() { return this; }

      public bool MoveNext() {
        if (_idx < _count) _idx += 1;
        if (_idx == _count) { return false; } else { return true; }
      }
      public Transform Current {
        get { return _t == null ? null : _t.GetChild(_idx); }
      }
      object System.Collections.IEnumerator.Current { get { return Current; } }
      public void Reset() {
        _idx = -1;
        _count = _t.childCount;
      }
      public void Dispose() { }
    }

    #endregion

    #region Orientation Utils

    /// <summary>
    /// Similar to Unity's Transform.LookAt(), but resolves the forward vector of this
    /// Transform to point away from the argument Transform.
    /// 
    /// Useful for billboarding Quads and UI elements whose forward vectors should match
    /// rather than oppose the Main Camera's forward vector.
    /// 
    /// Optionally, you may also pass an upwards vector, which will be provided to the underlying
    /// Quaternion.LookRotation. Vector3.up will be used by default.
    /// </summary>
    public static void LookAwayFrom(this Transform thisTransform, Transform transform) {
      thisTransform.rotation = Quaternion.LookRotation(thisTransform.position - transform.position, Vector3.up);
    }

    /// <summary>
    /// Similar to Unity's Transform.LookAt(), but resolves the forward vector of this
    /// Transform to point away from the argument Transform.
    /// 
    /// Allows specifying an upwards parameter; this is passed as the upwards vector to the Quaternion.LookRotation.
    /// </summary>
    /// <param name="thisTransform"></param>
    /// <param name="transform"></param>
    public static void LookAwayFrom(this Transform thisTransform, Transform transform, Vector3 upwards) {
      thisTransform.rotation = Quaternion.LookRotation(thisTransform.position - transform.position, upwards);
    }

    #endregion

    #region Physics Utils

    public static void IgnoreCollisions(GameObject first, GameObject second, bool ignore = true) {
      if (first == null || second == null)
        return;

      Collider[] first_colliders = first.GetComponentsInChildren<Collider>();
      Collider[] second_colliders = second.GetComponentsInChildren<Collider>();

      for (int i = 0; i < first_colliders.Length; ++i) {
        for (int j = 0; j < second_colliders.Length; ++j) {
          if (first_colliders[i] != second_colliders[j] &&
              first_colliders[i].enabled && second_colliders[j].enabled) {
            Physics.IgnoreCollision(first_colliders[i], second_colliders[j], ignore);
          }
        }
      }
    }

    #endregion

    #region Collider Utils

    public static Vector3 GetDirection(this CapsuleCollider capsule) {
      switch (capsule.direction) {
        case 0: return Vector3.right;
        case 1: return Vector3.up;
        case 2: default: return Vector3.forward;
      }
    }

    public static void GetCapsulePoints(this CapsuleCollider capsule, out Vector3 a,
                                                                      out Vector3 b) {
      a = capsule.GetDirection() * ((capsule.height * 0.5f) - capsule.radius);
      b = -a;

      a = capsule.transform.TransformPoint(a);
      b = capsule.transform.TransformPoint(b);
    }

    /// <summary>
    /// Manipulates capsule.transform.position, capsule.transform.rotation, and capsule.height
    /// so that the line segment defined by the capsule connects world-space points a and b.
    /// </summary>
    public static void SetCapsulePoints(this CapsuleCollider capsule, Vector3 a, Vector3 b) {
      capsule.center = Vector3.zero;

      capsule.transform.position = (a + b) / 2F;

      Vector3 capsuleDirection = capsule.GetDirection();

      Vector3 capsuleDirWorldSpace = capsule.transform.TransformDirection(capsuleDirection);
      Quaternion necessaryRotation = Quaternion.FromToRotation(capsuleDirWorldSpace, a - capsule.transform.position);
      capsule.transform.rotation = necessaryRotation * capsule.transform.rotation;

      Vector3 aCapsuleSpace = capsule.transform.InverseTransformPoint(a);
      float capsuleSpaceDistToA = aCapsuleSpace.magnitude;
      capsule.height = (capsuleSpaceDistToA + capsule.radius) * 2;
    }

    /// <summary>
    /// Recursively searches the hierarchy of the argument GameObject to find all of the
    /// Colliders that are attached to the object's Rigidbody (or that _would_ be
    /// attached to its Rigidbody if it doesn't have one) and adds them to the provided
    /// colliders list. Warning: The provided "colliders" List will be cleared before
    /// use.
    ///
    /// Colliders that are the children of other Rigidbody elements beneath the argument
    /// object are ignored. Optionally, colliders of inactive GameObjects can be included
    /// in the returned list; by default, these colliders are skipped.
    /// </summary>
    public static void FindColliders<T>(GameObject obj, List<T> colliders,
                                        bool includeInactiveObjects = false)
                                    where T : Collider {
      colliders.Clear();
      Stack<Transform> toVisit = Pool<Stack<Transform>>.Spawn();
      List<T> collidersBuffer = Pool<List<T>>.Spawn();

      try {
        // Traverse the hierarchy of this object's transform to find
        // all of its Colliders.
        toVisit.Push(obj.transform);
        Transform curTransform;
        while (toVisit.Count > 0) {
          curTransform = toVisit.Pop();

          // Recursively search children and children's children
          foreach (var child in curTransform.GetChildren()) {
            // Ignore children with Rigidbodies of their own; its own Rigidbody
            // owns its own colliders and the colliders of its children
            if (child.GetComponent<Rigidbody>() == null
                && (includeInactiveObjects || child.gameObject.activeSelf)) {
              toVisit.Push(child);
            }
          }

          // Since we'll visit every valid child, all we need to do is add the colliders
          // of every transform we visit.
          collidersBuffer.Clear();
          curTransform.GetComponents<T>(collidersBuffer);
          foreach (var collider in collidersBuffer) {
            colliders.Add(collider);
          }
        }
      } finally {
        toVisit.Clear();
        Pool<Stack<Transform>>.Recycle(toVisit);

        collidersBuffer.Clear();
        Pool<List<T>>.Recycle(collidersBuffer);
      }
    }

    #endregion

    #region Color Utils

    public static Color WithAlpha(this Color color, float alpha) {
      return new Color(color.r, color.g, color.b, alpha);
    }

    /// <summary>
    /// Just like ColorUtility.TryParseHtmlString but throws a useful
    /// error message if it fails.
    /// </summary>
    public static Color ParseHtmlColorString(string htmlString) {
      Color color;
      if (!ColorUtility.TryParseHtmlString(htmlString, out color)) {
        throw new ArgumentException("The string [" + htmlString + "] is not a valid color code.  Valid color codes include:\n" +
                                    "#RGB\n" +
                                    "#RGBA\n" +
                                    "#RRGGBB\n" +
                                    "#RRGGBBAA\n" +
                                    "For more information, see the documentation for ColorUtility.TryParseHtmlString.");
      }

      return color;
    }

    /// <summary>
    /// Lerps this color towards the argument color in HSV space and returns the lerped
    /// color.
    /// </summary>
    public static Color LerpHSV(this Color color, Color towardsColor, float t) {
      float h0, s0, v0;
      Color.RGBToHSV(color, out h0, out s0, out v0);

      float h1, s1, v1;
      Color.RGBToHSV(towardsColor, out h1, out s1, out v1);

      // Cyclically lerp hue. (Input hues are always between 0 and 1.)
      if (h0 - h1 < -0.5f) h0 += 1f;
      if (h0 - h1 > 0.5f) h1 += 1f;
      float hL = Mathf.Lerp(h0, h1, t) % 1f;

      float sL = Mathf.Lerp(s0, s1, t);
      float vL = Mathf.Lerp(v0, v1, t);
      return Color.HSVToRGB(hL, sL, vL);
    }

    /// <summary>
    /// Cyclically lerps hue arguments by t.
    /// </summary>
    public static float LerpHue(float h0, float h1, float t) {
      // Enforce hue values between 0f and 1f.
      if (h0 < 0f) h0 = 1f - (-h0 % 1f);
      if (h1 < 0f) h1 = 1f - (-h1 % 1f);
      if (h0 > 1f) h0 = h0 % 1f;
      if (h1 > 1f) h1 = h1 % 1f;

      if (h0 - h1 < -0.5f) h0 += 1f;
      if (h0 - h1 > 0.5f) h1 += 1f;
      return Mathf.Lerp(h0, h1, t) % 1f;
    }

    #endregion

    #region Gizmo Utils

    public static void DrawCircle(Vector3 center,
                           Vector3 normal,
                           float radius,
                           Color color,
                           int quality = 32,
                           float duration = 0,
                           bool depthTest = true) {
      Vector3 planeA = Vector3.Slerp(normal, -normal, 0.5f);
      DrawArc(360, center, planeA, normal, radius, color, quality);
    }

    /* Adapted from: Zarrax (http://math.stackexchange.com/users/3035/zarrax), Parametric Equation of a Circle in 3D Space?, 
     * URL (version: 2014-09-09): http://math.stackexchange.com/q/73242 */
    public static void DrawArc(float arc,
                           Vector3 center,
                           Vector3 forward,
                           Vector3 normal,
                           float radius,
                           Color color,
                           int quality = 32) {

      Gizmos.color = color;
      Vector3 right = Vector3.Cross(normal, forward).normalized;
      float deltaAngle = arc / quality;
      Vector3 thisPoint = center + forward * radius;
      Vector3 nextPoint = new Vector3();
      for (float angle = 0; Mathf.Abs(angle) <= Mathf.Abs(arc); angle += deltaAngle) {
        float cosAngle = Mathf.Cos(angle * Constants.DEG_TO_RAD);
        float sinAngle = Mathf.Sin(angle * Constants.DEG_TO_RAD);
        nextPoint.x = center.x + radius * (cosAngle * forward.x + sinAngle * right.x);
        nextPoint.y = center.y + radius * (cosAngle * forward.y + sinAngle * right.y);
        nextPoint.z = center.z + radius * (cosAngle * forward.z + sinAngle * right.z);
        Gizmos.DrawLine(thisPoint, nextPoint);
        thisPoint = nextPoint;
      }
    }

    public static void DrawCone(Vector3 origin,
                           Vector3 direction,
                           float angle,
                           float height,
                           Color color,
                           int quality = 4,
                           float duration = 0,
                           bool depthTest = true) {

      float step = height / quality;
      for (float q = step; q <= height; q += step) {
        DrawCircle(origin + direction * q, direction, Mathf.Tan(angle * Constants.DEG_TO_RAD) * q, color, quality * 8, duration, depthTest);
      }
    }

    #endregion

    #region Texture Utils

    private static TextureFormat[] _incompressibleFormats = new TextureFormat[] {
      TextureFormat.R16,
      TextureFormat.EAC_R,
      TextureFormat.EAC_R_SIGNED,
      TextureFormat.EAC_RG,
      TextureFormat.EAC_RG_SIGNED,
      TextureFormat.ETC_RGB4_3DS,
      TextureFormat.ETC_RGBA8_3DS
    };

    /// <summary>
    /// Returns whether or not the given format is a valid input to EditorUtility.CompressTexture();
    /// </summary>
    public static bool IsCompressible(TextureFormat format) {
      if (format < 0) {
        return false;
      }

      return Array.IndexOf(_incompressibleFormats, format) < 0;
    }

    #endregion

    #region Rect Utils

    /// <summary>
    /// Returns the area of the Rect, width * height.
    /// </summary>
    public static float Area(this Rect rect) {
      return rect.width * rect.height;
    }

    /// <summary>
    /// Returns a new Rect with the argument padding as a margin relative to each
    /// border of the provided Rect.
    /// </summary>
    public static Rect PadInner(this Rect r, float padding) {
      return new Rect(r.x + padding, r.y + padding, r.width - (padding * 2), r.height - (padding * 2));
    }

    #endregion

    #region List Utils

    public static void EnsureListExists<T>(ref List<T> list) {
      if (list == null) {
        list = new List<T>();
      }
    }

    public static void EnsureListCount<T>(this List<T> list, int count) {
      if (list.Count == count) return;

      while (list.Count < count) {
        list.Add(default(T));
      }

      while (list.Count > count) {
        list.RemoveAt(list.Count - 1);
      }
    }

    public static void EnsureListCount<T>(this List<T> list, int count, Func<T> createT, Action<T> deleteT = null) {
      while (list.Count < count) {
        list.Add(createT());
      }

      while (list.Count > count) {
        T tempT = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);

        if (deleteT != null) {
          deleteT(tempT);
        }
      }
    }

    #endregion

  }

}