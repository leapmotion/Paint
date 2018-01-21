using Leap.Unity;
using Leap.Unity.PhysicalInterfaces;
using Leap.Unity.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  public static class NewUtils {


    #region Transform Utils

    /// <summary>
    /// Returns a list of transforms including this transform and ALL of its children,
    /// including the children of its children, and the children of their children, and
    /// so on.
    /// 
    /// THIS ALLOCATES GARBAGE. Use it for editor code only.
    /// </summary>
    public static List<Transform> GetSelfAndAllChildren(this Transform t,
                                                       bool includeInactiveObjects = false) {
      var allChildren = new List<Transform>();

      Stack<Transform> toVisit = Pool<Stack<Transform>>.Spawn();

      try {
        // Traverse the hierarchy of this object's transform to find all of its Colliders.
        toVisit.Push(t.transform);
        Transform curTransform;
        while (toVisit.Count > 0) {
          curTransform = toVisit.Pop();

          // Recursively search children and children's children
          foreach (var child in curTransform.GetChildren()) {
            // Ignore children with Rigidbodies of their own; its own Rigidbody
            // owns its own colliders and the colliders of its children
            if (includeInactiveObjects || child.gameObject.activeSelf) {
              toVisit.Push(child);
            }
          }

          // Since we'll visit every valid child, all we need to do is add the colliders
          // of every transform we visit.
          allChildren.Add(curTransform);
        }
      }
      finally {
        toVisit.Clear();
        Pool<Stack<Transform>>.Recycle(toVisit);
      }

      return allChildren;
    }


    /// <summary>
    /// Recursively searches the hierarchy of the argument Transform to find all of the
    /// Components of type ComponentType (the first type argument) that should be "owned"
    /// by the OwnerType component type (the second type argument).
    /// 
    /// If a child GameObject itself has an OwnerType component, that
    /// child is ignored, and its children are ignored -- the assumption being that such
    /// a child owns itself and any ComponentType components beneath it.
    /// 
    /// For example, a call to FindOwnedChildComponents with ComponentType Collider and
    /// OwnerType Rigidbody would return all of the Colliders that are attached to the
    /// rootObj Rigidbody, but none of the colliders that are attached to a rootObj's
    /// child's own Rigidbody.
    /// 
    /// Optionally, ComponentType components of inactive GameObjects can be included
    /// in the returned list; by default, these components are skipped.
    /// 
    /// This is not a cheap method to call, but it does not allocate garbage, so it is safe
    /// for use at runtime.
    /// </summary>
    /// 
    /// <typeparam name="ComponentType">
    /// The component type to search for.
    /// </typeparam>
    /// 
    /// <typeparam name="OwnerType">
    /// The component type that assumes ownership of any ComponentType in its own Transform
    /// or its Transform's children/grandchildren.
    /// </typeparam>
    public static void FindOwnedChildComponents<ComponentType, OwnerType>
                                                 (OwnerType rootObj,
                                                  List<ComponentType> ownedComponents,
                                                  bool includeInactiveObjects = false)
                                               where OwnerType : Component {
      ownedComponents.Clear();
      Stack<Transform> toVisit = Pool<Stack<Transform>>.Spawn();
      List<ComponentType> componentsBuffer = Pool<List<ComponentType>>.Spawn();

      try {
        // Traverse the hierarchy of this object's transform to find
        // all of its Colliders.
        toVisit.Push(rootObj.transform);
        Transform curTransform;
        while (toVisit.Count > 0) {
          curTransform = toVisit.Pop();

          // Recursively search children and children's children.
          foreach (var child in curTransform.GetChildren()) {
            // Ignore children with OwnerType components of their own; its own OwnerType
            // component owns its own ComponentType components and the ComponentType
            // components of its children.
            if (child.GetComponent<OwnerType>() == null
                && (includeInactiveObjects || child.gameObject.activeSelf)) {
              toVisit.Push(child);
            }
          }

          // Since we'll visit every valid child, all we need to do is add the
          // ComponentType components of every transform we visit.
          componentsBuffer.Clear();
          curTransform.GetComponents<ComponentType>(componentsBuffer);
          foreach (var component in componentsBuffer) {
            ownedComponents.Add(component);
          }
        }
      }
      finally {
        toVisit.Clear();
        Pool<Stack<Transform>>.Recycle(toVisit);

        componentsBuffer.Clear();
        Pool<List<ComponentType>>.Recycle(componentsBuffer);
      }
    }

    /// <summary>
    /// Sets the localPosition, localRotation, and localScale to their default values:
    /// Vector3.zero, Quaternion.identity, and Vector3.one.
    /// </summary>
    public static void ResetLocalTransform(this Transform t) {
      t.localPosition = Vector3.zero;
      t.localRotation = Quaternion.identity;
      t.localScale = Vector3.one;
    }

    /// <summary>
    /// Sets the localPosition and localRotation of this Transform to Vector3.zero and
    /// Quaternion.identity. Doesn't affect localScale.
    /// </summary>
    public static void ResetLocalPose(this Transform t) {
      t.localPosition = Vector3.zero;
      t.localRotation = Quaternion.identity;
    }

    #endregion

    #region Rect Utils

    public static Rect PadOuter(this Rect r, float width) {
      return new Rect(r.x - width, r.y - width,
                      r.width + (width * 2f), r.height + (width * 2f));
    }

    /// <summary>
    /// Returns a horizontal strip of lineHeight of this rect (from the top by default) and
    /// provides what's left of this rect after the line is removed as theRest.
    /// </summary>
    public static Rect TakeHorizontal(this Rect r, float lineHeight,
                                out Rect theRest,
                                bool fromTop = true) {
      theRest = new Rect(r.x, (fromTop ? r.y + lineHeight : r.y), r.width, r.height - lineHeight);
      return new Rect(r.x, (fromTop ? r.y : r.y + r.height - lineHeight), r.width, lineHeight);
    }

    #endregion

    #region Vector3 Utils

    /// <summary>
    /// Returns this position moved towards the argument position, up to but no more than
    /// the max movement amount from the original position.
    /// </summary>
    public static Vector3 MovedTowards(this Vector3 thisPosition,
                                      Vector3 otherPosition,
                                      float maxMovementAmount) {
      var delta = thisPosition - otherPosition;
      if (delta.sqrMagnitude > maxMovementAmount * maxMovementAmount) {
        delta = Vector3.ClampMagnitude(delta, maxMovementAmount);
      }
      return thisPosition + delta;
    }

    public static Vector3 ToVector3(this Vector4 v4) {
      return new Vector3(v4.x, v4.y, v4.z);
    }

    #endregion

    #region Pose Utils

    /// <summary>
    /// Returns a pose such that fromPose.Then(thisPose) will have this position
    /// and the fromPose's rotation.
    /// </summary>
    public static Pose From(this Vector3 position, Pose fromPose) {
      return new Pose(position, fromPose.rotation).From(fromPose);
    }

    public static Pose GetPose(this Rigidbody rigidbody) {
      return new Pose(rigidbody.position, rigidbody.rotation);
    }

    /// <summary>
    /// Returns a Pose that has its position and rotation mirrored on the X axis.
    /// </summary>
    public static Pose MirroredX(this Pose pose) {
      var v = pose.position;
      var q = pose.rotation;
      return new Pose(new Vector3(-v.x, v.y, v.z),
                      new Quaternion(-q.x, q.y, q.z, -q.w).Flipped());
    }

    /// <summary>
    /// Returns a Pose that has its position and rotation flipped.
    /// </summary>
    public static Pose Negated(this Pose pose) {
      var v = pose.position;
      var q = pose.rotation;
      return new Pose(new Vector3(-v.x, -v.y, -v.z),
                      new Quaternion(-q.z, -q.y, -q.z, q.w));
    }

    #endregion

    #region Rect Utils

    public static void SplitHorizontallyWithLeft(this Rect rect, out Rect left, out Rect right, float leftWidth) {
      left = rect;
      left.width = leftWidth;
      right = rect;
      right.x += left.width;
      right.width = rect.width - leftWidth;
    }

    #endregion

    #region Array Utils

    /// <summary>
    /// Sets all elements in the array of type T to default(T).
    /// </summary>
    public static T[] ClearWithDefaults<T>(this T[] arr) {
      for (int i = 0; i < arr.Length; i++) {
        arr[i] = default(T);
      }
      return arr;
    }

    /// <summary>
    /// Sets all elements in the array of type T to the argument value.
    /// </summary>
    public static T[] ClearWith<T>(this T[] arr, T value) {
      for (int i = 0; i < arr.Length; i++) {
        arr[i] = value;
      }
      return arr;
    }

    public static IIndexable<T> ToIndexable<T>(this T[] arr) {
      return new ArrayIndexable<T>(arr);
    }

    public struct ArrayIndexable<T> : IIndexable<T> {
      private T[] _arr;

      public ArrayIndexable(T[] arr) {
        _arr = arr;
      }

      public T this[int idx] { get { return _arr[idx]; } }

      public int Count { get { return _arr.Length; } }
    }

    #endregion

    #region Geometry Utils

    #region Plane & Rect Clamping

    public static Vector3 GetLocalPlanePosition(this Vector3 worldPosition, Pose planePose) {
      var localSpacePoint = worldPosition.From(planePose).position;
      return localSpacePoint;
    }

    public static Vector3 ClampedToPlane(this Vector3 worldPosition, Pose planePose) {
      var localSpacePoint = worldPosition.From(planePose).position;

      var localSpaceOnPlane = new Vector3(localSpacePoint.x,
                                        localSpacePoint.y, 0f);

      return (planePose * localSpaceOnPlane).position;
    }

    public static Vector3 ClampedToRect(this Vector3 worldPosition, Pose rectCenterPose,
                                        float rectWidth, float rectHeight) {
      bool unused;
      return worldPosition.ClampedToRect(rectCenterPose, rectWidth, rectHeight, out unused);
    }

    public static Vector3 ClampedToRect(this Vector3 worldPosition, Pose rectCenterPose,
                                        float rectWidth, float rectHeight,
                                        out bool isProjectionWithinRect) {
      var localSpacePoint = worldPosition.From(rectCenterPose).position;

      var localSpaceOnPlane = new Vector3(Mathf.Clamp(localSpacePoint.x, -rectWidth,  rectWidth),
                                        Mathf.Clamp(localSpacePoint.y, -rectHeight, rectHeight), 0f);

      isProjectionWithinRect = Mathf.Abs(localSpacePoint.x) <= rectWidth;
      isProjectionWithinRect &= Mathf.Abs(localSpacePoint.y) <= rectHeight;

      return (rectCenterPose * localSpaceOnPlane).position;
    }

    public static Vector3 ClampedToRect(this Vector3 worldPosition, Pose rectCenterPose,
                                        float rectWidth, float rectHeight,
                                        out float sqrDistToRect,
                                        out bool isProjectionInRect) {
      var localSpacePoint = worldPosition.From(rectCenterPose).position;

      isProjectionInRect = Mathf.Abs(localSpacePoint.x) <= rectWidth / 2f;
      isProjectionInRect &= Mathf.Abs(localSpacePoint.y) <= rectHeight / 2f;

      var localSpaceOnPlane = new Vector3(Mathf.Clamp(localSpacePoint.x, -rectWidth / 2f,  rectWidth / 2f),
                                        Mathf.Clamp(localSpacePoint.y, -rectHeight / 2f, rectHeight / 2f), 0f);

      var positionOnRect = (rectCenterPose * localSpaceOnPlane).position;

      sqrDistToRect = (positionOnRect - worldPosition).sqrMagnitude;

      return positionOnRect;
    }

    #endregion

    #endregion

    #region Vector3 Utils

    /// <summary>
    /// Returns this vector converted from world space to the local space of the argument Transform.
    /// </summary>
    public static Vector3 InLocalSpace(this Vector3 v, Transform t) {
      return t.InverseTransformPoint(v);
    }

    public static float Dot(this Vector3 a, Vector3 b) {
      return Vector3.Dot(a, b);
    }

    public static float Angle(this Vector3 a, Vector3 b) {
      return Vector3.Angle(a, b);
    }

    public static float SignedAngle(this Vector3 a, Vector3 b, Vector3 axis) {
      return Vector3.SignedAngle(a, b, axis);
    }

    #endregion

    #region Float Utils

    public static float Clamped01(this float f) {
      return Mathf.Clamp01(f);
    }

    public static float Clamped(this float f, float min, float max) {
      return Mathf.Clamp(f, min, max);
    }

    #endregion

    #region Matrix Utils

    public static Matrix4x4 CompMul(Matrix4x4 m, float f) {
      return new Matrix4x4(m.GetColumn(0) * f, m.GetColumn(1) * f, m.GetColumn(2) * f, m.GetColumn(3) * f);
    }

    #endregion

    #region RuntimeGizmoDrawer Utils

    public static void DrawPose(this RuntimeGizmos.RuntimeGizmoDrawer drawer,
                                Pose pose, float radius = 0.10f) {
      drawer.PushMatrix();

      drawer.matrix = Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one);

      var origColor = drawer.color;

      drawer.DrawWireSphere(Vector3.zero, radius);
      drawer.DrawPosition(Vector3.zero, radius * 2);

      drawer.color = origColor;

      drawer.PopMatrix();
    }

    public static void DrawRay(this RuntimeGizmos.RuntimeGizmoDrawer drawer,
                               Vector3 position, Vector3 direction) {
      drawer.DrawLine(position, position + direction);
    }

    #endregion

    #region Quaternion Utils

    public static Vector3 GetRight(this Quaternion q) {
      return q * Vector3.right;
    }

    public static Vector3 GetUp(this Quaternion q) {
      return q * Vector3.up;
    }

    public static Vector3 GetForward(this Quaternion q) {
      return q * Vector3.forward;
    }

    public static Quaternion Flipped(this Quaternion q) {
      return new Quaternion(-q.x, -q.y, -q.z, -q.w);
    }

    #endregion

  }

  #region Grids

  public struct GridPoint {
    public int x, y;
    public Vector3 rootPos;
    public float cellWidth, cellHeight;

    public Vector3 centerPos { get { return rootPos + new Vector3(cellWidth / 2f, -cellHeight / 2f); } }

    public int gridId;
  }

  public struct GridPointEnumerator {

    public Vector2 size;
    public int numRows, numCols;
    public Matrix4x4 transform;

    private int _index;
    private Vector2 _cellSize;

    public GridPointEnumerator(Vector2 size, int numRows, int numCols) {
      if (numRows < 1) numRows = 1;
      if (numCols < 1) numCols = 1;

      this.size = size;
      this.numRows = numRows;
      this.numCols = numCols;

      this.transform = Matrix4x4.identity;

      this._index = -1;
      _cellSize = new Vector2(size.x / numCols, size.y / numRows);
    }

    public GridPointEnumerator GetEnumerator() { return this; }

    private int maxIndex { get { return numRows * numCols - 1; } }

    public bool MoveNext() {
      _index += 1;
      return _index <= maxIndex;
    }

    public GridPoint Current {
      get {
        var x = _index % numCols;
        var y = _index / numCols;
        var pos = transform * (new Vector3(-(size.x / 2) + _cellSize.x * x,
                                           (size.y / 2) - _cellSize.y * y));
        return new GridPoint() {
          x = x,
          y = y,
          rootPos = pos,
          gridId = _index,
          cellWidth = _cellSize.x,
          cellHeight = _cellSize.y
        };
      }
    }

  }

  #endregion

}

