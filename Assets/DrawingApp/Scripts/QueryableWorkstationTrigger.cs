using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class QueryableWorkstationTrigger : QueryableTrigger {
  
  [SerializeField]
  private WorkstationUI _lastQueriedWorkstationUI = null;

  [Tooltip("This event is fired if no WorkstationUI object is found within the QueryableTrigger's collider bounds.")]
  public UnityEvent OnQueryFailed;

  public void QueryForWorkstationUI() {
    _lastQueriedWorkstationUI = base.Query<WorkstationUI>();
    if (_lastQueriedWorkstationUI != null) {

    }
    else {
      OnQueryFailed.Invoke();
    }
  }

}
