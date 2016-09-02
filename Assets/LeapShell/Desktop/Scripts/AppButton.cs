using UnityEngine;
using UnityEngine.UI;

public class AppButton : CurvedButton {

  [Header("App Settings")]
  [SerializeField]
  protected AppGrabbable _grabbablePrefab;

  [SerializeField]
  private Text _titleText;

  private AppData _data;

  public AppGrabbable InstantiateGrabbableIcon() {
    var grabbable = Instantiate(_grabbablePrefab) as AppGrabbable;
    grabbable.InitGrabbable(_data);
    grabbable.gameObject.SetActive(true);
    return grabbable;
  }

  public void InitButton(AppData appData) {
    _data = appData;
    _titleText.text = appData.sprite.name;

    _iconRenderer.sprite = appData.sprite;
    _shadowRenderer.sprite = appData.sprite;
  }

  public override void SetAlpha(float alpha) {
    base.SetAlpha(alpha);
    _titleText.color = new Color(1, 1, 1, alpha);
  }
}
