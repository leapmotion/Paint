using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Launcher {

  public enum CurvatureType { Flat, Cylindrical, Spherical }

  public enum HeldOrientabilityType { LockedFacing, SingleAxis, Free }

  public enum HeldVisiblityType { Hide, StayOpen }

  public enum HandleType { Orb, Titlebar }

  public class PanelCommandReceiver : MonoBehaviour {

    #region Curvature Amount

    /// <summary>
    /// Returns normalized curvature amount.
    /// </summary>
    public float GetCurvature() {
      throw new NotImplementedException();
    }
    
    public void SetCurvature(float normalizedCurvatureAmount) {

    }

    #endregion

    #region Curvature Type

    public CurvatureType GetCurvatureType() {
      throw new NotImplementedException();
    }

    public void SetCurvatureType(CurvatureType type) {

    }

    #endregion

    #region Held Orientability

    public HeldOrientabilityType GetHeldOrientabilityType() {
      throw new NotImplementedException();
    }

    public void SetHeldOrientabilityType(HeldOrientabilityType type) {

    }

    #endregion

    #region Held Visibility

    public HeldVisiblityType GetHeldVisiblityType() {
      throw new NotImplementedException();
    }

    public void SetHeldVisiblityType(HeldVisiblityType type) {

    }

    #endregion

    #region Handle Type

    public HandleType GetHandleType() {
      throw new NotImplementedException();
    }

    public void SetHandleType(HandleType type) {

    }

    #endregion


  }

}
