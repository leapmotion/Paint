using UnityEngine;
using System.Collections;

public class Follower : MonoBehaviour {

  public Transform toFollow;

  [Range(0F, 1F)]
  [Tooltip("Following is implemented each frame as A.Lerp(A, B, <snapCoefficient>). 1 means instantaneous following, 0 will result in no following at all.")]
  public float snapCoefficient = 1F;

  [Tooltip("Follow target rotation is offset by this Euler rotation.")]
  public Vector3 axisRotationOffsets = new Vector3(90F, 180F, 0F);

  private bool _followingEnabled = true;

  protected void Update() {
    if (_followingEnabled) {
      this.transform.position = Vector3.Lerp(this.transform.position, toFollow.transform.position, snapCoefficient);
      this.transform.rotation = Quaternion.Slerp(this.transform.rotation, toFollow.transform.rotation, snapCoefficient);
    }
  }

  public void EnableFollow() {
    _followingEnabled = true;
  }

  public void DisableFollow() {
    _followingEnabled = false;
  }


}
