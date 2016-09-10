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

      this.transform.position = MirrorUtil.GetMirroredPosition(this.transform.position, currentPalm, targetPalm);
      this.transform.rotation = MirrorUtil.GetMirroredRotation(this.transform.rotation, currentPalm, targetPalm);
      this.transform.parent = targetPalm;
      _chirality = whichHand;
      OnAnchorChiralityChanged(_chirality);
    }
  }

}