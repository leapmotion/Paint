using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class FileSelectionClaimer : MonoBehaviour, IPointerDownHandler {

  public FileBrowser _toClaimSelectionFrom;

  public void ClaimSelection() {
    _toClaimSelectionFrom.Select(this);
  }

  public virtual void OnPointerDown(PointerEventData eventData) {
    ClaimSelection();
  }

}
