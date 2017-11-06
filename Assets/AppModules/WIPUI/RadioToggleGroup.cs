using Leap.Unity.Attributes;
using Leap.Unity.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioToggleGroup : MonoBehaviour {

  [EditTimeOnly]
  public List<InteractionToggle> toggles;
  
  private int _activeToggleIdx = 0;
  public int activeToggleIdx { get { return _activeToggleIdx; } }
  public InteractionToggle activeToggle { get { return toggles[activeToggleIdx]; } }

  public Action<int> OnIndexToggled = (idx) => { };

  void Awake() {
    for (int i = 0; i < toggles.Count; i++) {
      var toggle = toggles[i];

      int toggleIndex = i;
      toggle.OnToggle += () => {
        toggle.controlEnabled = false;
        onIndexToggled(toggleIndex);
      };

      for (int j = 0; j < toggles.Count; j++) {
        if (j == i) continue;

        var otherToggle = toggles[j];
        toggle.OnToggle += () => {
          otherToggle.controlEnabled = true;
          otherToggle.isToggled = false;
        };
      }
    }
  }

  private void onIndexToggled(int idx) {
    _activeToggleIdx = idx;

    OnIndexToggled(idx);
  }

}
