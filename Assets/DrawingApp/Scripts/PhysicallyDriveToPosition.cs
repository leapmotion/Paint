using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class PhysicallyDriveToPosition : MonoBehaviour {

  public WorkstationUI _workstationUI;
  public Rigidbody _rigidbody;

  public float _driveForce = 1F;

  public void DriveToReasonablePositionUsingWorkstationUI() {

    Vector3 worldPositionToDriveTo = _workstationUI.GetReasonableWorkstationPosition();

    DriveToPosition(worldPositionToDriveTo);

  }

  private Vector3 _desiredPosition;
  private bool _driving = false;

  public void DriveToPosition(Vector3 position) {
    _desiredPosition = position;
    _driving = true;
  }

  public void StopDriving() {
    _driving = false;
  }

  private float _drag = 0F;
  private float _distanceToStartDrag = 1F;

  protected void Update() {
    if (_driving) {

      Vector3 deltaVec = _desiredPosition - _rigidbody.position;
      float curDistance = deltaVec.magnitude;

      if (curDistance > 0.0005F) {
        _rigidbody.drag = Mathf.Max(0F, (_distanceToStartDrag / curDistance) - 1F);
        _rigidbody.AddForce(_driveForce * deltaVec.normalized);
      }
      else {
        _driving = false;
        OnFinishedDriving.Invoke();
      }
    }
  }

  public UnityEvent OnFinishedDriving;

}
