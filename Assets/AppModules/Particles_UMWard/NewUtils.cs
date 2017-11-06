using Leap.Unity;
using Leap.Unity.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

  #endregion


  #region Quaternion Utils

  /// <summary>
  /// Converts the quaternion into an axis and an angle and returns the vector
  /// axis * angle.
  /// </summary>
  public static Vector3 ToAngleAxisVector(this Quaternion q) {
    float angle;
    Vector3 axis;
    q.ToAngleAxis(out angle, out axis);
    return axis * angle;
  }

  /// <summary>
  /// Returns a Quaternion described by the provided angle axis vector.
  /// </summary>
  public static Quaternion QuaternionFromAngleAxisVector(Vector3 angleAxisVector) {
    if (angleAxisVector == Vector3.zero) return Quaternion.identity;
    return Quaternion.AngleAxis(angleAxisVector.magnitude, angleAxisVector.normalized);
  }

  public static Quaternion From(this Quaternion thisQuaternion, Quaternion otherQuaternion) {
    return thisQuaternion * Quaternion.Inverse(otherQuaternion);
  }

  public static Quaternion Then(this Quaternion thisQuaternion, Quaternion otherQuaternion) {
    return otherQuaternion * thisQuaternion;
  }

  #endregion


  #region Rect Utils

  public static Rect PadOuter(this Rect r, float width) {
    return new Rect(r.x - width, r.y - width,
                    r.width + (width * 2f), r.height + (width * 2f));
  }

  #region Pad, No Out

  public static Rect PadTop(this Rect r, float padding) {
    return new Rect(r.x, r.y + padding, r.width, r.height - padding);
  }

  public static Rect PadBottom(this Rect r, float padding) {
    return new Rect(r.x, r.y, r.width, r.height - padding);
  }

  public static Rect PadLeft(this Rect r, float padding) {
    return new Rect(r.x + padding, r.y, r.width - padding, r.height);
  }

  public static Rect PadRight(this Rect r, float padding) {
    return new Rect(r.x, r.y, r.width - padding, r.height);
  }

  #endregion

  #region Pad, With Out

  /// <summary>
  /// Returns the Rect if padded on the top by the padding amount, and optionally
  /// outputs the remaining margin into marginRect.
  /// </summary>
  public static Rect PadTop(this Rect r, float padding, out Rect marginRect) {
    marginRect = new Rect(r.x, r.y, r.width, padding);
    return PadTop(r, padding);
  }

  /// <summary>
  /// Returns the Rect if padded on the bottom by the padding amount, and optionally
  /// outputs the remaining margin into marginRect.
  /// </summary>
  public static Rect PadBottom(this Rect r, float padding, out Rect marginRect) {
    marginRect = new Rect(r.x, r.y + r.height - padding, padding, r.height);
    return PadBottom(r, padding);
  }

  /// <summary>
  /// Returns the Rect if padded on the left by the padding amount, and optionally
  /// outputs the remaining margin into marginRect.
  /// </summary>
  public static Rect PadLeft(this Rect r, float padding, out Rect marginRect) {
    marginRect = new Rect(r.x, r.y, padding, r.height);
    return PadLeft(r, padding);
  }

  /// <summary>
  /// Returns the Rect if padded on the right by the padding amount, and optionally
  /// outputs the remaining margin into marginRect.
  /// </summary>
  public static Rect PadRight(this Rect r, float padding, out Rect marginRect) {
    marginRect = new Rect(r.x + r.width - padding, r.y, padding, r.height);
    return PadRight(r, padding);
  }

  #endregion

  #region Pad Percent, Two Sides

  public static Rect PadTopBottomPercent(this Rect r, float padPercent) {
    float padHeight = r.height * padPercent;
    return new Rect(r.x, r.y + padHeight, r.width, r.height - padHeight * 2f);
  }

  public static Rect PadLeftRightPercent(this Rect r, float padPercent) {
    float padWidth = r.width * padPercent;
    return new Rect(r.x + padWidth, r.y, r.width - padWidth * 2f, r.height);
  }

  #endregion

  #region Pad Percent

  public static Rect PadTopPercent(this Rect r, float padPercent) {
    float padHeight = r.height * padPercent;
    return PadTop(r, padHeight);
  }

  public static Rect PadBottomPercent(this Rect r, float padPercent) {
    float padHeight = r.height * padPercent;
    return PadBottom(r, padHeight);
  }

  public static Rect PadLeftPercent(this Rect r, float padPercent) {
    return PadLeft(r, r.width * padPercent);
  }

  public static Rect PadRightPercent(this Rect r, float padPercent) {
    return PadRight(r, r.width * padPercent);
  }

  #endregion

  #region Take, No Out

  /// <summary>
  /// Return a margin of the given width on the left side of the input Rect.
  /// <summary>
  public static Rect TakeLeft(this Rect r, float widthFromLeft) {
    Rect theRest;
    return TakeLeft(r, widthFromLeft, out theRest);
  }

  /// <summary>
  /// Return a margin of the given width on the left side of the input Rect.
  /// <summary>
  public static Rect TakeRight(this Rect r, float widthFromRight) {
    Rect theRest;
    return TakeRight(r, widthFromRight, out theRest);
  }

  #endregion

  #region Take, With Out
  
  /// <summary>
  /// Return a margin of the given width on the left side of the input Rect, and
  /// optionally outputs the rest of the Rect into theRest.
  /// <summary>
  public static Rect TakeLeft(this Rect r, float padWidth, out Rect theRest) {
    Rect thePadding;
    theRest = PadLeft(r, padWidth, out thePadding);
    return thePadding;
  }

  /// <summary>
  /// Return a margin of the given width on the right side of the input Rect, and
  /// optionally outputs the rest of the Rect into theRest.
  /// <summary>
  public static Rect TakeRight(this Rect r, float padWidth, out Rect theRest) {
    Rect thePadding;
    theRest = PadRight(r, padWidth, out thePadding);
    return thePadding;
  }

  #endregion

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

  #region Enumerators

  /// <summary>
  /// Slices numLines horizontal line Rects from this Rect and returns an enumerator that
  /// will return each line Rect.
  /// 
  /// The height of each line is the height of the Rect divided by the number of lines
  /// requested.
  /// </summary>
  public static HorizontalLineRectEnumerator TakeAllLines(this Rect r, int numLines) {
    return new HorizontalLineRectEnumerator(r, numLines);
  }

  public struct HorizontalLineRectEnumerator : IQueryOp<Rect> {
    Rect rect;
    int numLines;
    int index;

    public HorizontalLineRectEnumerator(Rect rect, int numLines) {
      this.rect = rect;
      this.numLines = numLines;
      this.index = -1;
    }

    public float eachHeight { get { return this.rect.height / numLines; } }

    public Rect Current {
      get { return new Rect(rect.x, rect.y + eachHeight * index, rect.width, eachHeight); }
    }
    public bool MoveNext() {
      index += 1;
      return index < numLines;
    }
    public HorizontalLineRectEnumerator GetEnumerator() { return this; }

    public bool TryGetNext(out Rect t) {
      if (MoveNext()) {
        t = Current; return true;
      }
      else {
        t = default(Rect); return false;
      }
    }

    public void Reset() {
      index = -1;
    }

    public QueryWrapper<Rect, HorizontalLineRectEnumerator> Query() {
      return new QueryWrapper<Rect, HorizontalLineRectEnumerator>(this);
    }
  }

  #endregion

  #endregion


  #region Pose Utils

  /// <summary>
  /// Returns the rotation that makes a transform at objectPosition point its forward
  /// vector at targetPosition and keep its rightward vector parallel with the horizon
  /// defined by a normal of Vector3.up.
  /// 
  /// For example, this will point an interface panel at a user camera while
  /// maintaining the alignment of text and other elements with the horizon line.
  /// </summary>
  /// <returns></returns>
  public static Quaternion FaceTargetWithoutTwist(Vector3 fromPosition, Vector3 targetPosition, bool flip180 = false) {
    return FaceTargetWithoutTwist(fromPosition, targetPosition, Vector3.up, flip180);
  }

  /// <summary>
  /// Returns the rotation that makes a transform at objectPosition point its forward
  /// vector at targetPosition and keep its rightward vector parallel with the horizon
  /// defined by the upwardDirection normal.
  /// 
  /// For example, this will point an interface panel at a user camera while
  /// maintaining the alignment of text and other elements with the horizon line.
  /// </summary>
  public static Quaternion FaceTargetWithoutTwist(Vector3 objectPosition, Vector3 targetPosition, Vector3 upwardDirection, bool flip180 = false) {
    Vector3 objToTarget = -1f * (Camera.main.transform.position - objectPosition);
    Vector3 horizonRight = Vector3.Cross(upwardDirection, objToTarget);
    Vector3 objUp = Vector3.Cross(objToTarget.normalized, horizonRight.normalized);
    return Quaternion.LookRotation((flip180 ? -1 : 1) * objToTarget, objUp);
  }

  #endregion


  #region Float Utils

  public static float From(this float thisFloat, float otherFloat) {
    return thisFloat - otherFloat;
  }

  public static float Then(this float thisFloat, float otherFloat) {
    return thisFloat + otherFloat;
  }

  #endregion


  #region Matrix4x4 Utils

  public static Matrix4x4 From(this Matrix4x4 thisMatrix, Matrix4x4 otherMatrix) {
    return thisMatrix * otherMatrix.inverse;
  }

  public static Matrix4x4 Then(this Matrix4x4 thisMatrix, Matrix4x4 otherMatrix) {
    return otherMatrix * thisMatrix;
  }

  #endregion


  #region Vector3 Utils

  public static Vector3 From(this Vector3 thisVector, Vector3 otherVector) {
    // More commonly written as "thisVector - otherVector"!
    return -1 * otherVector + thisVector;
  }

  public static Vector3 Then(this Vector3 thisVector, Vector3 otherVector) {
    return thisVector + otherVector;
  }

  public static Vector3 Rotate(this Vector3 thisVector, Quaternion byQuaternion) {
    return byQuaternion * thisVector;
  }

  #endregion


  public static void SplitHorizontallyWithLeft(this Rect rect, out Rect left, out Rect right, float leftWidth) {
    left = rect;
    left.width = leftWidth;
    right = rect;
    right.x += left.width;
    right.width = rect.width - leftWidth;
  }

}
