using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class StageConfiguration : MonoBehaviour {

  public StageSetting _stageSetting = StageSetting.Neutral;
  public Material _skyboxMat;
  public Material _fogMat;
  public Material _islandMat;
  public Material _groundMat;
  public Material _ringsMat;

  [Header("Dark Configuration")]
  [Range(0F, 20F)]
  public float _darkSunIntensity = 0.39f;
  public Color _darkSky;
  public Color _darkHorizon;
  public Color _darkFog;
  public Color _darkIsland;
  public Color _darkGround;
  public Color _darkRings;

  [Header("Neutral Configuration")]
  [Range(0F, 20F)]
  public float _neutralSunIntensity = 0.39F;
  public Color _neutralSky;
  public Color _neutralHorizon;
  public Color _neutralFog;
  public Color _neutralIsland;
  public Color _neutralGround;
  public Color _neutralRings;

  [Header("Light Configuration")]
  [Range(0F, 20F)]
  public float _lightSunIntensity = 0.39F;
  public Color _lightSky;
  public Color _lightHorizon;
  public Color _lightFog;
  public Color _lightIsland;
  public Color _lightGround;
  public Color _lightRings;

  public float SunIntensity {
    get {
      switch (_stageSetting) {
        case StageSetting.Dark: return _darkSunIntensity;
        case StageSetting.Light: return _lightSunIntensity;
        case StageSetting.Neutral:
        default: return _neutralSunIntensity;
      }
    }
  }

  public Color SkyColor {
    get {
      switch (_stageSetting) {
        case StageSetting.Dark: return _darkSky;
        case StageSetting.Light: return _lightSky;
        case StageSetting.Neutral:
        default: return _neutralSky;
      }
    }
  }

  public Color HorizonColor {
    get {
      switch (_stageSetting) {
        case StageSetting.Dark: return _darkHorizon;
        case StageSetting.Light: return _lightHorizon;
        case StageSetting.Neutral:
        default: return _neutralHorizon;
      }
    }
  }

  public Color FogColor {
    get {
      switch (_stageSetting) {
        case StageSetting.Dark: return _darkFog;
        case StageSetting.Light: return _lightFog;
        case StageSetting.Neutral:
        default: return _neutralFog;
      }
    }
  }

  public Color IslandColor {
    get {
      switch (_stageSetting) {
        case StageSetting.Dark: return _darkIsland;
        case StageSetting.Light: return _lightIsland;
        case StageSetting.Neutral:
        default: return _neutralIsland;
      }
    }
  }

  public Color GroundColor {
    get {
      switch (_stageSetting) {
        case StageSetting.Dark: return _darkGround;
        case StageSetting.Light: return _lightGround;
        case StageSetting.Neutral:
        default: return _neutralGround;
      }
    }
  }

  public Color RingsColor {
    get {
      switch (_stageSetting) {
        case StageSetting.Dark: return _darkRings;
        case StageSetting.Light: return _lightRings;
        case StageSetting.Neutral:
        default: return _neutralRings;
      }
    }
  }

  void OnValidate() {
    SetStageColors(_stageSetting);
  }

  void Start() {
    SetStageColors(_stageSetting);
  }

  public void SetStageDark() {
    SetStageColors(StageSetting.Dark);
  }

  public void SetStageNeutral() {
    SetStageColors(StageSetting.Neutral);
  }

  public void SetStageLight() {
    SetStageColors(StageSetting.Light);
  }

  public void SetStageColors(StageSetting stageSetting) {
    _stageSetting = stageSetting;
    _skyboxMat.SetFloat(Shader.PropertyToID("_SunIntensity"), SunIntensity);
    _skyboxMat.SetColor(Shader.PropertyToID("_SkyColor1"), SkyColor);
    _skyboxMat.SetColor(Shader.PropertyToID("_SkyColor2"), HorizonColor);
    _fogMat.SetColor(Shader.PropertyToID("_Color"), FogColor);
    RenderSettings.fogColor = FogColor;
    _islandMat.color = IslandColor;
    _groundMat.color = GroundColor;
    _ringsMat.color = RingsColor;
  }

}

public enum StageSetting {
  Dark,
  Neutral,
  Light
}
