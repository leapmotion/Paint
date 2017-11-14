using Leap.Unity.Attributes;
using Leap.Unity.PhysicalInterfaces;
using Leap.Unity.Query;
using Leap.Unity.Space;
using Leap.Unity.WIPUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Launcher {

  public enum CurvatureType { Flat, Cylindrical, Spherical }

  public enum HandleType { Orb, Titlebar }

  public class PanelCommandReceiver : MonoBehaviour {

    #region Curvature Control

    [Header("Curvature Control")]

    public bool supportsCurvature = true;

    [SerializeField, OnEditorChange("curvatureType")]
    private CurvatureType _currentCurvatureType = CurvatureType.Spherical;
    public CurvatureType currentCurvatureType {
      get { return _currentCurvatureType; }
      set {
        if (value != _currentCurvatureType) {
          if (value == CurvatureType.Spherical) {
            if (sphericalPanelObject != null)   sphericalPanelObject.SetActive(true);
            if (cylindricalPanelObject != null) cylindricalPanelObject.SetActive(false);
            if (flatPanelObject != null)        flatPanelObject.SetActive(false);
          }
          else if (value == CurvatureType.Cylindrical) {
            if (sphericalPanelObject != null)   sphericalPanelObject.SetActive(false);
            if (cylindricalPanelObject != null) cylindricalPanelObject.SetActive(true);
            if (flatPanelObject != null)        flatPanelObject.SetActive(false);
          }
          else if (value == CurvatureType.Flat) {
            if (sphericalPanelObject != null)   sphericalPanelObject.SetActive(false);
            if (cylindricalPanelObject != null) cylindricalPanelObject.SetActive(false);
            if (flatPanelObject != null)        flatPanelObject.SetActive(true); 
          }

          _currentCurvatureType = value;
        }
      }
    }

    public GameObject flatPanelObject;

    [SerializeField, OnEditorChange("sphericalSpace")]
    private LeapSphericalSpace _sphericalSpace;
    public LeapSphericalSpace sphericalSpace {
      get { return _sphericalSpace; }
      set {
        _sphericalSpace = value;
        if (_sphericalSpace != null) {
          _sphericalSpace.radius = curvatureToRadius(curvatureAmount);
        }
      }
    }
    public GameObject sphericalPanelObject;

    [SerializeField, OnEditorChange("cylindricalSpace")]
    private LeapCylindricalSpace _cylindricalSpace;
    public LeapCylindricalSpace cylindricalSpace {
      get { return _cylindricalSpace; }
      set {
        _cylindricalSpace = value;
        if (_cylindricalSpace != null) {
          _cylindricalSpace.radius = curvatureToRadius(curvatureAmount);
        }
      }
    }
    public GameObject cylindricalPanelObject;

    [SerializeField, OnEditorChange("curvatureAmount")]
    private float _curvatureAmount = 0f;
    public float curvatureAmount {
      get { return _curvatureAmount; }
      set {
        value = Mathf.Clamp01(value);
        _curvatureAmount = value;
        if (sphericalSpace != null) {
          sphericalSpace.radius = curvatureToRadius(_curvatureAmount);
        }
        if (cylindricalSpace != null) {
          cylindricalSpace.radius = curvatureToRadius(_curvatureAmount);
        }
      }
    }

    private float curvatureToRadius(float curvatureAmount) {
      return Mathf.Sqrt(curvatureAmount).Map(0f, 1f, 2f, 0.2f);
    }

    /// <summary>
    /// Returns normalized curvature amount.
    /// </summary>
    public float GetCurvature() {
      return curvatureAmount;
    }
    
    /// <summary>
    /// Sets the amount of curvature and sets the radius appropriately of an attached
    /// LeapRadialSpace.
    /// </summary>
    public void SetCurvature(float normalizedCurvatureAmount) {
      curvatureAmount = normalizedCurvatureAmount;
    }

    public CurvatureType GetCurvatureType() {
      return _currentCurvatureType;
    }

    public void SetCurvatureType(CurvatureType type) {
      currentCurvatureType = type;
    }

    #endregion

    #region Held Orientability

    [Header("Held Orientability")]

    public MonoBehaviour[] lockOrientationComponents;

    public HeldOrientabilityType GetHeldOrientabilityType() {
      if (lockOrientationComponents.Query().Any(c => c.enabled)) {
        return HeldOrientabilityType.LockedFacing;
      }

      return HeldOrientabilityType.Free;
    }

    public void SetHeldOrientabilityType(HeldOrientabilityType type) {
      switch (type) {
        case HeldOrientabilityType.Free:
          foreach (var component in lockOrientationComponents) {
            component.enabled = false;
          }
          break;
        case HeldOrientabilityType.LockedFacing:
          foreach (var component in lockOrientationComponents) {
            component.enabled = true;
          }
          break;
      }
    }

    #endregion

    #region Held Orientability - NYI

    //[Header("Held Orientability - NYI")]

    public HeldVisiblityType GetHeldVisiblityType() {
      return HeldVisiblityType.Hide;
    }

    public void SetHeldVisiblityType(HeldVisiblityType type) {
      return;
    }

    #endregion

    #region Handle Switching

    [Header("Handle Switching")]

    [SerializeField, OnEditorChange("currentHandleType")]
    private HandleType _currentHandleType = HandleType.Orb;
    public HandleType currentHandleType {
      get { return _currentHandleType; }
      set {
        if (value != _currentHandleType) {
          if (value == HandleType.Orb) {
            if (orbHandle as MonoBehaviour != null) {
              (orbHandle as MonoBehaviour).gameObject.SetActive(true);
            }
            if (titlebarHandle as MonoBehaviour != null) {
              (titlebarHandle as MonoBehaviour).gameObject.SetActive(false);
            }
          }
          else if (value == HandleType.Titlebar) {
            if (titlebarHandle as MonoBehaviour != null) {
              (titlebarHandle as MonoBehaviour).gameObject.SetActive(true);
            }
            if (orbHandle as MonoBehaviour != null) {
              (orbHandle as MonoBehaviour).gameObject.SetActive(false);
            }
          }

          _currentHandleType = value;
        }
      }
    }

    private MonoBehaviour curHandleBehaviour {
      get {
        switch (_currentHandleType) {
          case HandleType.Orb:
            return _orbHandle;
          case HandleType.Titlebar:
          default:
            return _titlebarHandle;
        }
      }
    }

    [SerializeField, ImplementsInterface(typeof(IHandle))]
    private MonoBehaviour _orbHandle;
    public IHandle orbHandle {
      get {
        return _orbHandle as IHandle;
      }
    }

    [SerializeField, ImplementsInterface(typeof(IHandle))]
    private MonoBehaviour _titlebarHandle;
    public IHandle titlebarHandle {
      get {
        return _titlebarHandle as IHandle;
      }
    }

    public HandleType GetHandleType() {
      return currentHandleType;
    }

    public void SetHandleType(HandleType type) {
      currentHandleType = type;
    }

    #endregion


  }

}
