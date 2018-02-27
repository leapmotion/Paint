using UnityEngine;
using System.Collections;
using Leap.Unity.Animation;

namespace Leap.Unity.LemurUI {

  public class LabelTextMeshDriver : LabelDriver<TextMesh> {

    public Label label;
    public TextMesh textMesh;

    public LabelTextMeshDriver() {
      textMesh = driven.gameObject.GetComponent<TextMesh>();

      Updater.instance.OnUpdate += onUpdate;
    }

    public override void Bind(Label label) {
      this.label = label;
    }

    private void onUpdate() {
      if (label == null) return;
      if (textMesh == null) return;

      textMesh.text = label.text;
    }

  }

}
