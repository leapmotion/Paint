using UnityEngine;
using System.Collections;

public class TutorialTip : MonoBehaviour {
  public EmergeableBehaviour UIElement;
  public TextMesh text;

  private bool hasShownOnce = false;

	void Start () {
    text.gameObject.SetActive(false);
    UIElement.OnFinishedEmerging += DoOnFinishedEmerging;
    UIElement.OnBegunVanishing += DoOnBegunVanishing;
	}

  void DoOnFinishedEmerging() {
    if (!hasShownOnce) {
      text.gameObject.SetActive(true);
      hasShownOnce = true;
    }
  }

  void DoOnBegunVanishing() {
    text.gameObject.SetActive(false);
  }



	// Update is called once per frame
	void Update () {
	
	}
}
