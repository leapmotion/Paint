using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.PhysicalInterfaces {

  public class LODItemController : MonoBehaviour {

    [Range(5f, 180f)]
    public float maxCameraLookAngle = 30f;

    [Range(0.10f, 1f)]
    public float maxDetailDistance = 0.30f;

    //public bool smoothDetailFade = false;
    //public bool oneAtATime = true;

    private List<LODItem> items = new List<LODItem>();

    [Header("Debug")]
    public bool drawDebug = false;

    private int counter = 0;

    private void Update() {

      GetComponentsInChildren<LODItem>(items);

      var camera = Camera.main;

      counter += 1;
      var pingThisFrame = false;
      if (counter % 10 == 0) {
        counter = 0;
        if (drawDebug) {
          pingThisFrame = true;
        }
      }

      var selector = GetComponent<PullTabSelector>();

      var closestAngle = float.PositiveInfinity;
      LODItem closestItem = null;
      foreach (var item in items) {
        var testAngle = Vector3.Angle(camera.transform.forward,
                                      item.transform.position - camera.transform.position);
        var testDist = Vector3.Distance(item.transform.position, camera.transform.position);


        if (pingThisFrame) {
          DebugPing.Ping(item.transform.position, LeapColor.blue, 0.08f);

          Debug.Log(testAngle);
        }

        if (testAngle < closestAngle
            && testAngle <= maxCameraLookAngle
            && testDist <= maxDetailDistance) {
          
          closestAngle = testAngle;
          
          if (selector != null && selector.listOpenCloseAmount < 0.10f) {
            var activeMarbleItem = selector.activeMarbleParent.GetComponentInChildren<LODItem>();
            if (item == activeMarbleItem) {
              closestItem = item;
            }
          }
          else {
            closestItem = item;
          }

          if (pingThisFrame) {
            DebugPing.Ping(item.transform.position, LeapColor.red, 0.09f);
          }
        }
      }


      foreach (var item in items) {
        if (item != closestItem && item.propertySwitch != null
          && item.propertySwitch.GetIsOnOrTurningOn()) {
          item.propertySwitch.Off();
        }
      }
      if (closestItem != null && closestItem.propertySwitch != null
          && closestItem.propertySwitch.GetIsOffOrTurningOff()) {
        closestItem.propertySwitch.On();
      }
      if (pingThisFrame && closestItem != null) {
        DebugPing.Ping(closestItem.transform.position, LeapColor.purple, 0.10f);
      }
    }

  }

}
