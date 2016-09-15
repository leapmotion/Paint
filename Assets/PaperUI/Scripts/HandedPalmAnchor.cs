using UnityEngine;
using System;
using Leap.Unity;

public class HandedPalmAnchor : MonoBehaviour {

  public Action<Chirality> OnAnchorChiralityChanged = (x) => { };

  [Header("Mirroring")]
  public Transform _leftPalm;
  public Transform _rightPalm;
  public Chirality _chirality = Chirality.Left;

  public Transform GetPalm(Chirality whichHand) {
    return (whichHand == Chirality.Left ? _leftPalm : _rightPalm);
  }

  public void SetChirality(Chirality whichHand) {
    if (whichHand == _chirality) {
      return;
    }
    else {
      Transform currentPalm;
      Transform targetPalm;
      if (_chirality == Chirality.Left) {
        currentPalm = _leftPalm;
        targetPalm = _rightPalm;
      }
      else {
        currentPalm = _rightPalm;
        targetPalm = _leftPalm;
      }

      Vector3 curLocal = transform.localPosition;
      this.transform.rotation = MirrorUtil.GetMirroredRotation(this.transform.rotation, currentPalm, targetPalm);
      this.transform.parent = targetPalm;
      this.transform.localPosition = new Vector3(-curLocal.x, curLocal.y, curLocal.z);

      _chirality = whichHand;
      OnAnchorChiralityChanged(_chirality);
    }
  }

}