using UnityEngine;

namespace Leap.Unity.Recording {

  public class OnUnityCallback : MonoBehaviour {

    [SerializeField]
    private EnumEventTable _table;

    private void Awake() {
      _table.Invoke((int)CallbackType.Awake);
    }

    private void Start() {
      _table.Invoke((int)CallbackType.Start);
    }

    private void OnEnable() {
      _table.Invoke((int)CallbackType.OnEnable);
    }

    private void OnDisable() {
      _table.Invoke((int)CallbackType.OnDisable);
    }

    private void OnDestroy() {
      _table.Invoke((int)CallbackType.OnDestroy);
    }

    private void FixedUpdate() {
      _table.Invoke((int)CallbackType.FixedUpdate);
    }

    private void Update() {
      _table.Invoke((int)CallbackType.Update);
    }

    private void LateUpdate() {
      _table.Invoke((int)CallbackType.LateUpdate);
    }

    public enum CallbackType {
      Awake,
      Start,
      OnEnable,
      OnDisable,
      OnDestroy,
      FixedUpdate,
      Update,
      LateUpdate
    }
  }
}
