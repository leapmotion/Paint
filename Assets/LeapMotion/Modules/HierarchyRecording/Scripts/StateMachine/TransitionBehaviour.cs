using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Recording {

  public class TransitionBehaviour : MonoBehaviour {
    private static List<TransitionBehaviour> _transitionBehaviours = new List<TransitionBehaviour>();

    public GameObject destination;
    public GameObject transitionState;

    [ContextMenu("Trigger Transition")]
    public void Transition() {
      gameObject.SetActive(false);

      if (transitionState != null) {
        transitionState.GetComponents(_transitionBehaviours);
        foreach (var tb in _transitionBehaviours) {
          tb.destination = destination;
        }

        transitionState.SetActive(true);
      } else {
        destination.SetActive(true);
      }
    }
  }
}
