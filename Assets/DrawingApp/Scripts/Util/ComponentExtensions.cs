using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public static class ComponentExtensions {

  public static T CopyProperties<T>(this Component comp, T other) where T : Component {
    Type type = comp.GetType();
    if (type != other.GetType()) return null; // type mis-match
    BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
    PropertyInfo[] pinfos = type.GetProperties(flags);
    foreach (var pinfo in pinfos) {
      if (pinfo.CanWrite) {
        try {
          pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
        }
        catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
      }
    }
    FieldInfo[] finfos = type.GetFields(flags);
    foreach (var finfo in finfos) {
      finfo.SetValue(comp, finfo.GetValue(other));
    }
    return comp as T;
  }

  /// <summary>
  /// Extension method for AddComponent that clones the argument component properties into the created component. (Types must match.)
  /// </summary>
  public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component {
    return go.AddComponent<T>().CopyProperties(toAdd) as T;
  }

}
