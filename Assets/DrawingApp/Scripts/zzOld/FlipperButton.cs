using UnityEngine;
using System.Collections;
using Leap.Unity;
using UnityEngine.Events;

public class FlipperButton : MonoBehaviour {

  public Camera centerEyeAnchor;

  public UnityEvent OnFlipped;

  #region PRIVATE ATTRIBUTES

  private bool _buttonEnabled = true;

  private Rigidbody _collidingHandBody;
  private int _handCollidersCount = 0;

  #endregion

  #region UNITY CALLBACKS

  void OnEnable() {
    Debug.Log("HI");
    _handCollidersCount = 0;
  }

  void OnTriggerEnter(Collider other) {
    _collidingHandBody = other.GetComponentInParent<Rigidbody>();
    if (gameObject.activeInHierarchy && _collidingHandBody != null && other.GetComponentInParent<RigidHand>() != null) {

      _handCollidersCount += 1;

      // Only actually UNDO when a hand part enters the trigger and has velocity away from the camera
      if (Vector3.Dot(_collidingHandBody.velocity, (centerEyeAnchor.transform.position - _collidingHandBody.position)) < 0F) {

        if (_buttonEnabled) {
          Debug.Log("UNDOing.");
          OnFlipped.Invoke();
          _buttonEnabled = false;
        }
      }
    }
  }

  void OnTriggerExit(Collider other) {
    _handCollidersCount -= 1;
    if (_handCollidersCount == 0) {
      _buttonEnabled = true;
    }
  }

  void Update() {

  }

  #endregion

}
