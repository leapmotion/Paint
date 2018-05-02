using Leap.Unity.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace Leap.Unity.LeapPaint_v3 {

  public class TextFader : MonoBehaviour {

    [Header("Required")]
    public Text uiText;

    [Header("Animation")]
    public float durationPerCharacter = 0.1f;
    [MinValue(0.001f)]
    public float fadeTime = 0.5f;
    public Vector3 fadedPositionOffset = Vector3.down * 0.1f;
    public AnimationCurve fadeCurve = DefaultCurve.SigmoidUp;
  
    private enum TextFadeState { FadingIn, Staying, FadingOut, Hidden }
    private TextFadeState _currState = TextFadeState.Hidden;
    private string _targetText = "";
    private float _currT = 0f;
    private float _stayTime = 0f;

    private Vector3? _backingStartPosition = null;
    private Vector3 _startPosition {
      get {
        if (!_backingStartPosition.HasValue) {
          if (uiText != null) {
            _backingStartPosition = uiText.transform.position;
          }
          else {
            return this.transform.position; // Doesn't set _backingStartPosition.
          }
        }
        return _backingStartPosition.Value;
      }
    }

    private string _currText {
      get { return uiText.text; }
      set { uiText.text = value; }
    }

    private void Reset() {
      if (uiText == null) uiText = GetComponent<Text>();
    }

    private void Update() {
      updateTransitions();

      // Progress animation time.
      _currT = Mathf.Clamp01(_currT + Time.deltaTime);

      updateVisualState();
    }

    private const float HIDDEN_TIME = 0.020f;
    private void updateTransitions() {
      // Staying -> FadingOut
      // This can occur by a natural wait OR if the target text has changed.
      if (_currState == TextFadeState.Staying &&
          (_currT >= _stayTime || !_currText.Equals(_targetText))) {
        _currState = TextFadeState.FadingOut;
        _currT = 0f;
      }

      // FadingOut -> Hidden
      if (_currState == TextFadeState.FadingOut && _currT >= fadeTime) {
        _currState = TextFadeState.Hidden;
        _currT = 0f;
      }

      // Hidden -> FadingIn
      // Requires a new target text, this also calculates the "stay time" for the new
      // target text and changes the current text.
      if (_currState == TextFadeState.Hidden && !_currText.Equals(_targetText)) {
        _currText = _targetText;
        _stayTime = _currText.Length * durationPerCharacter;

        // Only transition to "fading in" if the text to display has content.
        // This prevents a delay where empty text has to fade in and out before new
        // text can come in.
        if (!string.IsNullOrEmpty(_currText)) {
          _currState = TextFadeState.FadingIn;
          _currT = 0f;
        }
      }

      // FadingIn -> Staying
      if (_currState == TextFadeState.FadingIn && _currT >= fadeTime) {
        _currState = TextFadeState.Staying;
        _currT = 0f;
      }
    }

    private void updateVisualState() {
      if (uiText == null) return;

      var alpha = 1f;
      var offset = Vector3.zero;
      var evalT = fadeCurve.Evaluate(_currT / fadeTime);
      switch (_currState) {
        case TextFadeState.Hidden:
          alpha = 0f;
          offset = fadedPositionOffset;
          break;
        case TextFadeState.FadingIn:
          alpha = Mathf.Lerp(0f, 1f, evalT);
          offset = Vector3.Lerp(fadedPositionOffset, Vector3.zero, evalT);
          break;
        case TextFadeState.Staying:
          alpha = 1f;
          offset = Vector3.zero;
          break;
        case TextFadeState.FadingOut:
          alpha = Mathf.Lerp(1f, 0f, evalT);
          offset = Vector3.Lerp(Vector3.zero, fadedPositionOffset, evalT);
          break;
      }

      uiText.color = uiText.color.WithAlpha(alpha);
      uiText.transform.position = _startPosition + offset;
    }

    public void SetText(string text) {
      _targetText = text;
    }

    public void ClearText() {
      _targetText = "";
    }

  }

}
