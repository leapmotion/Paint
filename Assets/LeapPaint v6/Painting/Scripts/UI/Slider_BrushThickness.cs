using Leap.Unity.UserContext;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.LeapPaint {

  public class Slider_BrushThickness : UISlider {

    [Header("Ucon Channel Output")]
    public UserContextType context = UserContextType.Local;
    public string channel = "brush/radius";

    public override float GetStartingSliderValue() {
      return slider.defaultHorizontalValue;
    }

    public override void OnSliderValue(float value) {
      base.OnSliderValue(value);

      Ucon.C(context).At(channel).Set<float>(value);
    }

  }

}
