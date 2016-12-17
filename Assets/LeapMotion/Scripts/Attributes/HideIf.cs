using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace Leap.Unity.Attributes {

  public class HideIf : CombinablePropertyAttribute, IPropertyHider {
    public readonly string propertyName;
    public readonly object testValue;
    public readonly bool hideResult;

    /// <summary>
    /// Conditionally hides a property based on the value of another property.  The only condition
    /// types that are currently supported are bool types, and enum types.  The property has two arguments
    /// names 'equalTo' and 'notEqualTo'.  Exactly one of them must be specified, like so:
    /// 
    /// [DisableIf("myBoolProperty", isEqualTo: true)]
    /// [DisableIf("myEnumProperty", isNotEqualTo: MyEnum.Value)]
    /// </summary>
    public HideIf(string propertyName, object isEqualTo = null, object isNotEqualTo = null) {
      this.propertyName = propertyName;

      if ((isEqualTo != null) == (isNotEqualTo != null)) {
        throw new ArgumentException("Must specify exactly one of 'equalTo' or 'notEqualTo'.");
      }

      if (isEqualTo != null) {
        testValue = isEqualTo;
        hideResult = true;
      }
      else if (isNotEqualTo != null) {
        testValue = isNotEqualTo;
        hideResult = false;
      }

      if (!(testValue is bool) && !(testValue is Enum)) {
        throw new ArgumentException("Only values of bool or Enum are allowed in comparisons using DisableIf.");
      }
    }

#if UNITY_EDITOR
    public bool ShouldHide(SerializedProperty property) {
      SerializedProperty prop = property.serializedObject.FindProperty(propertyName);

      if (prop.propertyType == SerializedPropertyType.Boolean) {
        return (prop.boolValue == (bool)testValue) == hideResult;
      }
      else if (prop.propertyType == SerializedPropertyType.Enum) {
        return (prop.intValue == (int)testValue) == hideResult;
      }
      else {
        Debug.LogError("Can only conditionally disable based on boolean or enum types.");
        return false;
      }
    }
#endif
  }
}
