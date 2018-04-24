using Leap.Unity.GraphicalRenderer;
using UnityEngine;

namespace Leap.Unity.LeapPaint {

  public class SetTextWhenEnabled : MonoBehaviour {

    public LeapTextGraphic textGraphic;

    public string textWhenEnabled = "Enabled";
    public string textWhenDisabled = "Disabled";

    private void OnEnable() {
      textGraphic.text = textWhenEnabled;
    }

    private void OnDisable() {
      textGraphic.text = textWhenDisabled;
    }

  }

}
