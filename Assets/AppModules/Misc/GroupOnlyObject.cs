using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity {

  [ExecuteInEditMode]
  public class GroupOnlyObject : MonoBehaviour {

    private void Start() {
#if UNITY_EDITOR
      var components = GetComponents<Component>();
      foreach (var component in components) {
        if (!(component is Transform
              || component is RectTransform
              || component is GroupOnlyObject)) {
          Debug.LogError("GroupOnlyObject components can only be added to empty "
                       + "Transforms. (Found component: " + component.GetType().Name + ")",
                       this.gameObject);
          DestroyImmediate(this);
          break;
        }
      }
#endif
    }

    private void Update() {
#if UNITY_EDITOR
      var components = GetComponents<Component>();
      foreach (var component in components) {
        if (!(component is Transform
              || component is RectTransform
              || component is GroupOnlyObject)) {
          Debug.LogError("The GroupOnlyObject component requires this object ("
                         + this.name + ") be empty -- it is a grouping object only.",
                         this);
          DestroyImmediate(component);
        }
      }

      //var localPosition = this.transform.localPosition;
      //var localRotation = this.transform.localRotation;
      //var localScale    = this.transform.localScale;

      //if (localPosition != Vector3.zero
      //    || localRotation != Quaternion.identity
      //    || localScale != Vector3.one) {
      //  Debug.LogError("Grouping-only objects must have identity transforms. Groups are "
      //               + "intended to be a hierarchy convenience only, not normal "
      //               + "transform container objects.", this.transform);
      //  this.transform.localPosition = Vector3.zero;
      //  this.transform.localRotation = Quaternion.identity;
      //  this.transform.localScale    = Vector3.one;
      //}
#endif
    }

  }

}