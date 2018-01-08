using UnityEngine;
using System.Collections;
using Leap.Unity.RuntimeGizmos;
using Leap.Unity;

public class UIActivator : MonoBehaviour, IRuntimeGizmoComponent {

  public float _radius = 1F;
  public HandModelBase _handModel;

  public float WorldRadius {
    get { return _radius * this.transform.lossyScale.x; }
  }

  public bool IsHandTracked {
    get { return _handModel.IsTracked; }
  }

  #region Gizmos

  private bool _drawGizmos = false;

  public void OnDrawRuntimeGizmos(RuntimeGizmoDrawer drawer) {
    if (_drawGizmos) {
      drawer.PushMatrix();
      drawer.matrix = this.transform.localToWorldMatrix;
      drawer.color = Color.blue;
      drawer.DrawWireSphere(Vector3.zero, _radius);
      drawer.PopMatrix();
    }
  }

  #endregion

}
