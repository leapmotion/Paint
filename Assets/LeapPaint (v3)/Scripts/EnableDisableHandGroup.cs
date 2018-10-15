using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.ARTesting {

  public class EnableDisableHandGroup : MonoBehaviour {

    public HandModelManager handModelManager;

    [System.Serializable]
    public struct GroupToggle {
      public string name;
      public int toggleNumberKey;
    }
    public GroupToggle[] groupToggles;

    private void Reset() {
      if (handModelManager == null) handModelManager = GetComponent<HandModelManager>();
    }

    private void Update() {
      int alphaKey = -1;
      for (int i = 1; i <= 9; i++) {
        if (Input.GetKeyDown(i.ToString())) {
          alphaKey = i;
          break;
        }
      }

      if (alphaKey != -1) {
        foreach (var groupToggle in groupToggles) {
          if (alphaKey == groupToggle.toggleNumberKey) {
            handModelManager.ToggleGroup(groupToggle.name);
          }
        }
      }
    }

  }

}
