using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Recording.Examples {
  using Interaction;

  public class TransitionOnButtonPress : TransitionBehaviour {

    [SerializeField]
    private InteractionButton _button;

    private void OnEnable() {
      _button.OnPress += onPress;
    }

    private void OnDisable() {
      _button.OnPress -= onPress;
    }

    private void onPress() {
      Transition();
    }
  }
}
