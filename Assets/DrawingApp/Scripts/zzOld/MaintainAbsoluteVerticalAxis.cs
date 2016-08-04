using UnityEngine;
using System.Collections;
using Leap.Unity;

public class MaintainAbsoluteVerticalAxis : MonoBehaviour {

  public AttachmentController handAttachmentController;

  void Update() {
    this.transform.rotation = Quaternion.Euler(0F, handAttachmentController.transform.rotation.eulerAngles.y, 0F);
  }

}
