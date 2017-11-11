using Leap.Unity.Geometry;
using Leap.Unity.RuntimeGizmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Leap.Unity.Interaction;

using Receiver = Leap.Unity.Launcher.PanelCommandReceiver;
using Leap.Unity.WIPUI;

namespace Leap.Unity.Launcher {

  public class PanelCommandController : PeriodicBehaviour, IRuntimeGizmoComponent {

    public Sphere visibilitySphere;
    public Point  controllerPoint;

    private void Reset() {
      visibilitySphere = new Sphere(0.10f, this);
      controllerPoint = new Point(this);
    }

    protected virtual void Awake() {
      base.updatePeriod = 4;
    }

    public ActivityManager<Receiver> receiverQueryManager
      = new ActivityManager<Receiver>(
        0.10f,
        (c) => c.GetComponentInParent<Receiver>()
        );

    private Maybe<Receiver> _lastKnownReceiver = Maybe.None;

    public override void PeriodicUpdate() {
      receiverQueryManager.activationRadius = visibilitySphere.radius;

      receiverQueryManager.UpdateActivityQuery(visibilitySphere.center);

      Receiver closestReceiver = null;
      float closestSqrDist = float.PositiveInfinity;
      foreach (var receiver in receiverQueryManager.ActiveObjects) {
        float testSqrDist = (receiver.transform.position
                             - controllerPoint.pos).sqrMagnitude;
        if (testSqrDist < closestSqrDist) {
          closestReceiver = receiver;
          testSqrDist = closestSqrDist;
        }
      }

      if (_lastKnownReceiver != closestReceiver) {
        initializeWithNewReceiver(closestReceiver);

        _lastKnownReceiver = closestReceiver;
      }
    }

    public InteractionSlider curvatureAmountSlider;
    public RadioToggleGroup  curvatureTypeToggles;
    public RadioToggleGroup  heldOrientabilityToggles;
    public RadioToggleGroup  heldVisibilityToggles;
    public RadioToggleGroup  handleTypeToggles;

    private Receiver _attachedReceiver;

    private void initializeWithNewReceiver(Receiver receiver) {
      if (receiver == null) {
        renderLossOfSignal();

        curvatureAmountSlider.controlEnabled = false;
        curvatureTypeToggles.controlsEnabled = false;
        heldOrientabilityToggles.controlsEnabled = false;
        heldVisibilityToggles.controlsEnabled = false;
        handleTypeToggles.controlsEnabled = false;
      }
      else {
        renderNewReceiverGained(receiver);

        curvatureAmountSlider.controlEnabled = true;
        curvatureTypeToggles.controlsEnabled = true;
        heldOrientabilityToggles.controlsEnabled = true;
        heldVisibilityToggles.controlsEnabled = true;
        handleTypeToggles.controlsEnabled = true;

        // Initialize sliders and radio buttons with the settings from the receiver.
        curvatureAmountSlider.normalizedHorizontalValue = receiver.GetCurvature();
        curvatureTypeToggles.toggles[(int)receiver.GetCurvatureType()].Toggle();
        heldOrientabilityToggles.toggles[(int)receiver.GetHeldOrientabilityType()].Toggle();
        heldVisibilityToggles.toggles[(int)receiver.GetHeldVisiblityType()].Toggle();
        handleTypeToggles.toggles[(int)receiver.GetHandleType()].Toggle();
      }

      _attachedReceiver = receiver;
    }

    protected override void Update() {
      base.Update();

      if (_attachedReceiver != null) {
        // If the user interacted with any of the sliders or buttons this frame,
        // update the current receiver with those values.

        if (curvatureAmountSlider.wasSlid) {
          _attachedReceiver.SetCurvature(
            curvatureAmountSlider.normalizedHorizontalValue);
        }
        if (curvatureTypeToggles.wasToggled) {
          _attachedReceiver.SetCurvatureType(
            (CurvatureType)curvatureTypeToggles.activeToggleIdx);
        }
        if (heldOrientabilityToggles.wasToggled) {
          _attachedReceiver.SetHeldOrientabilityType(
            (HeldOrientabilityType)heldOrientabilityToggles.activeToggleIdx);
        }
        if (heldVisibilityToggles.wasToggled) {
          _attachedReceiver.SetHeldVisiblityType(
            (HeldVisiblityType)heldVisibilityToggles.activeToggleIdx);
        }
        if (handleTypeToggles.wasToggled) {
          _attachedReceiver.SetHandleType(
            (HandleType)handleTypeToggles.activeToggleIdx);
        }
      }

    }

    private Color renderColor = Color.Lerp(LeapColor.cerulean, Color.cyan, 0.7f);

    private void renderLossOfSignal() {
      // TODO: Make this look nicer.
      DebugPing.Ping(controllerPoint, Color.black);
    }

    private void renderNewReceiverGained(Receiver receiver) {
      // TODO: Make this look nicer.
      DebugPing.Ping(controllerPoint, renderColor);
      DebugPing.Ping(receiver.transform.position, renderColor);
    }

    public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
      controllerPoint.RenderSphere(renderColor, 3f);
      controllerPoint.Render(renderColor, 3f);

      var subtleColor = renderColor.WithAlpha(0.4f);
      visibilitySphere.Render(subtleColor);
    }
  }

}
