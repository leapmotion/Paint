using UnityEngine;
using System.Collections.Generic;

namespace Leap.Unity.LeapPaint_v3 {


  public static class GameObjectExtensions {

    public static T FindClosest<T>(this GameObject obj, List<T> candidates, out float distance) where T : MonoBehaviour {
      T closestCandidate = null;
      float closestCandidateDistance = float.PositiveInfinity;
      for (int i = 0; i < candidates.Count; i++) {
        if (closestCandidate != null) {
          float tempDistance = Vector3.Distance(candidates[i].transform.position, obj.transform.position);
          if (tempDistance < closestCandidateDistance) {
            closestCandidate = candidates[i];
            closestCandidateDistance = tempDistance;
          }
        }
        else {
          closestCandidate = candidates[i];
          closestCandidateDistance = Vector3.Distance(closestCandidate.transform.position, obj.transform.position);
        }
      }
      distance = closestCandidateDistance;
      return closestCandidate;
    }

    public static T FindClosest<T>(this GameObject obj, List<T> candidates) where T : MonoBehaviour {
      float distance = 0F;
      return FindClosest<T>(obj, candidates, out distance);
    }

  }


}
