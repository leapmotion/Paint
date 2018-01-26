using System;
using System.Reflection;
using System.Linq.Expressions;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Leap.Unity.Recording {
  using Query;

#if UNITY_EDITOR

  /// <summary>
  /// This struct mimics the function of AnimationUtility.GetFloatValue.
  /// It trades construct time for access time, once the accessor is 
  /// constructed, access is roughly 25 times faster compared to GetFloatValue.
  /// </summary>
  public struct AnimationPropertyAccessor {
    private static MethodInfo GetFloatMethod;
    private static MethodInfo GetColorMethod;
    private static MethodInfo GetVectorMethod;

    static AnimationPropertyAccessor() {
      Type[] intArr = new Type[] { typeof(int) };
      GetFloatMethod = typeof(Material).GetMethod("GetFloat", intArr);
      GetColorMethod = typeof(Material).GetMethod("GetColor", intArr);
      GetVectorMethod = typeof(Material).GetMethod("GetVector", intArr);
    }

    private Func<float> _accessor;

    public AnimationPropertyAccessor(GameObject target, EditorCurveBinding binding) {
      string[] names = binding.path.Split('/');

      foreach (var name in names) {
        for (int i = 0; i < target.transform.childCount; i++) {
          var child = target.transform.GetChild(i);
          if (child.gameObject.name == name) {
            target = child.gameObject;
            break;
          }
        }
      }

      Component component = target.GetComponent(binding.type);
      Expression propertyExpr = Expression.Constant(component);

      string[] propertyPath = binding.propertyName.Split('.');

      if (propertyPath[0] == "material" && component is Renderer) {
        //## Special path for materials

        Material material = (component as Renderer).sharedMaterial;
        if (material == null) {
          _accessor = () => 0;
          Debug.LogError("Could not record property because material was null", component);
          return;
        }

        Shader shader = material.shader;
        if (shader == null) {
          _accessor = () => 0;
          Debug.LogError("Could not record property because shader was null", component);
          return;
        }

        string propertyName = propertyPath[1];
        ShaderUtil.ShaderPropertyType? propertyType = null;

        int shaderPropCount = ShaderUtil.GetPropertyCount(shader);
        for (int i = 0; i < shaderPropCount; i++) {
          if (ShaderUtil.GetPropertyName(shader, i) == propertyName) {
            propertyType = ShaderUtil.GetPropertyType(shader, i);
            break;
          }
        }

        if (!propertyType.HasValue) {
          _accessor = () => 0;
          Debug.LogError("Could not find property " + propertyName + " in shader " + shader);
          return;
        }

        var idExpr = Expression.Constant(Shader.PropertyToID(propertyName));
        var matExpr = Expression.Property(propertyExpr, "sharedMaterial");
        switch (propertyType.Value) {
          case ShaderUtil.ShaderPropertyType.Float:
          case ShaderUtil.ShaderPropertyType.Range:
            propertyExpr = Expression.Call(matExpr, GetFloatMethod, idExpr);
            break;
          case ShaderUtil.ShaderPropertyType.Color:
            propertyExpr = Expression.Call(matExpr, GetColorMethod, idExpr);
            break;
          case ShaderUtil.ShaderPropertyType.Vector:
            propertyExpr = Expression.Call(matExpr, GetVectorMethod, idExpr);
            break;
          default:
            _accessor = () => 0;
            Debug.LogError("Could not handle property type " + propertyType.Value, component);
            return;
        }

        for (int i = 2; i < propertyPath.Length; i++) {
          propertyExpr = Expression.PropertyOrField(propertyExpr, propertyPath[i]);
        }
      } else {
        //## Special path for fields/properties

        for (int i = 0; i < propertyPath.Length; i++) {
          string propertyName;
          if (i == 0) {
            propertyName = getClosestPropertyName(binding.type, propertyPath[i]);
          } else {
            propertyName = propertyPath[i];
          }

          propertyExpr = Expression.PropertyOrField(propertyExpr, propertyName);
        }
      }

      var lambda = Expression.Lambda<Func<float>>(propertyExpr);
      _accessor = lambda.Compile();
    }

    public float Access() {
      return _accessor();
    }

    private static string getClosestPropertyName(Type type, string bindingProperty) {
      BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

      var members = type.GetFields(flags).
                         Query().
                         Cast<MemberInfo>().
                         Concat(type.GetProperties(flags).
                                     Query().
                                     Cast<MemberInfo>()).
                                     ToList();

      {
        MemberInfo exactMatch = members.Query().FirstOrDefault(f => f.Name == bindingProperty);
        if (exactMatch != null) {
          return exactMatch.Name;
        }
      }

      {
        MemberInfo minusPrefix = members.Query().FirstOrDefault(f => bindingProperty.Substring(2).ToLower() == f.Name.ToLower());
        if (minusPrefix != null) {
          return minusPrefix.Name;
        }
      }

      {
        MemberInfo containsMatch = members.Query().FirstOrDefault(f => bindingProperty.ToLower().Contains(f.Name.ToLower()));
        if (containsMatch != null) {
          return containsMatch.Name;
        }
      }

      return null;
    }
  }
#endif
}
