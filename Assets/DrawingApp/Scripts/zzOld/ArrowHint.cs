using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ArrowHint : MonoBehaviour {

  public Sprite arrowSprite;
  public Color arrowColor;

  private bool _hintActive = false;

  private const int ARROWS_PER_WAVE = 3;
  private int _arrowsLaunchedThisWave = 0;
  private Image[] _arrows = new Image[ARROWS_PER_WAVE];
  private Image[] _activeArrows = new Image[ARROWS_PER_WAVE];
  private float[] _arrowClocks = new float[ARROWS_PER_WAVE];

  private float _arrowSpawnTime = 0.1F;
  private float _waveSpawnTime  = 0.5F; // a "wave" is a succession of ARROWS_PER_WAVE arrows

  private float _arrowSpawnTimer = 0F;
  private float _waveSpawnTimer  = 0F;

  private Vector3 _localStartPosition = Vector3.right * 1F;
  private Vector3 _localEndPosition   = Vector3.right * 125F;
  private float   _arrowTravelTime    = 0.5F;

  void Start() {
    for (int i = 0; i < ARROWS_PER_WAVE; i++) {
      _arrows[i] = new GameObject().AddComponent<Image>();
      _arrows[i].transform.SetParent(this.transform);
      _arrows[i].transform.localRotation = Quaternion.identity;
      _arrows[i].transform.localScale = Vector3.one * 0.5F;
      _arrows[i].sprite = arrowSprite;
      _arrows[i].color = new Color(arrowColor.r, arrowColor.g, arrowColor.b, 0F);
    }
  }

  void Update() {
    if (_hintActive) {

      if (_arrowsLaunchedThisWave < ARROWS_PER_WAVE) {
        _arrowSpawnTimer += Time.deltaTime;

        if (_arrowSpawnTimer >= _arrowSpawnTime) {
          // Spawn an arrow
          var spawnedArrow = _arrows[_arrowsLaunchedThisWave];
          spawnedArrow.color = new Color(spawnedArrow.color.r, spawnedArrow.color.g, spawnedArrow.color.b, 1F);
          spawnedArrow.transform.localPosition = _localStartPosition;
          _activeArrows[_arrowsLaunchedThisWave] = spawnedArrow;

          _arrowsLaunchedThisWave += 1;
          _arrowSpawnTimer = 0F;
        }
      }
      else {
        // waiting for new wave
        _waveSpawnTimer += Time.deltaTime;

        if (_waveSpawnTimer >= _waveSpawnTime) {
          // Start the wave by setting _arrowsLaunchedThisWave to zero
          _arrowsLaunchedThisWave = 0;
          _waveSpawnTimer = 0F;
        }
      }

      // Update currently active arrows
      for (int i = 0; i < _activeArrows.Length; i++) {
        var activeArrow = _activeArrows[i];
        if (activeArrow != null) {
          // progress time for the arrow
          _arrowClocks[i] += Time.deltaTime;

          // lerp position based on time
          activeArrow.transform.localPosition = Vector3.Lerp(_localStartPosition, _localEndPosition, _arrowClocks[i] / _arrowTravelTime);

          // set transparency based on (time OR position -- equivalent here)
          activeArrow.color = Color.Lerp(new Color(activeArrow.color.r, activeArrow.color.g, activeArrow.color.b, 1F),
                                         new Color(activeArrow.color.r, activeArrow.color.g, activeArrow.color.b, 0F),
                                         _arrowClocks[i] / _arrowTravelTime);

          // if it's done, make it inactive
          if (_arrowClocks[i] >= _arrowTravelTime) {
            activeArrow.color = new Color(activeArrow.color.r, activeArrow.color.g, activeArrow.color.b, 0F);
            _arrowClocks[i] = 0F;
            _activeArrows[i] = null;
          }
        }
      }

    }
  }

  public void ActivateHint() {
    _hintActive = true;
  }

  public void DeactivateHint() {
    _hintActive = false;

    // Reset wave / arrow clocks and make all arrows inactive
    for (int i = 0; i < ARROWS_PER_WAVE; i++) {
      _arrowClocks[i] = 0F;
      _activeArrows[i] = null;
      _arrows[i].color = new Color(_arrows[i].color.r, _arrows[i].color.g, _arrows[i].color.b, 0F);
    }
    _arrowsLaunchedThisWave = 0;
    _arrowSpawnTimer = 0F;
    _waveSpawnTimer = 0F;
  }

}
